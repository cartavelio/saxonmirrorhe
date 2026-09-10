package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeString;

public abstract class PullElaborator extends Elaborator {

    @Override
    public abstract PullEvaluator getPullEvaluator();

    @Override
    public PushEvaluator getPushEvaluator() {
        PullEvaluator pull = getPullEvaluator();
        return (out, context) -> {
            SequenceIterator iter = pull.eval(context);
            Item it;
            while ((it = iter.next()) != null) {
                out.append(it);
            }
        };
    }

    @Override
    public ItemEvaluator getItemEvaluator() {
        PullEvaluator pull = getPullEvaluator();
        return context -> pull.eval(context).next();
    }

    public BooleanEvaluator getBooleanEvaluator() {
        PullEvaluator pull = getPullEvaluator();
        return context -> ExpressionTool.effectiveBooleanValue(pull.eval(context));
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

