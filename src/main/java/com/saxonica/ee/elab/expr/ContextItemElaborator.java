package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;

/**
 * Elaborator for the context item expression, "dot".
 */

public class ContextItemElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        ContextItemExpression cie = (ContextItemExpression) getExpression();
        if (cie.isContextPossiblyUndefined()) {
            return context -> {
                Item current = context.getContextItem();
                if (current == null) {
                    cie.reportAbsentContext(context);
                }
                return current;
            };
        } else {
            return XPathContext::getContextItem;
        }
    }

    @Override
    public PullEvaluator elaborateForPull() {
        return super.elaborateForPull();
    }
}

