package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.functions.Contains;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.UnicodeString;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for a call on fn:contains where the collation is known statically
 */

public class ContainsCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        Contains contains = (Contains) fnc.getTargetFunction();
        Expression arg0 = fnc.getArg(0);
        Expression arg1 = fnc.getArg(1);
        SubstringMatcher stringCollator = (SubstringMatcher)contains.getStringCollator();
        Generator ga = compiler.getCurrentGenerator();

        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ContainsCompiler");
        visitLineNumber(compiler, ga, expression);

        if (stringCollator instanceof CodepointCollator) {

            //return s0.indexOf(s1, 0) >= 0;
            LabelInfo end = methodInfo.newLabel("endContains");
            LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
            compiler.compileToPrimitive(arg0, String.class, OnEmpty.RETURN_EMPTY_STRING);
            compiler.compileToPrimitive(arg1, String.class, OnEmpty.RETURN_EMPTY_STRING);
            ga.push(0);
            ga.invokeInstanceMethod(String.class, "indexOf", String.class, Integer.TYPE);
            ga.ifZCmp(GeneratorAdapter.GE, returnTrue.label());
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            methodInfo.placeLabel(end);

        } else {

            allocateStatic(compiler, stringCollator);
            compiler.compileToPrimitive(arg0, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            compiler.compileToPrimitive(arg1, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
            ga.invokeInstanceMethod(SubstringMatcher.class, "contains", UnicodeString.class, UnicodeString.class);
        }

    }


}


// Copyright (c) 2011-2020 Saxonica Limited
