package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.RootExpression;

/**
 * Elaborator for a root expression ({@code /})
 */

public class RootExprElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final RootExpression expr = (RootExpression) getExpression();
        return expr::getNode;
    }

}

