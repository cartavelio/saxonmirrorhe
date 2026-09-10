package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ForExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

import java.util.List;


/**
 * Streaming code to implement a "for" expression
 */
public class ForExpressionAdjunct extends StreamingAdjunct implements FeedMaker {

    /**
     * Get the "sweep" of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo      information about the context item
     * @param reasons          the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        ForExpression exp = (ForExpression)getExpression();
        Posture p = Streamability.getStreamability(exp.getSequence(), contextInfo, reasons).getPosture();
        if (p == Posture.GROUNDED) {
            return super.computeStreamability(contextInfo, reasons);
        } else {
            if (reasons != null) {
                String desc = exp.getVariableQName().hasURI(NamespaceConstant.SAXON_GENERATED_VARIABLE)
                        ? exp.getSequence().toShortString()
                        : "bound to the variable $" + exp.getVariableName();
                reasons.add("The expression " + desc +
                        " is capable of delivering streamed nodes (line " + getExpression().getLocation().getLineNumber() + ")");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
    }


    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        return new ForExpressionFeed(getExpression(), out, context);
    }

    public static class ForExpressionFeed extends ItemFeed {

        public ForExpressionFeed(Expression expr, ItemFeed out, XPathContext context) {
            super(expr, out, context);
        }

        /**
         * Supply one item towards the streamed input of the expression
         *
         * @param item the item to be supplied
         */
        @Override
        public void append(Item item) throws XPathException {
            getContext().setLocalVariable(((ForExpression)getExpression()).getLocalSlotNumber(), item);
            SequenceIterator iter = ((ForExpression)getExpression()).getAction().iterate(getContext());
            processItems(iter, getNextOutputter());
        }
    }

}

// Copyright (c) 2013-2022 Saxonica Limited