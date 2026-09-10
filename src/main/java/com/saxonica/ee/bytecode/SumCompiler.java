package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.functions.Sum;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.Int64Value;


/**
 * Generate bytecode for the sum function
 */
public class SumCompiler extends ToItemCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Sum sum = (Sum) fnc.getTargetFunction();
        Expression operand0 = fnc.getArg(0);
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        LabelInfo end = methodInfo.newLabel("endSum");
        compiler.compileToIterator(operand0);
        compiler.generateGetContext();
        allocateStatic(compiler, fnc.getLocation());
        ga.checkClass(Location.class);
        ga.invokeStaticMethod(Sum.class, "total", SequenceIterator.class, XPathContext.class, Location.class);
        ga.dup();
        ga.ifNonNull(end.label());
        ga.pop();
        if (sum.getArity() == 2) {
            compiler.compileToItem(fnc.getArg(1));
        } else {
            ga.getStaticField(Int64Value.class, "ZERO", Int64Value.class);
        }
        methodInfo.placeLabel(end);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
