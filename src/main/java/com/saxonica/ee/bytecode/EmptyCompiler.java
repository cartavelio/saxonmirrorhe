package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.value.Cardinality;

/**
 * Generate bytecode for the fn:empty() function
 */
public class EmptyCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        Expression arg = fnc.getArg(0);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "EmptyCompiler");
        LabelInfo returnTrue = methodInfo.newLabel("returnTrueEmpty");
        LabelInfo end = methodInfo.newLabel("endEmpty");

        if (Cardinality.allowsMany(arg.getCardinality())) {
            IteratorDetails argIter = compiler.compileToIterator(arg);
            ga.dup();
            ga.invokeInstanceMethod(argIter.iteratorClass, "next");
            ga.ifNull(returnTrue.label());
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            methodInfo.placeLabel(end);
            ga.swap();
            ga.invokeInstanceMethod(argIter.iteratorClass, "close");
        } else {
            compiler.compileToItem(arg);
            ga.ifNull(returnTrue.label());
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            methodInfo.placeLabel(end);
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


