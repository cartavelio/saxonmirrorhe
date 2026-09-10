package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

/**
 * Generate code for a Integer to String CastExpression
 */
public class IntegerToStringCompiler extends ConverterCompiler {
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final AtomicType targetType = cast.getTargetType();
        final Converter converter = cast.getConverter();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        AtomicType sourceType = (AtomicType) base.getItemType();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToStringConverter-Item");

        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, UnicodeString.class, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, UnicodeString.class, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
        }
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToStringConverter-Primitive");
        LabelInfo end = methodInfo.newLabel("end");
        LabelInfo notNull = methodInfo.newLabel("notNullIntToStr");

        if (compiler.isInRangeForLong(base)) {
            compiler.compileToPrimitive(base, Long.TYPE, onEmpty);
            ga.invokeStaticMethod(Long.class, "toString", Long.TYPE);
            if (requiredClass == UnicodeString.class) {
                ga.invokeStaticMethod(BMPString.class, "of", String.class);
            }
        } else {
            compiler.compileToItem(base);
            if (allowsEmpty) {
                ga.dup();
                ga.ifNonNull(notNull.label());
                ExpressionCompiler.handleEmptyStringResult(onEmpty, ga, methodInfo, end);
                methodInfo.placeLabel(notNull);
            }

            ga.checkClass(IntegerValue.class);
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(IntegerValue.class, "getStringValue");
            } else {
                ga.invokeInstanceMethod(IntegerValue.class, "getUnicodeStringValue");
            }
        }
        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited