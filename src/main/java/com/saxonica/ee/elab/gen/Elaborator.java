package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.str.UnicodeString;

public abstract class Elaborator {


    private Expression expression;

    public Expression getExpression() {
        return expression;
    }

    public void setExpression(Expression expr) {
        this.expression = expr;
    }

    protected Configuration getConfiguration() {
        return expression.getConfiguration();
    }

    public Elaborator() {}


    public static Elaborator make(Expression expression) {
        if (expression instanceof SystemFunctionCall) {
            String label = ((SystemFunctionCall)expression).getTargetFunction().getCompilerName();
            Elaborator elab = ElaboratorMap.getElaborator(label);
            if (elab == null) {
                System.err.println("Using fallback elaborator for " + expression.toShortString());
                return new FallbackElaborator(expression);
            } else {
                elab.setExpression(expression);
                return elab;
            }
        } else {
            String label = expression.getExpressionName();
            Elaborator elab = ElaboratorMap.getElaborator(label);
            if (elab == null) {
                System.err.println("Using fallback elaborator for " + expression.toShortString());
                return new FallbackElaborator(expression);
            } else {
                elab.setExpression(expression);
                return elab;
            }
        }
    }

    public abstract PullEvaluator getPullEvaluator();

    public abstract PushEvaluator getPushEvaluator();

    public abstract ItemEvaluator getItemEvaluator();

    public abstract BooleanEvaluator getBooleanEvaluator();

    public abstract StringEvaluator getStringEvaluator(UnicodeString resultOnEmpty);

}

