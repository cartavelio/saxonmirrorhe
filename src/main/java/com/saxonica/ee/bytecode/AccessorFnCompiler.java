package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;

/**
 * Generate bytecode for evaluation of accessor functions such as day-from-date, seconds-from-duration,
 * namespace-uri=from-QName
 */
public class AccessorFnCompiler extends ToItemCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        AccessorFn fn = (AccessorFn)fnc.getTargetFunction();
        visitAnnotation(compiler, "AccessorFn Expr:" + fn.getFunctionName().getDisplayName());
        LabelInfo accessorReturn = methodInfo.newLabel("accessorReturn");
        compiler.compileToItem(fnc.getArg(0));
        if (Cardinality.allowsZero(fnc.getArg(0).getCardinality())) {
            ga.dup();
            ga.ifNull(accessorReturn.label());
        }
        ga.checkClass(AtomicValue.class);
        allocateStatic(compiler, fn.getComponentId());
        ga.invokeInstanceMethod(AtomicValue.class, "getComponent", AccessorFn.Component.class);
        methodInfo.placeLabel(accessorReturn);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
