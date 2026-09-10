package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;

/**
 * Compile the fn:boolean() function to Java bytecode
 */
public class BooleanFnCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        Expression arg = ((SystemFunctionCall) expression).getArg(0);
        compiler.compileToBoolean(arg);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


