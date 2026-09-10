package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.Substring;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringItem;

/**
 * Generate bytecode for evaluation of a call to the substring() function
 */
public class SubstringCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        final Substring fn = (Substring) fnc.getTargetFunction();
        int numArgs = fn.getArity();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Substring-str");

        LabelInfo substringReturn = methodInfo.newLabel("substringReturn");

        compiler.compileToItem(fnc.getArg(0));
        returnZeroLengthStringIfNull(fnc.getArg(0), ga, methodInfo, substringReturn, 0);

        ga.checkClass(StringItem.class);

        if (numArgs == 2) {
            compiler.compileToItem(fnc.getArg(1));
            ga.checkClass(NumericValue.class);
            ga.invokeStaticMethod(Substring.class, "substring", StringItem.class, NumericValue.class);
        } else {
            compiler.compileToItem(fnc.getArg(1));
            ga.checkClass(NumericValue.class);
            compiler.compileToItem(fnc.getArg(2));
            ga.checkClass(NumericValue.class);
            ga.invokeStaticMethod(Substring.class, "substring", StringItem.class, NumericValue.class, NumericValue.class);
        }

        methodInfo.placeLabel(substringReturn);
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(Object.class, "toString");
        }

    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final Substring fn = (Substring) fnc.getTargetFunction();
        int numArgs = fn.getArity();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Substring-str");

        LabelInfo substringReturn = methodInfo.newLabel("substringReturn");

        compiler.compileToItem(fnc.getArg(0));
        if (Cardinality.allowsZero(fnc.getArg(0).getCardinality())) {
            LabelInfo stringArgNonNull = methodInfo.newLabel("stringArgNonNull");
            ga.dup();
            ga.ifNonNull(stringArgNonNull.label());
            ga.pop();
            allocateStatic(compiler, StringItem.EMPTY_STRING);
            ga.goTo(substringReturn);
            methodInfo.placeLabel(stringArgNonNull);
        }

        ga.checkClass(StringItem.class);

        if (numArgs == 2) {
            compiler.compileToItem(fnc.getArg(1));
            ga.checkClass(NumericValue.class);
            ga.invokeStaticMethod(Substring.class, "substring", StringItem.class, NumericValue.class);
            ga.invokeStaticMethod(StringItem.class, "makeStringValue", CharSequence.class);
        } else {
            compiler.compileToItem(fnc.getArg(1));
            ga.checkClass(NumericValue.class);
            compiler.compileToItem(fnc.getArg(2));
            ga.checkClass(NumericValue.class);
            ga.invokeStaticMethod(Substring.class, "substring", StringItem.class, NumericValue.class, NumericValue.class);
            ga.invokeStaticMethod(StringItem.class, "makeStringValue", CharSequence.class);
        }

        methodInfo.placeLabel(substringReturn);

    }


}

// Copyright (c) 2011-2020 Saxonica Limited
