package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.AtomicSequenceConverter;
import net.sf.saxon.om.SequenceIterator;

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
        //noinspection Convert2MethodRef
        return context -> expr.evaluateItem(context);
    }
}

