package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.FloatValue;
import net.sf.saxon.value.IntegerValue;
import org.objectweb.asm.Type;

import java.util.Stack;

/**
 * Generate code for a Integer to Float CastExpression
 */

public class IntegerToFloatCompiler extends ConverterCompiler {
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
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToFloatConverter-Item");
        ga.newInstance(Type.getType(FloatValue.class));
        ga.dup();
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            Stack<Class<?>> classStack = new Stack<>();
            classStack.push(FloatValue.class);
            classStack.push(FloatValue.class);
            OnEmpty.UnwindAndJump unwindAndJump = new OnEmpty.UnwindAndJump(classStack, empty);
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Long.TYPE, unwindAndJump);
            ga.invokeConstructor(FloatValue.class, Float.TYPE);
            unwindAndJump.getUnwindStack().pop();
            unwindAndJump.getUnwindStack().pop();
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Long.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeConstructor(FloatValue.class, Float.TYPE);
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
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToFloatConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        if (compiler.isInRangeForLong(base)) {
            compiler.compileToPrimitive(base, Long.TYPE, onEmpty);
            ga.cast(Type.LONG_TYPE, Type.FLOAT_TYPE);
        } else {
            compiler.compileToItem(base);
            ga.checkClass(IntegerValue.class);
            ga.invokeInstanceMethod(IntegerValue.class, "getDoubleValue");
            ga.cast(Type.DOUBLE_TYPE, Type.FLOAT_TYPE);
        }

    }
}

// Copyright (c) 2011-2022 Saxonica Limited