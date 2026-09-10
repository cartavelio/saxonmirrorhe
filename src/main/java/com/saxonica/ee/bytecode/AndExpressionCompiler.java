package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.AndExpression;
import net.sf.saxon.expr.Expression;

/**
 * Generate bytecode for a call to the And expression
 */
public class AndExpressionCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, /*@NotNull*/ Expression expression)
            throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Expression operand1 = ((AndExpression) expression).getLhsExpression();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "AndExpression-Boolean");
        compiler.compileToBoolean(operand1);

        LabelInfo op1Lab = methodInfo.newLabel("op1Lab");
        LabelInfo end = methodInfo.newLabel("endAnd");
        ga.ifTrue(op1Lab);
        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(op1Lab);

        Expression operand2 = ((AndExpression) expression).getRhsExpression();
        compiler.compileToBoolean(operand2);

        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
