package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;

public class FallbackElaborator extends Elaborator {

    public FallbackElaborator() {
        
    }

    public FallbackElaborator(Expression expr) {
        super();
        setExpression(expr);
    }


    public PullEvaluator getPullEvaluator() {
        return getExpression()::iterate;
    }

    public PushEvaluator getPushEvaluator() {
        return (out, context) -> getExpression().process(out, context);
    }

    public ItemEvaluator getItemEvaluator() {
        PullEvaluator puller = getPullEvaluator();
        return context -> puller.eval(context).next();
    }

    public BooleanEvaluator getBooleanEvaluator() {
        PullEvaluator puller = getPullEvaluator();
        return context -> ExpressionTool.effectiveBooleanValue(puller.eval(context));
    }

    public StringEvaluator getStringEvaluator(UnicodeString resultOnEmpty) {
        ItemEvaluator itemEval = getItemEvaluator();
        return context -> {
            Item item = itemEval.eval(context);
            return item == null ? resultOnEmpty : item.getUnicodeStringValue();
        };
    }


}

