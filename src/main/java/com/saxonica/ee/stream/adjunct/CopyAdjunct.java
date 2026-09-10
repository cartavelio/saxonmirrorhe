package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ComplexNodeEventFeed;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.event.TreeReceiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Copy;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

/**
 * Compile an xsl:copy expression/instruction to streaming code.
 * Used only for XSLT streaming capability.
 */

public class CopyAdjunct extends StreamingAdjunct implements FeedMaker {


    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new CopyFeed(watchManager, (Copy) getExpression(), out, context);
    }

    private static class CopyFeed extends ComplexNodeEventFeed {
        WatchManager watchManager;
        XPathContext context;
        int nodeKind;
        Receiver receiver;

        public CopyFeed(WatchManager watchManager, Copy expression, Feed result, XPathContext context) throws XPathException {
            super(expression, result, context);
            this.watchManager = watchManager;
        }

        @Override
        protected void processLeft(XPathContext context) throws XPathException {
            Item item = context.getContextItem();
            Copy expression = (Copy) getExpression();
            if (item instanceof FleetingParentNode) {
                NodeInfo node = (NodeInfo) item;
                nodeKind = node.getNodeKind();
                switch (nodeKind) {
                    case Type.DOCUMENT: {
                        receiver = getResult().getReceiver();
                        int properties = 0;
                        if (!expression.isPreservingTypes()) {
                            Controller controller = context.getController();
                            Configuration config = controller.getConfiguration();
                            ParseOptions options = new ParseOptions(expression.getValidationOptions());
                            options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
                            receiver = new TreeReceiver(config.getDocumentValidator(
                                    receiver, node.getBaseURI(), options, expression.getLocation()));
                        } else if (!(receiver instanceof TreeReceiver)) {
                            receiver = new TreeReceiver(receiver);
                        }

                        receiver.startDocument(properties);
                        break;
                    }
                    case Type.ELEMENT: {
                        NodeName elemName = NameOfNode.makeName(node);
                        SchemaType typeCode = expression.getValidationAction() == Validation.PRESERVE ? AnyType.getInstance() : Untyped.getInstance();
                        int properties = expression.isInheritNamespacesToChildren() ? 0 : ReceiverOptions.DISINHERIT_NAMESPACES;

                        receiver = getResult().getReceiver();
                        if (!expression.isPreservingTypes()) {
                            Controller controller = context.getController();
                            assert controller != null;
                            ParseOptions options = new ParseOptions(expression.getValidationOptions());
                            options.setTopLevelElement(elemName.getStructuredQName());
                            try {
                                receiver = context.getConfiguration().getElementValidator(
                                        receiver, options, expression.getLocation());
                            } catch (ValidationException e) {
                                dynamicError(e);
                            }
                        }
                        if (!(receiver instanceof TreeReceiver)) {
                            receiver = new TreeReceiver(receiver);
                        }
                        if (receiver.getSystemId() == null) {
                            receiver.setSystemId(expression.getNewBaseURI(context, node));
                        }

                        receiver.startElement(elemName, typeCode, expression.getLocation(), properties);

                        // output the required namespace nodes via a callback

                        expression.outputNamespaceNodes(context, receiver, elemName, node);

                        break;
                    }
                    default:
                        try {
                            TailCall tc = expression.processLeavingTail(context, node);
                            while (tc != null) {
                                tc = tc.processLeavingTail();
                            }
                        } catch (XPathException err) {
                            err.maybeSetLocation(expression.getLocation());
                            throw err;
                        }
                        break;
                }
            }
        }

        @Override
        protected void processRight(XPathContext context) throws XPathException {
            try {
                if (nodeKind == Type.DOCUMENT) {
                    receiver.endDocument();
                } else if (nodeKind == Type.ELEMENT) {
                    receiver.endElement();
                }
            } catch (XPathException e) {
                dynamicError(e);
            }
        }

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            return null;
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void processItem(Item<?> item) throws XPathException {
            if (!hasFailed()) {
                if (receiver == null) {
                    receiver = getResult().getReceiver();
                }
                try {
                    receiver.append(item);
                } catch (XPathException e) {
                    dynamicError(e);
                }
            }
        }

        /**
         * Get a receiver to which start and end events for documents and elements can be
         * written. By default this will be the same as the receiver for the result feed,
         * because very few XSLT constructs (specifically, variables and functions) allow
         * the results of document and element constructors to be captured for further
         * processing. A construct that does need to process these events will intercept
         * them by providing its own receiver, typically a SequenceOutputter.
         */
        @Override
        public Receiver getReceiver() {
            return receiver;
        }
    }


}

// Copyright (c) 2009-2016 Saxonica Limited.