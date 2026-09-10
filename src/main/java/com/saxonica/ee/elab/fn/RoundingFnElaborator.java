package com.saxonica.ee.elab.fn;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Sum;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;

public class SumFnElaborator extends ItemElaborator {

    public ItemEvaluator getItemEvaluator() {
        SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
        PullEvaluator puller = Elaborator.make(fnc.getArg(0)).getPullEvaluator();
        ItemEvaluator zero = fnc.getArity() == 2
                ? Elaborator.make(fnc.getArg(1)).getItemEvaluator()
                : context -> Int64Value.ZERO;
        return context -> {
            Sum.SumFold fold = new Sum.SumFold(context, (AtomicValue)zero.eval(context));
            SequenceIterator iter = puller.eval(context);
            Item it;
            while ((it = iter.next()) != null) {
                fold.processItem(it);
            }
            return (AtomicValue)fold.result();
        };
    }


}

