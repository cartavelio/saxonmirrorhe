package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.OrExpression;
import org.objectweb.asm.commons.GeneratorAdapter;


/**
 * Generate bytecode for an Or expression
 */
public class OrExpressionCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Expression operand1 = ((OrExpression) expression).getLhsExpression();
        compiler.compileToBoolean(operand1);
        LabelInfo op1Lab = methodInfo.newLabel("op1Lab");
        LabelInfo end = methodInfo.newLabel("endOr");
        ga.ifZCmp(GeneratorAdapter.EQ, op1Lab.label());
        ga.push(true);
        ga.goTo(end);
        methodInfo.placeLabel(op1Lab);

        Expression operand2 = ((OrExpression) expression).getRhsExpression();
        compiler.compileToBoolean(operand2);

        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
