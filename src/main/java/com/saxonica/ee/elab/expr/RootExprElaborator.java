package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.RootExpression;

public class RootExprElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final RootExpression expr = (RootExpression) getExpression();
        return expr::getNode;
    }

}

