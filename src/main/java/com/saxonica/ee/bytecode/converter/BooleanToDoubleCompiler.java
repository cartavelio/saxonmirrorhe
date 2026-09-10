package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;

/**
 * Bytecode generator for a cast from boolean to double
 */
public class BooleanToDoubleCompiler extends ConverterCompiler {
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "BooleanToDoubleConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Double.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(DoubleValue.class, "makeDoubleValue", Double.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Double.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(DoubleValue.class, "makeDoubleValue", Double.TYPE);
        }
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "BooleanToDoubleConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToBoolean(base);
        LabelInfo isTrue = methodInfo.newLabel("isTrue");
        LabelInfo end = methodInfo.newLabel("castEnd");
        ga.ifTrue(isTrue);
        ga.push(0.0);
        ga.goTo(end);
        methodInfo.placeLabel(isTrue);
        ga.push(1.0);
        ga.goTo(end);
        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited