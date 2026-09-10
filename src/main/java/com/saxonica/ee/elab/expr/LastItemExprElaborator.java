package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.FirstItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.ReversibleIterator;

public class LastItemExprElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        final FirstItemExpression expr = (FirstItemExpression) getExpression();
        final PullEvaluator baseEval = Elaborator.make(expr.getBaseExpression()).getPullEvaluator();
        return context -> {
            final SequenceIterator forwards = baseEval.eval(context);
            if (forwards instanceof ReversibleIterator) {
                return ((ReversibleIterator) forwards).getReverseIterator().next();
            } else {
                Item current = null;
                while (true) {
                    Item item = forwards.next();
                    if (item == null) {
                        return current;
                    }
                    current = item;
                }
            }
        };
    }

}

