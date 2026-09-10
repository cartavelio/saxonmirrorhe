package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.feed.ComplexNodeEventFeed;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.DocumentInstr;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;


/**
 * Evaluate an xsl:document instruction in streaming code.
 */

public class DocumentInstrAdjunct extends ParentConstructorAdjunct implements FeedMaker {


    public Feed makeItemFeed(WatchManager watchManager, Feed out, XPathContext context) throws XPathException {
        return new DocumentInstrFeed(getExpression(), out, context);
    }

    public static class DocumentInstrFeed extends ComplexNodeEventFeed {

        private Builder builder;

        public DocumentInstrFeed(Expression exp, Feed result, XPathContext context) {
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
                Controller controller = context.getController();
                Configuration config = controller.getConfiguration();
                builder = controller.makeBuilder();
                if (builder instanceof TinyBuilder) {
                    ((TinyBuilder) builder).setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                }

                builder.setBaseURI(exp.getStaticBaseURIString());
                builder.setTiming(false);

                PipelineConfiguration pipe = controller.makePipelineConfiguration();
                pipe.setXPathContext(context);
                pipe.setHostLanguage(Configuration.XSLT);
                builder.setPipelineConfiguration(pipe);

                Receiver out = ComplexContentOutputter.makeComplexContentReceiver(builder, exp.getValidationOptions());
                context.setReceiver(out);
                out.open();
                out.startDocument(0);
                setReceiver(out);

            } catch (XPathException e) {
                e.maybeSetLocation(exp.getLocation());
                e.maybeSetContext(context);
                dynamicError(e);
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
            getReceiver().endDocument();
            getResult().processItem(builder.getCurrentRoot());
        }
    }

}

// Copyright (c) 2010 Saxonica Limited.