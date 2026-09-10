package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
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
        if (Streamability.getSweep(select) == Sweep.CONSUMING) {
            // The select expression drills down
            final Inversion ie = Inversion.invertExpression(select, forGrouping);
            final Inversion route = ie.copy();
            Inversion.PushAction letAction = new Inversion.PushAction();
            letAction.feedMaker = new FeedMakerForConsumingSelect(((LetExpression) getExpression()));
            letAction.pushMethod = Expression.ITEM_FEED_METHOD;
            letAction.streamingChildSequence = 0;
            route.prepend(letAction);
            return CSharp.methodRef(route::getWatch);
        } else {

            // If the action operand consumes the input stream, use LetExpressionAdjunctB, which handles
            // this case by extending TransmissionAdjunct.
            StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), getExpression());
            return sa.getWatchMaker(forGrouping);
        }
    }

    private static class FeedMakerForConsumingSelect implements FeedMaker {

        public final LetExpression letExpr;

        public FeedMakerForConsumingSelect(LetExpression letExpr) {
            this.letExpr = letExpr;
        }

        @Override
        public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
            return new ItemFeedForConsumingSelect(letExpr, out, context);
        }
    }

    private static class ItemFeedForConsumingSelect extends ItemFeed {

        private SequenceCollector collector;
        private ComplexContentOutputter cco;

        public ItemFeedForConsumingSelect(Expression letExpr, ItemFeed out, XPathContext context) {
            super(letExpr, out, context);
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            super.open(terminator);
            collector = new SequenceCollector(getResultFeed().getPipelineConfiguration());
            cco = new ComplexContentOutputter(collector);
        }

        @Override
        public void startDocument(int properties) throws XPathException {
            cco.startDocument(properties);
        }

        @Override
        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
            cco.startElement(elemName, typeCode, location, properties);
        }

        @Override
        public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
            cco.startElement(elemName, type, attributes, namespaces, location, properties);
        }

        @Override
        public void endDocument() throws XPathException {
            cco.endDocument();
        }

        @Override
        public void namespace(String prefix, NamespaceUri namespaceUri, int properties) throws XPathException {
            cco.namespace(prefix, namespaceUri, properties);
        }

        @Override
        public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
            cco.attribute(attName, typeCode, value, location, properties);
        }

        @Override
        public void endElement() throws XPathException {
            cco.endElement();
        }

        @Override
        public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
            cco.characters(chars, location, properties);
        }

        @Override
        public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
            cco.processingInstruction(name, data, location, properties);
        }

        @Override
        public void comment(UnicodeString content, Location location, int properties) throws XPathException {
            cco.comment(content, location, properties);
        }

        @Override
        public void append(Item item) throws XPathException {
            cco.append(item);
        }

        @Override
        public void close() throws XPathException {
            LetExpression letExpr = (LetExpression)getExpression();
            GroundedValue value = collector.getSequence();
            XPathContext context = getContext();
            context.setLocalVariable(letExpr.getLocalSlotNumber(), value);
            letExpr.getAction().process(getResultFeed(), context);
            super.close();
        }

    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        LetExpression expr = (LetExpression)getExpression();
        if (expr.getSequence() instanceof ContextItemExpression &&
                contextInfo.getContextItemPosture() != Posture.GROUNDED) {
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

// Copyright (c) 2010-2022 Saxonica Limited