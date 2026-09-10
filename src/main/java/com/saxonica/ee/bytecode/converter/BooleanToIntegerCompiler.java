package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.Int64Value;
import org.objectweb.asm.Type;

import java.util.Stack;

/**
 * Generate code for a Boolean to Integer CastExpression
 */
public class BooleanToIntegerCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
//        final Expression base = cast.getBaseExpression();
//        final AtomicType targetType = cast.getTargetType();
//        final Converter converter = cast.getConverter();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
//        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
//        AtomicType sourceType = (AtomicType) base.getItemType();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "BooleanToIntegerConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Boolean.TYPE, new OnEmpty.UnwindAndJump(new Stack<>(), empty));
            ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
            ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Boolean.TYPE, OnEmpty.RETURN_NULL);
            ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
            ga.invokeStaticMethod(Int64Value.class, "makeIntegerValue", Long.TYPE);
        }
        return Int64Value.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
//        final Generator ga = compiler.getCurrentGenerator();
//        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "BooleanToIntegerConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToBoolean(base);
        // Following code removed because bytecode does not distinguish true/false from 1/0
//        LabelInfo isTrue = methodInfo.newLabel("isTrue");
//        LabelInfo makeItem = methodInfo.newLabel("makeitem");
//        LabelInfo end = methodInfo.newLabel("castEnd");
//        ga.ifTrue(isTrue);
//        ga.push(0);
//        ga.goTo(end);
//        methodInfo.placeLabel(isTrue);
//        ga.push(1);
//        ga.goTo(end);
//        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited