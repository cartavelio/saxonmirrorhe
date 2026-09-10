package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;

public class ContextItemElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
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

}

