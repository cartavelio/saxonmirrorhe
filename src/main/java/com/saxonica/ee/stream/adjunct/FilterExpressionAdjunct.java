package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.BufferingFilterExpressionFeed;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.BasePatternWithPredicate;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.type.TypeHierarchy;

import java.util.List;


/**
 * Generate streaming code for a filter expression (when not part of a Watch).
 */
public class FilterExpressionAdjunct extends FilteringAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        FilterExpression exp = (FilterExpression)getExpression();
        Expression start = exp.getSelectExpression();
        Expression filter = exp.getActionExpression();
        // See 19.8.7.8
        PostureAndSweep startPS = Streamability.getStreamability(start,contextInfo, reasons);
        ContextItemStaticInfoEE info =
                new ContextItemStaticInfoEE(start.getItemType(), false, startPS.getPosture());
        PostureAndSweep filterPS = Streamability.getStreamability(filter, info, reasons);
        if (filterPS.getSweep() == Sweep.MOTIONLESS) {
            return startPS;
        } else {
            if (reasons != null) {
                reasons.add("Predicate [" + filter.toShortString() + "] at line " + filter.getLocation().getLineNumber() + " is not motionless");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
    }


    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     *
     * @param config           the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */
    @Override
    public Pattern toStreamingPattern(Configuration config) {
        FilterExpression exp = (FilterExpression)getExpression();
        TypeHierarchy th = config.getTypeHierarchy();
        Expression base = exp.getSelectExpression();
        Expression filter = exp.getFilter();
        PostureAndSweep fs = Streamability.getStreamability(filter, ContextItemStaticInfoEE.DEFAULT, null);
        if (fs.getSweep() != Sweep.MOTIONLESS) {
            return null;
        }

        Pattern basePattern = Streamability.toStreamingPattern(base, config);
//        boolean addAnchor = false;
//        if (basePattern instanceof AncestorQualifiedPattern &&
//                ((AncestorQualifiedPattern) basePattern).getUpperPattern() instanceof AnchorPattern &&
//                ((AncestorQualifiedPattern) basePattern).getUpwardsAxis() == AxisInfo.PARENT) {
//            basePattern = ((AncestorQualifiedPattern) basePattern).getBasePattern();
//            addAnchor = true;
//        }
        if (basePattern == null) {
            return null;
        }

        if (exp.isPositional(th)) {
            return null;
        } else if (ExpressionTool.containsLocalVariableReference(exp)) {
            return null;
        } else {
            return new BasePatternWithPredicate(basePattern, filter);
        }
    }


    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dyamic context
     * @param out the next filter in the pipeline
     * @return a filter that can be added to the event stream
     */
    @Override
    public FilteringFeed.Filter makeFilter(XPathContext context, Feed out)  {
        return new PredicateFilter(((FilterExpression)getExpression()).getActionExpression(), context);
    }

    public FeedMaker getFeedMaker(int arg) throws XPathException {
        FilterExpression f = (FilterExpression) getExpression();
        final boolean isBuffering =
                (f.getActionExpression().getDependencies() & StaticProperty.DEPENDS_ON_LAST) != 0;
        if (isBuffering) {
            return (watchManager, out, context) -> new BufferingFilterExpressionFeed(getExpression(), out, context);
        } else {
            return super.getFeedMaker(arg);
        }
    }

    private static class PredicateFilter implements FilteringFeed.Filter {

        private Expression predicate;
        private XPathContext context;
        private ManualIterator iter;

        public PredicateFilter(Expression predicate, XPathContext context) {
            this.predicate = predicate;
            this.context = context.newMinorContext();
            iter = new ManualIterator();
            this.context.setCurrentIterator(iter);

        }

        public int matches(Item item, int position) throws XPathException {
            iter.setContextItem(item);
            iter.setPosition(position);
            SequenceIterator predicateValue = predicate.iterate(context);
            boolean matches = FilterIterator.testPredicateValue(predicateValue, position, predicate);
            return matches ? MATCHES : SKIP;
        }

    }
}

// Copyright (c) 2010-2013 Saxonica Limited.