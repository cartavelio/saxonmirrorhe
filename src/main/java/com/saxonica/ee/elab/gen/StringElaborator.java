package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.StringValue;

public abstract class StringElaborator extends Elaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        StringEvaluator strEval = elaborateForString(false);
        return context -> {
            UnicodeString value = strEval.eval(context);
            if (value == null) {
                return EmptyIterator.emptyIterator();
            } else {
                return SingletonIterator.makeIterator(StringValue.makeUStringValue(value));
            }
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        ItemEvaluator ie = elaborateForItem();
        return (out, context) -> {
            Item it = ie.eval(context);
            if (it != null) {
                out.append(it);
            }
            return null;
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        StringEvaluator strEval = elaborateForString(false);
        return context -> {
            UnicodeString value = strEval.eval(context);
            if (value == null) {
                return null;
            } else {
                return StringValue.makeUStringValue(value);
            }
        };
    }

    public BooleanEvaluator elaborateForBoolean() {
        StringEvaluator strEval = elaborateForString(false);
        return context -> {
            UnicodeString value = strEval.eval(context);
            return value != null && !value.isEmpty();
        };
    }

    @Override
    public abstract StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent);


}

