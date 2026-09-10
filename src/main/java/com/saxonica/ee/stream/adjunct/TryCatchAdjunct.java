package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.TryCatch;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;

/**
 * Streaming code for a try/catch expression
 */
public class TryCatchAdjunct extends TransmissionAdjunct implements FeedMaker {

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new TryCatchFeed(getExpression(), out, context);
    }

    public static class TryCatchFeed extends ItemFeed {

        private final ItemFeed originalResult;
        private boolean done = false;
        private boolean opened = false;

        TryCatchFeed(Expression exp, ItemFeed out, XPathContext context) {
            super(new ComplexContentOutputter(new SequenceCollector(out.getPipelineConfiguration())), context, true);
            setExpression(exp);
            originalResult = out;
        }

        /**
         * Start evaluating the expression. The default implementation does nothing.
         *
         * @param terminator used to achieve early exit
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        public void open(Terminator terminator) throws XPathException {
            originalResult.open(terminator);
            getNextOutputter().open();
            opened = true;
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {
            getNextOutputter().append(item);
        }

        /**
         * Finish evaluating the expression.
         *
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        public void close() throws XPathException {
            ComplexContentOutputter out = (ComplexContentOutputter)getNextOutputter();
            SequenceCollector collector = (SequenceCollector)out.getReceiver();
            collector.close();
            try {
                for (Item i : collector.getList()) {
                    originalResult.append(i);
                }
            } catch (XPathException e) {
                dynamicError(e);
            }
            originalResult.close();
        }

        @Override
        public void dynamicError(XPathException err) throws XPathException {
            if (err.isGlobalError()) {
                err.setIsGlobalError(false);
            } else if (done) {
                // validation errors in particular may be reported more than once; we only process them the first time
                return;
            } else {
                if (!opened) {
                    open(new Terminator());
                }
                done = true;
                StructuredQName code = err.getErrorCodeQName();
                for (TryCatch.CatchClause clause : ((TryCatch)getExpression()).getCatchClauses()) {
                    if (code == null) {
                        // fallback, use a code that's only likely to be matched by catch="*"
                        code = new StructuredQName("saxon", NamespaceConstant.SAXON, "XXXX9999");
                    }
                    if (clause.nameTest.matches(code)) {
                        Expression caught = clause.catchOp.getChildExpression();
                        XPathContextMajor c2 = getContext().newContext();
                        c2.setCurrentException(err);
                        SequenceIterator iter = caught.iterate(c2);
                        processItems(iter, originalResult);
                        originalResult.close();
                        return;
                    }
                }
            }
            err.setHasBeenReported(false);
            throw err;
        }
    }
}

// Copyright (c) 2010-2022 Saxonica Limited