package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.SingletonIterator;

public abstract class ItemElaborator extends Elaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        ItemEvaluator ie = getItemEvaluator();
        return context -> new SingletonIterator(ie.eval(context));
    }

    @Override
    public PushEvaluator getPushEvaluator() {
        ItemEvaluator ie = getItemEvaluator();
        return (out, context) -> out.append(ie.eval(context));
    }

    @Override
    public abstract ItemEvaluator getItemEvaluator();

    public BooleanEvaluator getBooleanEvaluator() {
        ItemEvaluator ie = getItemEvaluator();
        return context -> ExpressionTool.effectiveBooleanValue(ie.eval(context));
    }

    @Override
    public StringEvaluator getStringEvaluator(UnicodeString resultOnEmpty) {
        ItemEvaluator ie = getItemEvaluator();
        return context -> {
            Item item = ie.eval(context);
            return item == null ? resultOnEmpty : item.getUnicodeStringValue();
        };
    }


}

