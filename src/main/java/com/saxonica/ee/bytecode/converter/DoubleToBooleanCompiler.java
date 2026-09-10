package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.Type;

/**
 * Generate code for a Double to Boolean CastExpression
 */
public class DoubleToBooleanCompiler extends ConverterCompiler {
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
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToBooleanConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Float.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Float.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
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
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToBooleanConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToPrimitive(base, Double.TYPE, onEmpty);
        LabelInfo checkNaN = methodInfo.newLabel("checkForNaN");
        LabelInfo isTrue = methodInfo.newLabel("isTrue_cast");
        LabelInfo end = methodInfo.newLabel("castEnd");
        ga.dup2();
        ga.push(0.0);
        ga.ifCmp(Type.DOUBLE_TYPE, Generator.NE, checkNaN.label());
        ga.pop2();

        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(checkNaN);
        ga.invokeStaticMethod(Double.class, "isNaN", Double.TYPE);
        ga.ifFalse(isTrue);

        ga.push(false);
        ga.goTo(end);
        methodInfo.placeLabel(isTrue);
        ga.push(true);
        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited