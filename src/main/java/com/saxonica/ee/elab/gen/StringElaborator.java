package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.StringValue;

public abstract class StringElaborator extends Elaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        StringEvaluator strEval = getStringEvaluator(null);
        return context -> {
            UnicodeString value = strEval.eval(context);
            if (value == null) {
                return EmptyIterator.emptyIterator();
            } else {
                return new SingletonIterator(StringValue.makeUStringValue(value));
            }
        };
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        ItemEvaluator ie = getItemEvaluator();
        return (out, context) -> out.append(ie.eval(context));
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        StringEvaluator strEval = getStringEvaluator(null);
        return context -> {
            UnicodeString value = strEval.eval(context);
            if (value == null) {
                return null;
            } else {
                return StringValue.makeUStringValue(value);
            }
        };
    }

    public BooleanEvaluator getBooleanEvaluator() {
        StringEvaluator strEval = getStringEvaluator(null);
        return context -> {
            UnicodeString value = strEval.eval(context);
            return value != null && !value.isEmpty();
        };
    }

    @Override
    public abstract StringEvaluator getStringEvaluator(UnicodeString resultOnEmpty);


}

