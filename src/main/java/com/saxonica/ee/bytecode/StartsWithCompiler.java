package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.CollatingFunctionFixed;
import net.sf.saxon.functions.StartsWith;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.UnicodeString;

/**
 * Generate bytecode for a call on fn:starts-with or fn:ends-with, with a statically known collation
 */

public class StartsWithCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        CollatingFunctionFixed function = (CollatingFunctionFixed) fnc.getTargetFunction();
        boolean isStartsWith = function instanceof StartsWith;
        Expression arg0 = fnc.getArg(0);
        Expression arg1 = fnc.getArg(1);
        SubstringMatcher stringCollator = (SubstringMatcher) function.getStringCollator();
        Generator ga = compiler.getCurrentGenerator();

        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, isStartsWith ? "StartsWithCompiler": "EndsWithCompiler");
        visitLineNumber(compiler, ga, expression);

        if (stringCollator instanceof CodepointCollator) {

            // Use arg0.startsWith(arg1, 0), or arg0.endsWith(arg1)

            LabelInfo end = methodInfo.newLabel("endContains");
            LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
            compiler.compileToPrimitive(arg0, String.class, OnEmpty.RETURN_EMPTY_STRING);
            compiler.compileToPrimitive(arg1, String.class, OnEmpty.RETURN_EMPTY_STRING);
            if (isStartsWith) {
                // call s0.startsWith(s1, 0) because that's exactly what s0.startsWith(s1) does
                ga.push(0);
                ga.invokeInstanceMethod(String.class, "startsWith", String.class, Integer.TYPE);
            } else {
                ga.invokeInstanceMethod(String.class, "endsWith", String.class);
            }
            ga.ifTrue(returnTrue);
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            methodInfo.placeLabel(end);

        } else {

            allocateStatic(compiler, stringCollator);
            compiler.compileToPrimitive(arg0, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            compiler.compileToPrimitive(arg1, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            ga.invokeInstanceMethod(SubstringMatcher.class,
                                    isStartsWith ? "startsWith" : "endsWith",
                                    UnicodeString.class, UnicodeString.class);
        }

    }


}


// Copyright (c) 2011-2022 Saxonica Limited
