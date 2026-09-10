package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ComplexNodeEventFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.DocumentInstr;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Schema;


/**
 * Evaluate an xsl:document instruction in streaming code.
 */

public class DocumentInstrAdjunct extends ParentConstructorAdjunct implements FeedMaker {


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new DocumentInstrFeed(getExpression(), out, context);
    }

    public static class DocumentInstrFeed extends ComplexNodeEventFeed {

        //private Builder builder;

        public DocumentInstrFeed(Expression exp, ItemFeed result, XPathContext context) {
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
            DocumentInstr exp = (DocumentInstr)getExpression();
            try {
                if (!exp.isPreservingTypes()) {
                    Schema schema = exp.getRetainedStaticContext().getPackageData().getImportedSchema();
                    Receiver validator = schema.getDocumentValidator(
                            next, exp.getStaticBaseURIString(), exp.getValidationOptions(), exp.getLocation());
                    next = new ComplexContentOutputter(validator);
                }
                next.startDocument(ReceiverOption.NONE);
            } catch (XPathException e) {
                dynamicError(e.maybeWithLocation(exp.getLocation()).maybeWithContext(context));
            }
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
            next.endDocument();
            //getResult().append(builder.getCurrentRoot());
        }
    }

}

// Copyright (c) 2010-2023 Saxonica Limited