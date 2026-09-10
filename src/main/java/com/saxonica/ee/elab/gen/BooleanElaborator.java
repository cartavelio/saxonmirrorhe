package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.BooleanValue;

public abstract class BooleanElaborator extends Elaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        ItemEvaluator b = elaborateForItem();
        return context -> SingletonIterator.makeIterator(b.eval(context));
    }

    @Override
    public PushEvaluator elaborateForPush() {
        ItemEvaluator b = elaborateForItem();
        return (out, context) -> {
            Item it = b.eval(context);
            if (it != null) {
                out.append(it);
            }
        };
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

