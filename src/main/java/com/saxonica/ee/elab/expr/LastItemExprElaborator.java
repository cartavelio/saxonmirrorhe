package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.LastItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.ReversibleIterator;

/**
 * Elaborator for a "last item expression" (typically {@code SEQ[last()]})
 */

public class LastItemExprElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {

        final LastItemExpression expr = (LastItemExpression) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();

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

