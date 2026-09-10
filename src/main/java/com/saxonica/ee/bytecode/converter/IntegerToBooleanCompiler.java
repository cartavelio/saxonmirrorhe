package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;
import org.objectweb.asm.Type;

/**
 * Generate code for a Integer to Boolean CastExpression
 */
public class IntegerToBooleanCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToBooleanConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Long.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Float.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
        }
        return BooleanValue.class;
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
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToBooleanConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        if (compiler.isInRangeForLong(base)) {
            compiler.compileToPrimitive(base, Long.TYPE, onEmpty);
        } else {
            Class<? extends Item> baseClass = compiler.compileToItem(base);
            baseClass = ga.downCast(baseClass, IntegerValue.class, true);
            ga.invokeInstanceMethod(baseClass, "longValue");
        }
        LabelInfo isTrue = methodInfo.newLabel("isTrue");
        LabelInfo end = methodInfo.newLabel("castBooleanEnd");
        ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
        ga.ifTrue(isTrue);
        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(isTrue);
        ga.push(true);
        methodInfo.placeLabel(end);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited