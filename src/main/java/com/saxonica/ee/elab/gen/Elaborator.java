package com.saxonica.ee.elab;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;

import java.util.HashMap;
import java.util.Map;

public abstract class Elaborator {

    private static Map<Class<Expression>, Class<Elaborator>> classMap = new HashMap<>();

    static{
        classMap.put(FixedElement.class, FixedElementElaborator.class);
    }

    private Expression expression;

    protected Expression getExpression() {
        return expression;
    }

    protected Configuration getConfiguration() {
        return expression.getConfiguration();
    }

    public static Elaborator make(Expression expression) {
        return null;
    }

    public PullEvaluator getPullEvaluator() {
        return expression::iterate;
    }

    public PushEvaluator getPushEvaluator() {
        return (out, context) -> expression.process(out, context);
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

