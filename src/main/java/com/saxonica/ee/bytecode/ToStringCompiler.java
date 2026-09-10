package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.value.*;

/**
 * Generate bytecode for evaluation of expressions that return a single string (or an empty sequence)
 */
public abstract class ToStringCompiler extends ToItemCompiler {

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
    public final void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileItemFromString(compiler, expression);
    }

    /**
     * Helper method to implement compileFromItem when the class implements compileToPrimitive
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    protected final void compileItemFromString(CompilerService compiler, Expression expression) throws CannotCompileException {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Generator ga = compiler.getCurrentGenerator();

        if (Cardinality.allowsZero(expression.getCardinality())) {
            LabelInfo returnNull = methodInfo.newLabel("returnNull");
            LabelInfo exit = methodInfo.newLabel("exit");
            compileToPrimitive(compiler, expression, CharSequence.class, new OnEmpty.UnwindAndJump(returnNull));
            ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);
            ga.goTo(exit);
            methodInfo.placeLabel(returnNull);
            ga.pushNull();
            methodInfo.placeLabel(exit);
        } else {
            compileToPrimitive(compiler, expression, CharSequence.class, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);
        }

    }


}

// Copyright (c) 2011-2020 Saxonica Limited
