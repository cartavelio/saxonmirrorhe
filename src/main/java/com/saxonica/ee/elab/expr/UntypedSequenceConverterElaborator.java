package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.AtomicSequenceConverter;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.UntypedSequenceConverter;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

/**
 * Elaborator for an UntypedSequenceConverter
 */

public class UntypedSequenceConverterElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final UntypedSequenceConverter expr = (UntypedSequenceConverter) getExpression();
        final UntypedSequenceConverter.UntypedConverter converter =
                (UntypedSequenceConverter.UntypedConverter)expr.getConverter();
        assert converter != null;
        final AtomicSequenceConverter.AtomicSequenceMappingFunction mapper =
                new AtomicSequenceConverter.AtomicSequenceMappingFunction();
        mapper.setConverter(converter);
        if (expr.getRoleSupplier() != null) {
            mapper.setErrorCode(expr.getRoleSupplier().get().getErrorCode());
        }
        final PullEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForPull();
        return context -> {
            SequenceIterator base = baseEval.eval(context);
            return new ItemMappingIterator(base, mapper, true);
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        final AtomicSequenceConverter expr = (AtomicSequenceConverter) getExpression();
        final ItemEvaluator baseEval = makeElaborator(expr.getBaseExpression()).elaborateForItem();
        final UntypedSequenceConverter.UntypedConverter converter =
                (UntypedSequenceConverter.UntypedConverter) expr.getConverter();
        assert converter != null;
        final boolean nullable = Cardinality.allowsZero(expr.getBaseExpression().getCardinality());
        return context -> {
            AtomicValue baseValue = (AtomicValue)baseEval.eval(context);
            if (nullable && baseValue == null) {
                return null;
            }
            return converter.convert(baseValue).asAtomic();
        };
    }
}

