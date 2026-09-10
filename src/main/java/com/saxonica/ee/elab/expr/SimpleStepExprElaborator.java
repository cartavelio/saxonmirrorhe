package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.AxisExpression;
import net.sf.saxon.expr.SimpleStepExpression;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.Cardinality;

public class SimpleStepExprElaborator extends PullElaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        final SimpleStepExpression forEach = (SimpleStepExpression) getExpression();
        final ItemEvaluator select = Elaborator.make(forEach.getSelectExpression()).getItemEvaluator();
        final AxisExpression step = (AxisExpression) forEach.getStep();
        final boolean nullable = Cardinality.allowsZero(forEach.getSelectExpression().getCardinality());
        return context -> {
            NodeInfo origin = (NodeInfo) select.eval(context);
            if (nullable && origin == null) {
                return EmptyIterator.getInstance();
            }
            return step.iterate(origin);
        };
    }

}

