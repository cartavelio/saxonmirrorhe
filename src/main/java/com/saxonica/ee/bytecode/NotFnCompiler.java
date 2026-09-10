package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;

/**
 * Generate bytecode for the fn:not() function
 */
public class NotFnCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        Expression arg = ((SystemFunctionCall) expression).getArg(0);
        Generator ga = compiler.getCurrentGenerator();
        compiler.compileToBoolean(arg);
        ga.not();
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
