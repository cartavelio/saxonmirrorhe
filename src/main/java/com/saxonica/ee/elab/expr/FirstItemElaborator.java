package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
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

