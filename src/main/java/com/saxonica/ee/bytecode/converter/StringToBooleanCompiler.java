package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate code for a String to Boolean CastExpression
 */
public class StringToBooleanCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToBooleanConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Boolean.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Integer.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
        }
        return BooleanValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToBooleanConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }

        LabelInfo end = methodInfo.newLabel("castEnd");

        compiler.compileToPrimitive(base, UnicodeString.class, onEmpty);
        final int strVar = ga.newLocal(UnicodeString.class);
        ga.storeLocal(strVar);
        ga.loadLocal(strVar);
        ga.invokeStaticMethod(Callback.class, "booleanFromString", UnicodeString.class);
        ga.dup();
        ga.ifZCmp(GeneratorAdapter.GE, end.label());
        ga.pop();
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Cannot cast string to boolean: {$1}", strVar),
                "FORG0001", cast.getLocation(), false);
        methodInfo.placeLabel(end);
        methodInfo.releaseLocal(strVar);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited