package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodeTest;

/**
 * Elaborator for an Atomizer
 */

public class AtomizerElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final Atomizer expr = (Atomizer) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        final boolean oneToOne = expr.isUntyped() && expr.getBaseExpression().getItemType() instanceof NodeTest;
        return context -> {
            SequenceIterator base = baseEval.eval(context);
            return Atomizer.getAtomizingIterator(base, oneToOne);
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final Atomizer expr = (Atomizer) getExpression();
        final ItemEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForItem();
        return context -> {
            Item it = baseEval.eval(context);
            if (it == null) {
                return null;
            } else {
                return it.atomize().head();
            }
        };
    }
}

