package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
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

public class FilterExprElaborator extends PullElaborator {

    public PullEvaluator getPullEvaluator() {
        final FilterExpression expr = (FilterExpression) getExpression();
        final PullEvaluator baseEval = Elaborator.make(expr.getBase()).getPullEvaluator();
        final PullEvaluator conditionEval = Elaborator.make(expr.getBase()).getPullEvaluator();
        return context -> {
            SequenceIterator base = baseEval.eval(context);
            XPathContext c2 = context.newMinorContext();
            c2.setCurrentIterator(new FocusTrackingIterator(base));
            return new FilteredIterator(c2, conditionEval);
        };
    }

    public static class FilteredIterator implements SequenceIterator {
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

        public FilteredIterator(XPathContext outerContext, PullEvaluator condition) {
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

}

