package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

import java.util.List;


/**
 * Compiles streaming code to implement a Let expression
 */
public class LetExpressionAdjunct extends StreamingAdjunct {


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        final Expression select = ((LetExpression) getExpression()).getSequence();
        final Expression action = ((LetExpression) getExpression()).getAction();
        if (Streamability.getSweep(select) == Sweep.CONSUMING) {
            // The select expression drills down
            final Inversion ie = Inversion.invertExpression(select, forGrouping);
            final Inversion route = ie.copy();
            Inversion.PushAction letAction = new Inversion.PushAction();
            letAction.feedMaker = new FeedMaker() {
                @Override
                public ItemFeed makeItemFeed(final WatchManager watchManager, final ItemFeed out, final XPathContext context) throws XPathException {
                    return new ItemFeed(getExpression(), out, context) {
                        private SequenceCollector collector;
                        private ComplexContentOutputter writer;

                        @Override
                        public void open(Terminator terminator) throws XPathException {
                            super.open(terminator);
                            collector = new SequenceCollector(watchManager.getPipelineConfiguration());
                            writer = new ComplexContentOutputter(collector);
                        }

                        @Override
                        public void startDocument( int properties) throws XPathException {
                            writer.startDocument(properties);
                        }

                        @Override
                        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
                            writer.startElement(elemName, typeCode, location, properties);
                        }

                        @Override
                        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
                            writer.startElement(elemName, type, attributes, namespaces, location, properties);
                        }

                        @Override
                        public void endDocument() throws XPathException {
                            writer.endDocument();
                        }

                        @Override
                        public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
                            writer.namespace(prefix, namespaceUri, properties);
                        }

                        @Override
                        public void attribute(NodeName attName, SimpleType typeCode, CharSequence value, Location location, int properties) throws XPathException {
                            writer.attribute(attName, typeCode, value, location, properties);
                        }

                        @Override
                        public void endElement() throws XPathException {
                            writer.endElement();
                        }

                        @Override
                        public void characters(CharSequence chars, Location location, int properties) throws XPathException {
                            writer.characters(chars, location, properties);
                        }

                        @Override
                        public void processingInstruction(String name, CharSequence data, Location location, int properties) throws XPathException {
                            writer.processingInstruction(name, data, location, properties);
                        }

                        @Override
                        public void comment(CharSequence content, Location location, int properties) throws XPathException {
                            writer.comment(content, location, properties);
                        }

                        @Override
                        public void append(Item item) throws XPathException {
                            writer.append(item);
                        }

                        @Override
                        public void close() throws XPathException {
                            Sequence value = collector.getSequence();
                            XPathContext context = getContext();
                            context.setLocalVariable(((LetExpression) getExpression()).getLocalSlotNumber(), value);
                            action.process(out, context);
                            super.close();
                        }

//                        @Override
//                        public Outputter getOutputter() {
//                            return new ComplexContentOutputter(writer);
//                        }
                    };
                }
            };
            letAction.pushMethod = Expression.ITEM_FEED_METHOD;
            letAction.streamingChildSequence = 0;
            route.prepend(letAction);
            return new WatchMaker() {
                @Override
                public Trigger makeWatch(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
                    return route.getWatch(watchManager, out, context);
                }
            };
        } else {

            // If the action operand consumes the input stream, use LetExpressionAdjunctB, which handles
            // this case by extending TransmissionAdjunct.
            StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(
                getConfiguration(), getExpression());
            return sa.getWatchMaker(forGrouping);

            // The action expression drills down
//            final Inversion ie = Inversion.invertExpression(action, false);
//            final Inversion route = ie.copy();
//            Inversion.PushAction letAction = new Inversion.PushAction();
//            letAction.feedMaker = new FeedMaker() {
//                public Feed makeItemFeed(WatchManager watchManager, final Feed out, final XPathContext context) throws XPathException {
//                    return new ItemFeed(getExpression(), out, context) {
//
//                        private XPathContext context = getContext();
//
//                        @Override
//                        public void open(Terminator terminator) throws XPathException {
//                            super.open(terminator);
//                            try {
//                                //Sequence value = SequenceExtent.makeSequenceExtent(select.iterate(context));
//                                Sequence value = ((LetExpression)getExpression()).eval(context);
//                                context.setLocalVariable(((LetExpression) getExpression()).getLocalSlotNumber(), value);
//                            } catch (XPathException e) {
//                                dynamicError(e);
//                            }
//                        }
//
//                        @Override
//                        public void processItem(Item item) throws XPathException {
//                            if (!hasFailed()) {
//                                context.getReceiver().append(item);
//                            }
//                        }
//
//                        /**
//                         * Signal that an element or document node has been found that matches the selection that this
//                         * Watch is looking for. This method is called by the WatchManager while
//                         * processing the startElement or startDocument event that matches the selection.
//                         *
//                         * @param node       the element or document node whose start event has been matched
//                         * @param locationId the location associated with the element or document node (may be
//                         *                   the location of the instruction that created it)
//                         * @return a Receiver to be notified of all events starting with the startElement/startDocument event
//                         * for the matched element, and ending with the endElement event for that element; or null if this
//                         * feature is not required.
//                         * @throws net.sf.saxon.trans.XPathException May be raised if a dynamic error occurs
//                         */
//                        public Receiver startSelectedParentNode(FleetingParentNode node, int locationId) throws XPathException {
//                            return getResult().startSelectedParentNode(node, locationId);
//                        }
//
//                        /**
//                         * Signal that the endElement event has occurred for the element whose startElement
//                         * event caused the Watch to be activated.
//                         *
//                         * @param locationId the location of the element
//                         * @throws net.sf.saxon.trans.XPathException May be raised if a constraint implemented by this
//                         *                                           Watch is violated
//                         */
//                        public void endSelectedParentNode(int locationId) throws XPathException {
//                            getResult().endSelectedParentNode(locationId);
//                        }
//                    };
//                }
//            };
//            letAction.pushMethod = Expression.ITEM_FEED_METHOD;
//            letAction.streamingChildSequence = 0;
//            route.prepend(letAction);
//            return new WatchMaker() {
//                public Trigger makeWatch(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
//                    return route.getWatch(watchManager, out, context);
//                }
//            };
        }
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        LetExpression expr = (LetExpression)getExpression();
        if (expr.getSequence() instanceof ContextItemExpression &&
                ((ContextItemStaticInfoEE)contextInfo).getContextItemPosture() != Posture.GROUNDED) {
            // this is typically binding a variable to implement the current() function. We assess the usages of the bound
            // variable, and if all usages are INSPECTION, we change the operand usage to INSPECTION to make it streamable
            if (!hasNonInspectionUsage(expr, expr.getAction())) {
                // all usages of the variable are inspection usages
                expr.getSequenceOp().setUsage(OperandUsage.INSPECTION);
            }
        }
        return super.computeStreamability(contextInfo, reasons);
    }

    private boolean hasNonInspectionUsage(LetExpression binding, Expression child) {
        for (Operand o : child.operands()) {
            if (o.getChildExpression() instanceof VariableReference) {
                if (((VariableReference) o.getChildExpression()).getBinding() == binding &&
                    o.getUsage() != OperandUsage.INSPECTION) {
                    return true;
                } else if (hasNonInspectionUsage(binding, o.getChildExpression())) {
                    return true;
                }
            } else {
                if (hasNonInspectionUsage(binding, o.getChildExpression())) {
                    return true;
                }
            }
        }
        return false;
    }
}

// Copyright (c) 2010-2020 Saxonica Limited