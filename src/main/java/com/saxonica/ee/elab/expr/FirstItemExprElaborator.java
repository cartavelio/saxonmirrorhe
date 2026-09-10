package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.FirstItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public class FirstItemExprElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final FirstItemExpression expr = (FirstItemExpression) getExpression();
        final PullEvaluator baseEval = Elaborator.make(expr.getBaseExpression()).getPullEvaluator();
        return context -> {
            final SequenceIterator iter = baseEval.eval(context);
            Item result = iter.next();
            iter.close();
            return result;
        };
    }

}

