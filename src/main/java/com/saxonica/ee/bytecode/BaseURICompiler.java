package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.value.AnyURIValue;

/**
 * Generate bytecode for evaluation of a call to the BaseURI function
 */
public class BaseURICompiler extends ToItemCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, /*@NotNull*/ Expression expression) throws CannotCompileException {

        Expression arg0 = ((SystemFunctionCall) expression).getArg(0);
        Class<? extends Item> operandClass = compiler.compileToItem(arg0);
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo nullLab = methodInfo.newLabel("notnull_BaseURI");
        LabelInfo end = methodInfo.newLabel("end_BaseURI");
        operandClass = ga.downCast(operandClass, NodeInfo.class);
        ga.dup();
        ga.ifNull(nullLab.label());
        ga.invokeInstanceMethod(operandClass, "getBaseURI");
        ga.dup();
        ga.ifNull(nullLab.label());
        ga.newInstance(AnyURIValue.class);
        ga.dupX1();
        ga.swap();
        ga.invokeConstructor(AnyURIValue.class, String.class);
        ga.goTo(end);

        methodInfo.placeLabel(nullLab);
        ga.pop();
        ga.push((String) null);
        methodInfo.placeLabel(end);
        return AnyURIValue.class;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited