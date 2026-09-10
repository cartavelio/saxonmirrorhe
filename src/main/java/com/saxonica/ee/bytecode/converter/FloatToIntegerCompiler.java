package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;
import org.objectweb.asm.Type;

/**
 * Generate code for a Float to Integer CastExpression
 */
public class FloatToIntegerCompiler extends ConverterCompiler {
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
        ExpressionCompiler.visitAnnotation(compiler, "FloatToIntegerConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compiler.compileToPrimitive(base, Float.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);
            ga.invokeStaticMethod(IntegerValue.class, "fromDouble", Double.TYPE);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compiler.compileToPrimitive(base, Float.TYPE, OnEmpty.RETURN_NULL);
            ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);
            ga.invokeStaticMethod(IntegerValue.class, "fromDouble", Double.TYPE);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
        }
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToIntegerConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToPrimitive(base, Float.TYPE, onEmpty);
        ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);
        ga.invokeStaticMethod(Callback.class, "doubleToIntegerOrError", Double.TYPE);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited