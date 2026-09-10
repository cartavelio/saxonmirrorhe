package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.StringLength_1;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;

/**
 * Generate bytecode for a call to the StringLength Function
 */
public class StringLengthCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty)
            throws CannotCompileException {

        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final StringLength_1 fn = (StringLength_1)fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "StringLengthCompiler");
        if (fn.getArity() == 0) {
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
            if (true /*|| fn.isContextPossiblyUndefined()*/) {
                ga.dup();
                LabelInfo notNull = methodInfo.newLabel("notNullLen");
                ga.ifNonNull(notNull.label());
                compiler.generateDynamicError("The context item for string-length() is absent", "XPDY0002", expression.getLocation(), false);
                methodInfo.placeLabel(notNull);
            }
            ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
        } else {
            compiler.compileToPrimitive(fnc.getArg(0), UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
        }

        ga.invokeInstanceMethod(UnicodeString.class, requiredClass == Long.TYPE ? "length" : "length32");

    }

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
        compileItemFromInt(compiler, expression);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
