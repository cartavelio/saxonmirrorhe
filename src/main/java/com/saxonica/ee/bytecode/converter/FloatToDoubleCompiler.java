package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import org.objectweb.asm.Type;

import java.util.Stack;

/**
 * Generate code for a Float to Double CastExpression
 */

public class FloatToDoubleCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "FloatToDoubleConverter-Item");
        ga.newInstance(DoubleValue.class);
        ga.dup();
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            Stack<Class<?>> classStack = new Stack<>();
            classStack.push(DoubleValue.class);
            classStack.push(DoubleValue.class);
            OnEmpty.UnwindAndJump unwindAndJump = new OnEmpty.UnwindAndJump(classStack, empty);
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Float.TYPE, new OnEmpty.UnwindAndJump(empty));
            //   ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);
            ga.invokeConstructor(DoubleValue.class, Double.TYPE);
            unwindAndJump.getUnwindStack().pop();
            unwindAndJump.getUnwindStack().pop();
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Float.TYPE, OnEmpty.RETURN_NULL);
            //  ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);
            ga.invokeConstructor(DoubleValue.class, Double.TYPE);
        }
        return DoubleValue.class;
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
        ExpressionCompiler.visitAnnotation(compiler, "FloatToDoubleConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToPrimitive(base, Float.TYPE, onEmpty);
        ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited