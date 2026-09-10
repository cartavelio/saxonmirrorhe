package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import net.sf.saxon.expr.IdentityComparison;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for an identity comparison (operators {@code is}, {@code <<}, and {@code >>} )
 */

public class IdentityComparisonElaborator extends BooleanElaborator {

    public BooleanEvaluator elaborateForBoolean() {
        final IdentityComparison exp = (IdentityComparison)getExpression();
        final ItemEvaluator p0 = makeElaborator(exp.getLhsExpression()).elaborateForItem();
        final ItemEvaluator p1 = makeElaborator(exp.getRhsExpression()).elaborateForItem();
        final boolean nullable0 = Cardinality.allowsZero(exp.getLhsExpression().getCardinality());
        final boolean nullable1 = Cardinality.allowsZero(exp.getRhsExpression().getCardinality());
        final int operator = exp.getOperator();

        return context -> {
            NodeInfo v0 = (NodeInfo)p0.eval(context);
            if (nullable0 && v0 == null) {
                return false;
            }
            NodeInfo v1 = (NodeInfo)p1.eval(context);
            if (nullable1 && v1 == null) {
                return false;
            }
            switch (operator) {
                case Token.IS:
                    return v0.equals(v1);
                case Token.PRECEDES:
                    return GlobalOrderComparer.getInstance().compare(v0, v1) < 0;
                case Token.FOLLOWS:
                    return GlobalOrderComparer.getInstance().compare(v0, v1) > 0;
                default:
                    throw new UnsupportedOperationException("Unknown node identity test");
            }
        };

    }

    public ItemEvaluator elaborateForItem() {
        final IdentityComparison exp = (IdentityComparison) getExpression();
        final ItemEvaluator p0 = makeElaborator(exp.getLhsExpression()).elaborateForItem();
        final ItemEvaluator p1 = makeElaborator(exp.getRhsExpression()).elaborateForItem();
        final boolean nullable0 = Cardinality.allowsZero(exp.getLhsExpression().getCardinality());
        final boolean nullable1 = Cardinality.allowsZero(exp.getRhsExpression().getCardinality());
        final int operator = exp.getOperator();

        return context -> {
            NodeInfo v0 = (NodeInfo) p0.eval(context);
            if (nullable0 && v0 == null) {
                return null;
            }
            NodeInfo v1 = (NodeInfo) p1.eval(context);
            if (nullable1 && v1 == null) {
                return null;
            }
            switch (operator) {
                case Token.IS:
                    return BooleanValue.get(v0.equals(v1));
                case Token.PRECEDES:
                    return BooleanValue.get(GlobalOrderComparer.getInstance().compare(v0, v1) < 0);
                case Token.FOLLOWS:
                    return BooleanValue.get(GlobalOrderComparer.getInstance().compare(v0, v1) > 0);
                default:
                    throw new UnsupportedOperationException("Unknown node identity test");
            }
        };

    }


}

