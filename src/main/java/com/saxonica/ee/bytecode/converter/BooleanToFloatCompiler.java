package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.FloatValue;

/**
 * Generate code for a Boolean to Float CastExpression
 */

public class BooleanToFloatCompiler extends ConverterCompiler {
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

        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Boolean.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(FloatValue.class, "makeFloatValue", Float.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Boolean.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(FloatValue.class, "makeFloatValue", Float.TYPE);
        }
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final AtomicType targetType = cast.getTargetType();
        final Converter converter = cast.getConverter();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        AtomicType sourceType = (AtomicType) base.getItemType();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());

        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToBoolean(base);
        LabelInfo isTrue = methodInfo.newLabel("isTrue");
        LabelInfo makeItem = methodInfo.newLabel("makeitem");
        LabelInfo end = methodInfo.newLabel("castEnd");
        ga.ifTrue(isTrue);
        ga.push((float) 0);
        ga.goTo(end);
        methodInfo.placeLabel(isTrue);
        ga.push((float) 1);
        ga.goTo(end);

        methodInfo.placeLabel(end);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited