package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.NegateExpression;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.NumericValue;

/**
 * Generate bytecode for evaluation of a call to the substring() function
 */
public class NegateExpressionCompiler extends ToItemCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "NegateExpression-Item");
        NegateExpression neg = (NegateExpression) expression;
        compiler.compileToItem(neg.getBaseExpression());
        LabelInfo negateReturn = methodInfo.newLabel("negateReturn");
        if (Cardinality.allowsZero(neg.getBaseExpression().getCardinality())) {
            LabelInfo notNull = methodInfo.newLabel("negateNotNull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            if (neg.isBackwardsCompatible()) {
                ga.getStaticField(DoubleValue.class, "NaN", DoubleValue.class);
            } else {
                ga.pushNull();
            }
            ga.goTo(negateReturn);
            methodInfo.placeLabel(notNull);
        }
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "negate");
        methodInfo.placeLabel(negateReturn);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
