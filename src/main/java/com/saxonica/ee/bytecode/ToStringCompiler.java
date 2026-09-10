package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.StringValue;

/**
 * Generate bytecode for evaluation of expressions that return a single string (or an empty sequence)
 */
public abstract class ToStringCompiler extends ToItemCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public final Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileItemFromString(compiler, expression);
        return StringValue.class;
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
            compileToPrimitive(compiler, expression, UnicodeString.class, new OnEmpty.UnwindAndJump(returnNull));
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
            ga.goTo(exit);
            methodInfo.placeLabel(returnNull);
            ga.pushNull();
            methodInfo.placeLabel(exit);
        } else {
            compileToPrimitive(compiler, expression, UnicodeString.class, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
        }

    }

    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass,
                                   OnEmpty onEmpty) throws CannotCompileException {
        if (requiredClass == UnicodeString.class) {
            compileToUnicodeString(compiler, expression, onEmpty);
        } else if (requiredClass == String.class) {
            compileToString(compiler, expression, onEmpty);
        }
    }

    // All subclasses must be able to compile to a UnicodeString
    public void compileToUnicodeString(CompilerService compiler, Expression expression, OnEmpty onEmpty)
            throws CannotCompileException {
        throw new CannotCompileException(expression, "CompileToUnicodeString not implemented");
    }

    // Some classes may also be able to compile directly to a String; those that can't will compile to a
    // UnicodeString and convert the result
    public void compileToString(CompilerService compiler, Expression expression, OnEmpty onEmpty)
            throws CannotCompileException {
        compileToUnicodeString(compiler, expression,
                               onEmpty == OnEmpty.RETURN_EMPTY_STRING ? OnEmpty.RETURN_EMPTY_UNICODE_STRING : onEmpty);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeInstanceMethod(UnicodeString.class, "toString");
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
