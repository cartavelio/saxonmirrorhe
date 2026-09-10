package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.BooleanValue;

public abstract class BooleanElaborator extends Elaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> new SingletonIterator(BooleanValue.get(b.eval(context)));
    }

    @Override
    public PushEvaluator elaborateForPush() {
        BooleanEvaluator b = elaborateForBoolean();
        return (out, context) -> out.append(BooleanValue.get(b.eval(context)));
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> BooleanValue.get(b.eval(context));
    }

    public abstract BooleanEvaluator elaborateForBoolean();

    @Override
    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> b.eval(context) ? StringConstants.TRUE : StringConstants.FALSE;
    }


}

