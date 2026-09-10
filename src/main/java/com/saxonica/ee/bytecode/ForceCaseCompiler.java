package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import com.saxonica.ee.bytecode.util.OnEmpty;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.UpperCase;
import net.sf.saxon.str.StringView;

/**
 * Generate bytecode for evaluation of a call to the upper-case() and lower-case functions
 */
public class ForceCaseCompiler extends ToStringCompiler {

    @Override
    public void compileToString(CompilerService compiler, Expression expression, OnEmpty onEmpty) throws CannotCompileException {
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

    @Override
    public void compileToUnicodeString(CompilerService compiler, Expression expression, OnEmpty onEmpty) throws CannotCompileException {
        compileToString(compiler, expression, onEmpty);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeStaticMethod(StringView.class, "of", String.class);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
