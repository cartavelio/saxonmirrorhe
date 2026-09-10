package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ComplexNodeEventFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Copy;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NameOfNode;
import net.sf.saxon.om.NoElementsSpaceStrippingRule;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;

/**
 * Compile an xsl:copy expression/instruction to streaming code.
 * Used only for XSLT streaming capability.
 */

public class CopyAdjunct extends StreamingAdjunct implements FeedMaker {


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new CopyFeed((Copy) getExpression(), out, context);
    }

    private static class CopyFeed extends ComplexNodeEventFeed {
        XPathContext context;
        int nodeKind;


        public CopyFeed(Copy expression, ItemFeed result, XPathContext context) {
            super(expression, result, context);
        }

        @Override
        protected void processLeft(XPathContext context) throws XPathException {
            Item item = context.getContextItem();
            Copy expression = (Copy) getExpression();
            if (item instanceof FleetingParentNode) {
                FleetingParentNode node = (FleetingParentNode) item;
                nodeKind = node.getNodeKind();
                switch (nodeKind) {
                    case Type.DOCUMENT: {

                        if (!expression.isPreservingTypes()) {
                            Controller controller = context.getController();
                            Configuration config = controller.getConfiguration();
                            ParseOptions options = new ParseOptions(expression.getValidationOptions());
                            options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
                            Receiver val = config.getDocumentValidator(
                                    next, node.getBaseURI(), options, expression.getLocation());
                            next = new ComplexContentOutputter(val);
                        }

                        next.startDocument(ReceiverOption.NONE);
                        break;
                    }
                    case Type.ELEMENT: {
                        NodeName elemName = NameOfNode.makeName(node);
                        SchemaType typeCode = expression.getValidationAction() == Validation.PRESERVE ? AnyType.getInstance() : Untyped.getInstance();
                        int properties =
                                expression.isBequeathNamespacesToChildren()
                                        ? ReceiverOption.NONE
                                        : ReceiverOption.DISINHERIT_NAMESPACES;

                        if (!expression.isPreservingTypes()) {
                            Controller controller = context.getController();
                            assert controller != null;
                            ParseOptions options = new ParseOptions(expression.getValidationOptions());
                            options.setTopLevelElement(elemName.getStructuredQName());
                            try {
                                Receiver val = context.getConfiguration().getElementValidator(
                                        next, options, expression.getLocation());
                                next = new ComplexContentOutputter(val);
                            } catch (ValidationException e) {
                                dynamicError(e);
                            }
                        }
                        if (next.getSystemId() == null) {
                            next.setSystemId(expression.getNewBaseURI(context, node));
                        }

                        next.startElement(elemName, typeCode,
                                              expression.getLocation(), properties);

                        // output the required namespace nodes via a callback
                        expression.outputNamespaceNodes(next, elemName, node);

                        break;
                    }
                    default:
                        try {
                            TailCall tc = expression.processLeavingTail(next, context, node);
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
                //cco.revertTo(originalReceiver);
                if (nodeKind == Type.DOCUMENT) {
                    next.endDocument();
                } else if (nodeKind == Type.ELEMENT) {
                    next.endElement();
                }
            } catch (XPathException e) {
                dynamicError(e);
            }
        }

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            return null;
        }

    }


}

// Copyright (c) 2009-2022 Saxonica Limited