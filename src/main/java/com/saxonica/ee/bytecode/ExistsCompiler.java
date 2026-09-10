package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.value.Cardinality;

/**
 * Generate bytecode for a call to the exists() function
 */
public class ExistsCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {


        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        Expression arg = fnc.getArg(0);
        visitAnnotation(compiler, "ExistsCompiler");
        visitLineNumber(compiler, ga, expression);
        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo end = methodInfo.newLabel("endExists");
        if (Cardinality.allowsMany(arg.getCardinality())) {
            IteratorDetails argIter = compiler.compileToIterator(arg);
            ga.dup();
            ga.invokeInstanceMethod(argIter.iteratorClass, "next");
            ga.ifNull(returnFalse.label());
            ga.invokeInstanceMethod(argIter.iteratorClass, "close");
            ga.push(true);
            ga.goTo(end);
            methodInfo.placeLabel(returnFalse);
            ga.pop();
            ga.push(false);
            methodInfo.placeLabel(end);
        } else {
            compiler.compileToItem(arg);
            ga.ifNull(returnFalse.label());
            ga.push(true);
            ga.goTo(end);
            methodInfo.placeLabel(returnFalse);
            ga.push(false);
            methodInfo.placeLabel(end);
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


