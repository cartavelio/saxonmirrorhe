package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.AtomicSequenceConverter;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.AtomicValue;

/**
 * Elaborator for an AtomicSequenceConverter (including an UntypedAtomicConverter, which is
 * the same except that it uses a different converter internally)
 */

public class AtomicSequenceConverterElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final AtomicSequenceConverter expr = (AtomicSequenceConverter) getExpression();
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        return context -> {
            SequenceIterator base = baseEval.eval(context);
            return expr.getConvertingIterator(context, base);
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final AtomicSequenceConverter expr = (AtomicSequenceConverter) getExpression();
        final ItemEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForItem();
        return context -> {
            AtomicValue base = (AtomicValue)baseEval.eval(context);
            return expr.convertItem(base, context);
        };
    }
}

