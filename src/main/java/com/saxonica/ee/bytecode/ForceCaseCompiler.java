package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import com.saxonica.ee.bytecode.util.OnEmpty;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.UpperCase;

/**
 * Generate bytecode for evaluation of a call to the upper-case() and lower-case functions
 */
public class ForceCaseCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "ForceCase-str");
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        compiler.compileToPrimitive(fnc.getArg(0), String.class, OnEmpty.RETURN_EMPTY_STRING);

        if (fnc.getTargetFunction() instanceof UpperCase) {
            ga.invokeInstanceMethod(String.class, "toUpperCase");
        } else {
            ga.invokeInstanceMethod(String.class, "toLowerCase");
        }

    }
}

// Copyright (c) 2011-12 Saxonica Limited
