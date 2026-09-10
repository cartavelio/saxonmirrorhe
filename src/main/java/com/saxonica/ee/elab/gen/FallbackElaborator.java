package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;

public abstract class Elaborator {


    private Expression expression;

    protected Expression getExpression() {
        return expression;
    }

    protected Configuration getConfiguration() {
        return expression.getConfiguration();
    }

    public Elaborator(Expression expr) {
        this.expression = expr;
    }


    public static Elaborator make(Expression expression) {
        if (expression instanceof SystemFunctionCall) {

        } else {
            String label = expression.getExpressionName();
            Elaborator elab = ElaboratorMap.getElaborator(label);
            if (elab == null) {
                return new FallbackElaborator(expression);
            }


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

