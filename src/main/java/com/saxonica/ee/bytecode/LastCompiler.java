package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.PositionAndLast;
import org.objectweb.asm.Type;

/**
 * Generate code for a call to last()
 */
public class LastCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) {
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        PositionAndLast.Last fn = (PositionAndLast.Last)fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "Last");
        if (fn.isContextPossiblyUndefined()) {
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
            LabelInfo lab = methodInfo.newLabel("lab");
            ga.ifNonNull(lab.label());
            // Throw error - context position is undefined
            compiler.generateDynamicError("The context for last() is absent", "XPDY0002", expression.getLocation(), false);
            methodInfo.placeLabel(lab);
        }
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getLast");
        if (requiredClass == Long.TYPE) {
            ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
        }
    }

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileItemFromInt(compiler, expression);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


