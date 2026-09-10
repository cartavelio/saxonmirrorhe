package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SingletonIntersectExpression;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;

/**
 * Elaborator for a Venn expression: that is {@code A union B}, {@code A intersect B},
 * or {@code A except B}
 */

public class SingletonIntersectElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {

        final SingletonIntersectExpression exp = (SingletonIntersectExpression) getExpression();
        final ItemEvaluator lhs = Elaborator.makeElaborator(exp.getLhsExpression()).elaborateForItem();
        final PullEvaluator rhs = Elaborator.makeElaborator(exp.getRhsExpression()).elaborateForPull();

        return context -> {
            NodeInfo node = (NodeInfo) lhs.eval(context);
            if (node == null) {
                return null;
            }
            SequenceIterator nodeSet = rhs.eval(context);
            return SingletonIntersectExpression.containsNode(nodeSet, node) ? node : null;
        };

    }
}

