package com.saxonica.ee.elab;

import net.sf.saxon.str.StringConstants;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.BooleanValue;

public abstract class BooleanElaborator extends Elaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        BooleanEvaluator b = getBooleanEvaluator();
        return context -> new SingletonIterator(BooleanValue.get(b.eval(context)));
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        BooleanEvaluator b = getBooleanEvaluator();
        return (out, context) -> out.append(BooleanValue.get(b.eval(context)));
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        BooleanEvaluator b = getBooleanEvaluator();
        return context -> BooleanValue.get(b.eval(context));
    }

    public abstract BooleanEvaluator getBooleanEvaluator();

    @Override
    public StringEvaluator getStringEvaluator(UnicodeString resultOnEmpty) {
        BooleanEvaluator b = getBooleanEvaluator();
        return context -> b.eval(context) ? StringConstants.TRUE : StringConstants.FALSE;
    }


}

