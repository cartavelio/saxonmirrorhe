package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.tree.iter.SingletonIterator;

public abstract class ItemElaborator extends Elaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        ItemEvaluator ie = elaborateForItem();
        return context -> SingletonIterator.makeIterator(ie.eval(context));
    }

    @Override
    public PushEvaluator elaborateForPush() {
        ItemEvaluator ie = elaborateForItem();
        return (out, context) -> out.append(ie.eval(context));
    }

    @Override
    public abstract ItemEvaluator elaborateForItem();

    public BooleanEvaluator elaborateForBoolean() {
        ItemEvaluator ie = elaborateForItem();
        return context -> ExpressionTool.effectiveBooleanValue(ie.eval(context));
    }

    @Override
    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        ItemEvaluator ie = elaborateForItem();
        return context -> {
            Item item = ie.eval(context);
            return item == null ? handleNullString(zeroLengthWhenAbsent) : item.getUnicodeStringValue();
        };
    }


}

