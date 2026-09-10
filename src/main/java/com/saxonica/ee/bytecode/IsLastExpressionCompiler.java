package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.IsLastExpression;
import net.sf.saxon.expr.XPathContext;

/**
 * Generate bytecode for the IsLastExpression, which tests whether position() is equal to last()
 */
public class IsLastExpressionCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "isAtLast");
        if (!((IsLastExpression) expression).getCondition()) {
            ga.not();
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
