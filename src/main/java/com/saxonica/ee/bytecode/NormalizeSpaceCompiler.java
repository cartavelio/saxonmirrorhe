package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Whitespace;

/**
 * Generate bytecode for evaluation of a call to the normalize-space() function
 */
public class NormalizeSpaceCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {

        final SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "NormalizeSpace-str");

        formInputString(compiler, expression, requiredClass, fnc, ga, methodInfo);
        ga.invokeStaticMethod(Whitespace.class, "collapseWhitespace", requiredClass);
        
    }

    private void formInputString(CompilerService compiler, Expression expression, Class<?> requiredClass, SystemFunctionCall fnc, Generator ga, GeneratedMethodInfo methodInfo) throws CannotCompileException {
        if (fnc.getArity() == 0) {

            LabelInfo nonNull = methodInfo.newLabel("nonNull");
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
            ga.dup();
            ga.ifNonNull(nonNull.label());
            ga.pop();
            compiler.generateDynamicError("Context item for normalize-space() is absent", "XPDY0002", expression.getLocation(), false);
            methodInfo.placeLabel(nonNull);
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(Item.class, "getStringValue");
            } else {
                ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
            }

        } else {

            compiler.compileToPrimitive(fnc.getArg(0), requiredClass,
                                        requiredClass == String.class ? OnEmpty.RETURN_EMPTY_STRING : OnEmpty.RETURN_EMPTY_UNICODE_STRING);

        }
    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        visitAnnotation(compiler, "NormalizeSpace-str");

        Generator ga = compiler.getCurrentGenerator();
        formInputString(compiler,
                        expression,
                        UnicodeString.class,
                        (SystemFunctionCall) expression,
                        ga,
                        compiler.getCurrentMethod());

        //return !Whitespace.isWhite(cs);
        ga.invokeStaticMethod(Whitespace.class, "isAllWhite", UnicodeString.class);
        ga.not();
    }
}

// Copyright (c) 2011-2020 Saxonica Limited
