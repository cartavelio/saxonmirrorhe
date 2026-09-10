package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;

/**
 * Generate code for a Double to Integer CastExpression
 */
public class DoubleToIntegerCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToIntegerConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compiler.compileToPrimitive(base, Double.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(IntegerValue.class, "fromDouble", Double.TYPE);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compiler.compileToPrimitive(base, Double.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(IntegerValue.class, "fromDouble", Double.TYPE);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
        }
        return IntegerValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToIntegerConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToPrimitive(base, Double.TYPE, onEmpty);
        ga.invokeStaticMethod(Callback.class, "doubleToIntegerOrError", Double.TYPE);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited