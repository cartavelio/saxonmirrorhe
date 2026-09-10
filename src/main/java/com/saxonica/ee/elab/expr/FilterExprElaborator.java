package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.FilterExpression;
import net.sf.saxon.expr.FilterIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.FocusTrackingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

/**
 * Elaborator for a filter expression
 */

public class FilterExprElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final FilterExpression expr = (FilterExpression) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBase()).elaborateForPull();
        if (expr.isSimpleBooleanFilter()) {
            final BooleanEvaluator conditionEval = makeElaborator(expr.getFilter()).elaborateForBoolean();
            return context -> {
                SequenceIterator base = baseEval.eval(context);
                XPathContext c2 = context.newMinorContext();
                c2.setCurrentIterator(new FocusTrackingIterator(base));
                return new SimpleFilteredIterator(c2, conditionEval);
            };
        } else {
            final PullEvaluator conditionEval = makeElaborator(expr.getFilter()).elaborateForPull();
            return context -> {
                SequenceIterator base = baseEval.eval(context);
                XPathContext c2 = context.newMinorContext();
                c2.setCurrentIterator(new FocusTrackingIterator(base));
                return new PositionalFilteredIterator(c2, conditionEval);
            };
        }

    }

    /**
     * An iterator for a filter expression in which the predicate is capable of evaluating as a number
     * (in which case it needs to be compared with the context position, rather than being converted
     * to a boolean)
     */

    public static class PositionalFilteredIterator implements SequenceIterator {
        private final XPathContext outerContext;
        private final FocusIterator base;
        private final PullEvaluator condition;

        /**
         * Construct a AxisFilter
         *
         * @param outerContext the underlying iterator that returns all the nodes on
         *             a required axis.
         * @param condition a test that is applied to each node returned by the
         *             underlying SequenceIterator; only those items that pass the NodeTest are
         *             returned by the filter
         */

        public PositionalFilteredIterator(XPathContext outerContext, PullEvaluator condition) {
            this.outerContext = outerContext;
            this.base = outerContext.getCurrentIterator();
            this.condition = condition;
        }

        /*@Nullable*/
        @Override
        public Item next() {
            try {
                while (true) {
                    Item next = base.next();
                    if (next == null) {
                        return null;
                    }
                    if (FilterIterator.testPredicateValue(condition.eval(outerContext), base.position(), null)) {
                        return next;
                    }
                }
            } catch (XPathException e) {
                throw new UncheckedXPathException(e);
            }
        }

    }

    /**
     * An iterator for a filter expression where it is known that the filter value will not be numeric,
     * and can therefore be evaluated directly as a boolean
     */

    public static class SimpleFilteredIterator implements SequenceIterator {
        private final XPathContext outerContext;
        private final FocusIterator base;
        private final BooleanEvaluator condition;

        /**
         * Construct a AxisFilter
         *
         * @param outerContext the underlying iterator that returns all the nodes on
         *                     a required axis.
         * @param condition    a test that is applied to each node returned by the
         *                     underlying SequenceIterator; only those items that pass the NodeTest are
         *                     returned by the filter
         */

        public SimpleFilteredIterator(XPathContext outerContext, BooleanEvaluator condition) {
            this.outerContext = outerContext;
            this.base = outerContext.getCurrentIterator();
            this.condition = condition;
        }

        /*@Nullable*/
        @Override
        public Item next() {
            try {
                while (true) {
                    Item next = base.next();
                    if (next == null) {
                        return null;
                    }
                    if (condition.eval(outerContext)) {
                        return next;
                    }
                }
            } catch (XPathException e) {
                throw new UncheckedXPathException(e);
            }
        }

    }

}

