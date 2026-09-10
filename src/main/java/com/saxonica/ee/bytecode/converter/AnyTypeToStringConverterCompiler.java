package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.StringValue;

/**
 * Generate code for a AnyType to String CastExpression
 */
public class AnyTypeToStringConverterCompiler extends ConverterCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        ExpressionCompiler.visitAnnotation(compiler, "AnyTypeToStringConverter-Item");
        compileToPrimitive(compiler, expression, UnicodeString.class, OnEmpty.RETURN_NULL);
        if (Cardinality.allowsZero(expression.getCardinality())) {
            final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            LabelInfo end = methodInfo.newLabel("endAnyTypeToItem");
            ga.dup();
            ga.ifNull(end.label());
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
            methodInfo.placeLabel(end);
        } else {
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
        }
        return StringValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        compiler.compileToItem(base);
        ExpressionCompiler.visitAnnotation(compiler, "AnyTypeToStringConverter-Primitive");
        if (allowsEmpty) {
            LabelInfo end = methodInfo.newLabel("endAnyTypeToStr");
            LabelInfo notNull = methodInfo.newLabel("notNull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            onEmpty.generate(ga);
            ga.goTo(end);
            methodInfo.placeLabel(notNull);
            generateNonNullCode(requiredClass, ga);
            methodInfo.placeLabel(end);
        } else {
            generateNonNullCode(requiredClass, ga);
        }
    }

    private void generateNonNullCode(Class<?> requiredClass, Generator ga) {
        ga.checkClass(AtomicValue.class);
        ga.invokeInstanceMethod(AtomicValue.class, requiredClass == String.class ? "getStringValue" : "getUnicodeStringValue");
    }
}

// Copyright (c) 2011-2022 Saxonica Limited