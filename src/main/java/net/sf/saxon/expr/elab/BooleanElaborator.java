package net.sf.saxon.expr.elab;

import net.sf.saxon.str.StringConstants;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.BooleanValue;

public abstract class BooleanElaborator extends Elaborator {

    @Override
    public EagerEvaluator eagerly() {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> BooleanValue.get(b.eval(context));
    }

    @Override
    public PullEvaluator elaborateForPull() {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> SingletonIterator.makeIterator(BooleanValue.get(b.eval(context)));
    }

    @Override
    public PushEvaluator elaborateForPush() {
        BooleanEvaluator b = elaborateForBoolean();
        return (out, context) -> {
            out.append(BooleanValue.get(b.eval(context)));
            return null;
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> BooleanValue.get(b.eval(context));
    }

    public abstract BooleanEvaluator elaborateForBoolean();

    @Override
    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        BooleanEvaluator b = elaborateForBoolean();
        return context -> b.eval(context) ? StringConstants.TRUE : StringConstants.FALSE;
    }


}

