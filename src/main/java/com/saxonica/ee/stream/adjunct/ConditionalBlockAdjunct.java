package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ConditionalBlock;
import net.sf.saxon.expr.instruct.OnEmptyExpr;
import net.sf.saxon.expr.instruct.OnNonEmptyExpr;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;


/**
 * Generate streaming code for a conditional Block, that is a sequence constructor containing
 * xsl:on-empty and/or xsl:on-not-empty instructions
 */
public class ConditionalBlockAdjunct extends TransmissionAdjunct implements FeedMaker {

    private int consumingArg = -1;

    /**
     * Get a WatchMaker, a factory class that creates a Watch to evaluate this expression
     * in streaming mode. This method will only be called if the getImplementationMethod()
     * method of the expression includes the value {@link net.sf.saxon.expr.Expression#WATCH_METHOD}
     *
     * @return a Watchmaker to evaluate the expression, or null if it is not streamable with respect
     *         to the specificed argument.
     * @throws net.sf.saxon.trans.XPathException
     *          in the event of a failure
     * @param forGrouping true if the selected items are being grouped
     */
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return super.getWatchMaker(forGrouping);
    }

    public static class ConditionalBlockFeed extends ItemFeed {

        private final int consumingArg;
        private boolean empty = true;
        private int level = 0;
        private boolean watchForDocumentChildren;

        private final boolean consumingArgIsOnEmpty;
        private final List<OnNonEmptyExpr> nonEmptyPendingList = new ArrayList<>();
        private final List<OutputterEvent> tentativeContent = new ArrayList<>();

        ConditionalBlockFeed(ConditionalBlock exp, int arg, ItemFeed result, XPathContext context) {
            super(exp, result, context);
            this.consumingArg = arg;
            consumingArgIsOnEmpty = exp.getChildExpression(arg) instanceof OnEmptyExpr;
        }

        public List<OutputterEvent> getTentativeContent() {
            return tentativeContent;
        }

        public void flushIfNonEmpty() throws XPathException {
            if (!empty && !tentativeContent.isEmpty()) {
                Outputter out = getNextOutputter();
                for (OutputterEvent event : tentativeContent) {
                    event.replay(out);
                }
                tentativeContent.clear();
            }
        }

        private void action() throws XPathException {
            if (level == 0 || (level == 1 && watchForDocumentChildren)) {
                empty = false;
            }
            flushIfNonEmpty();
        }


        public boolean isEmpty() {
            return empty;
        }

        private boolean isSignificant(Item item) {
            return SignificantItemDetector.isSignificant(item);
        }

        /**
         * Open the watch. This is done at the point where it first starts watching for nodes
         * that match the selection.
         * @param terminator used for early exit
         */

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            ConditionalBlock block = (ConditionalBlock)getExpression();
            Outputter result = getNextOutputter();
            for (int i = 0; i < consumingArg; i++) {
                Expression child = block.getChildExpression(i);
                //noinspection StatementWithEmptyBody
                if (child instanceof OnEmptyExpr) {
                    // No action until close
                } else if (child instanceof OnNonEmptyExpr) {
                    if (empty) {
                        OutputterEventBuffer temp = new OutputterEventBuffer();
                        temp.setBuffer(tentativeContent);
                        SequenceIterator iter = child.iterate(getContext());
                        processItems(iter, temp);
                    } else {
                        SequenceIterator iter = child.iterate(getContext());
                        processItems(iter, result);
                    }
                } else {
                    SequenceTool.supply(child.iterate(getContext()), (ItemConsumer<? super Item>) item -> {
                        if (empty && isSignificant(item)) {
                            empty = false;
                            flushIfNonEmpty();
                        }
                        result.append(item);
                    });
                }
            }
        }

        /**
         * The close() method is called immediately before the Watch is destroyed, that is,
         * when the element whose declaration scopes the constraint implemented by this
         * Watch goes out of scope. The default implementation does nothing.
         */

        @Override
        public void close() throws XPathException {
            if (consumingArgIsOnEmpty) {
                // In assessing whether any items have been written, disregard any items written
                // by previous xsl:on-empty instructions
                empty = true;
            }
            ConditionalBlock block = (ConditionalBlock)getExpression();
            Outputter result = getNextOutputter();
            for (int i = consumingArg + 1; i < block.size(); i++) {
                Expression child = block.getChildExpression(i);
                if (child instanceof OnEmptyExpr) {
                    if (empty) {
                        //emptyPendingList.add((OnEmptyExpr) child);
                        SequenceIterator si = child.iterate(getContext());
                        processItems(si, result);
                    }
                } else if (child instanceof OnNonEmptyExpr) {
                    if (empty) {
                        if (i != block.size() - 1) {
                            OutputterEventBuffer temp = new OutputterEventBuffer();
                            temp.setBuffer(tentativeContent);
                            SequenceIterator iter = child.iterate(getContext());
                            processItems(iter, temp);
                        }
                    } else {
                        processItems(child.iterate(getContext()), result);
                    }
                } else {
                    SequenceTool.supply(child.iterate(getContext()), (ItemConsumer<? super Item>) item -> {
                        if (isSignificant(item)) {
                            empty = false;
                            flushIfNonEmpty();
                        }
                        result.append(item);
                    });
                }
            }
            result.close();
        }

//        private void flushTentativeContent(Outputter out) throws XPathException {
//            if (tentativeContent != null && !tentativeContent.isEmpty()) {
//                for (Item it : tentativeContent) {
//                    out.append(it);
//                }
//                tentativeContent = null;
//                empty = false;
//            }
//            for (Expression e : nonEmptyPendingList) {
//                processItems(e.iterate(getContext()), out);
//            }
//        }

//        /**
//         * Signal that a node (other than a document or element node) that matches the selection
//         * has been found, or that a grounded item is being passed to the next expression in the
//         * evaluation pipeline.
//         *
//         * @param item the node that was matched.
//         * @throws net.sf.saxon.trans.XPathException
//         *          May be raised if a dynamic error occurs
//         */
//        public void append(Item item) throws XPathException {
//            if (empty && isSignificant(item)) {
//                empty = false;
//                for (OnNonEmptyExpr e : nonEmptyPendingList) {
//                    SequenceIterator si = e.iterate(getContext());
//                    processItems(si, getNextOutputter());
//                }
//            }
//            getNextOutputter().append(item);
//        }

        /**
         * Signal that an element or document node has been found that matches the selection that this
         * Watch is looking for. This method is called by the WatchManager while
         * processing the startElement or startDocument event that matches the selection.
         *
         * @param node       the element or document node whose start event has been matched
         * @param locationId the location associated with the element or document node (may be
         *                   the location of the instruction that created it)
         * @return a Receiver to be notified of all events starting with the startElement/startDocument event
         *         for the matched element, and ending with the endElement event for that element; or null if this
         *         feature is not required.
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a dynamic error occurs
         */
        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            if (empty) {
                empty = false;
                for (OnNonEmptyExpr e : nonEmptyPendingList) {
                    SequenceIterator si = e.iterate(getContext());
                    processItems(si, getNextOutputter());
                }
            }
            return getResultFeed().startSelectedParentNode(node, locationId);
        }

        /**
         * Signal that the endElement event has occurred for the element whose startElement
         * event caused the Watch to be activated.
         *
         * @param locationId the location of the element
         * @throws net.sf.saxon.trans.XPathException
         *          May be raised if a constraint implemented by this
         *          Watch is violated
         */
        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            getResultFeed().endSelectedParentNode(locationId);
        }

        @Override
        public void startDocument(int properties) throws XPathException {
            if (level == 0) {
                watchForDocumentChildren = true;
            }
            level++;
            getNextOutputter().startDocument(properties);
        }

        @Override
        public void endDocument() throws XPathException {
            level--;
            watchForDocumentChildren = false;
            getNextOutputter().endDocument();
        }

        @Override
        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
            action();
            level++;
            getNextOutputter().startElement(elemName, typeCode, location, properties);
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
            action();
            level++;
            getNextOutputter().startElement(elemName, type, attributes, namespaces, location, properties);
        }

        @Override
        public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
            action();
            getNextOutputter().namespace(prefix, namespaceUri, properties);
        }

        @Override
        public void endElement() throws XPathException {
            level--;
            getNextOutputter().endElement();
        }

        @Override
        public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
            if (ReceiverOption.contains(properties,ReceiverOption.SEPARATOR) && isEmpty() && chars.toString().equals(" ")) {
                // A whitespace separator between atomic values does not constitute significant
                // content for assessing xsl:on-empty and xsl:on-non-empty. Treat it as if generated
                // using xsl:on-non-empty. Test case si-on-non-empty-044
                tentativeContent.add(new OutputterEvent.Text(StringConstants.SINGLE_SPACE, location, properties));
            } else {
                action();
                getNextOutputter().characters(chars, location, properties);
            }
        }

        @Override
        public void comment(UnicodeString content, Location location, int properties) throws XPathException {
            action();
            getNextOutputter().comment(content, location, properties);
        }

        @Override
        public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
            action();
            getNextOutputter().attribute(attName, typeCode, value, location, properties);
        }

        @Override
        public void append(Item item) throws XPathException {
            if (item instanceof StringValue && ((StringValue) item).isEmpty()) {
                if (isEmpty()) {
                    tentativeContent.add(new OutputterEvent.Append(item, Loc.NONE, ReceiverOption.NONE));
                } else {
                    getNextOutputter().append(item);
                }
            } else {
                action();
                getNextOutputter().append(item);
            }
        }


    }


    @Override
    public ItemFeed makeItemFeed(final WatchManager watchManager, ItemFeed out, final XPathContext context) throws XPathException {
        consumingArg = findConsumingArg(getExpression(), consumingArg);
        return new ConditionalBlockFeed((ConditionalBlock)getExpression(),  consumingArg, out, context);
    }



}

// Copyright (c) 2018-2022 Saxonica Limited