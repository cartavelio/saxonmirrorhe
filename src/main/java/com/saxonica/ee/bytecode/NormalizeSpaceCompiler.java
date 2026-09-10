package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.Whitespace;

/**
 * Generate bytecode for evaluation of a call to the normalize-space() function
 */
public class NormalizeSpaceCompiler extends ToStringCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {

        final SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "NormalizeSpace-str");

        formInputString(compiler, expression, requiredClass, fnc, ga, methodInfo);

        ga.invokeStaticMethod(Whitespace.class, "collapseWhitespace", CharSequence.class);
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(CharSequence.class, "toString");
        }

    }

    private void formInputString(CompilerService compiler, Expression expression, Class requiredClass, SystemFunctionCall fnc, Generator ga, GeneratedMethodInfo methodInfo) throws CannotCompileException {
        if (fnc.getArity() == 0) {

            LabelInfo nonNull = methodInfo.newLabel("nonNull");
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
            ga.dup();
            ga.ifNonNull(nonNull.label());
            ga.pop();
            compiler.generateDynamicError("Context item for normalize-space() is absent", "XPDY0002", expression.getLocation(), false);
            methodInfo.placeLabel(nonNull);
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(CharSequence.class, "toString");
            }

        } else {

            compiler.compileToPrimitive(fnc.getArg(0), CharSequence.class, OnEmpty.RETURN_EMPTY_STRING);
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(CharSequence.class, "toString");
            }

        }
    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        visitAnnotation(compiler, "NormalizeSpace-str");

        Generator ga = compiler.getCurrentGenerator();
        formInputString(compiler,
                        expression,
                        CharSequence.class,
                        (SystemFunctionCall) expression,
                        ga,
                        compiler.getCurrentMethod());

        //return !Whitespace.isWhite(cs);
        ga.invokeStaticMethod(Whitespace.class, "isWhite", CharSequence.class);
        ga.not();
    }
}

//Copyright (c) 2011-2018 Saxonica Limited.
