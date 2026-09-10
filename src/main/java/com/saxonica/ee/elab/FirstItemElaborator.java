package com.saxonica.ee.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.FirstItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public abstract class FirstItemElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        FirstItemExpression expr = (FirstItemExpression) getExpression();
        Expression arg = expr.getBaseExpression();
        PullEvaluator puller = Elaborator.make(arg).getPullEvaluator();
        return context -> {
            SequenceIterator iter = puller.eval(context);
            Item nextItem = iter.next();
            iter.close();
            return nextItem;
        };
    }


}

