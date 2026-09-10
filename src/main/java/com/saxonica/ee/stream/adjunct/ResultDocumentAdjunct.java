package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.ComplexNodeEventFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FeedMakerDelegate;
import com.saxonica.ee.stream.feed.ItemFeed;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ResultDocument;
import net.sf.saxon.trans.XPathException;

/**
 * Handle an xsl:result-document expression/instruction for XSLT streaming capability.
 */

public class ResultDocumentAdjunct extends ParentConstructorAdjunct {


    @Override
    public FeedMaker getFeedMaker(final int arg) throws XPathException {
        ResultDocument inst = (ResultDocument)getExpression();
        if (Streamability.getSweep(inst.getContentExpression()) == Sweep.CONSUMING) {
            return new FeedMakerDelegate((watchManager, out, context) -> new ResultDocumentFeed(getExpression(), out, context));
        } else {
            return super.getFeedMaker(arg);
        }
    }



    public static class ResultDocumentFeed extends ComplexNodeEventFeed {

        public ResultDocumentFeed(Expression exp, ItemFeed result, XPathContext context) {
            super(exp, result, context);
        }

        /**
         * In streaming mode, process the first half of the instruction (for example, to start a new document or element)
         *
         * @param context The dynamic context
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        protected void processLeft(XPathContext context) throws XPathException {
            ResultDocument instr = (ResultDocument)getExpression();
            next = instr.processLeft(context);
        }

        /**
         * In streaming mode, process the right half of the instruction (for example, to end a new document or element)
         *
         * @param context The dynamic context
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs
         */
        @Override
        protected void processRight(XPathContext context) throws XPathException {
            try {
                next.endDocument();
            } catch (XPathException err) {
                throw err.withXPathContext(context);
            }
            next.close();
        }


    }
}

// Copyright (c) 2009-2023 Saxonica Limited