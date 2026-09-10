package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.FloatValue;
import org.objectweb.asm.Type;

/**
 * Generate code for a String to Float CastExpression
 */
public class StringToFloatCompiler extends ConverterCompiler {
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToFloatConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, Float.TYPE, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(FloatValue.class, "makeFloatValue", Float.TYPE);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, Float.TYPE, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(FloatValue.class, "makeFloatValue", Float.TYPE);
        }
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToFloatConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }

        compiler.compileToPrimitive(base, UnicodeString.class, onEmpty);
        final int strValue = methodInfo.allocateLocal(UnicodeString.class);
        ga.storeLocal(strValue);
        ExpressionCompiler.allocateStatic(compiler, compiler.getConfiguration().getConversionRules().getStringToDoubleConverter());
        ga.loadLocal(strValue);

        LabelInfo L0 = methodInfo.newLabel("L0_castExpr");
        LabelInfo L1 = methodInfo.newLabel("L1_castExpr");
        LabelInfo L2 = methodInfo.newLabel("L2_castExpr");
        LabelInfo done = methodInfo.newLabel("doneCast");
        ga.visitTryCatchBlock(L0, L1, L2, "java/lang/NumberFormatException");
        /* try block*/
        methodInfo.placeLabel(L0);
        ga.invokeInstanceMethod(net.sf.saxon.type.StringToDouble.class, "stringToNumber", UnicodeString.class);
        ga.cast(Type.DOUBLE_TYPE, Type.FLOAT_TYPE);
        methodInfo.placeLabel(L1);
        ga.goTo(done.label());
        /*catch block*/
        methodInfo.placeLabel(L2);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Cannot cast string to float: {$1}", strValue),
                "FORG0001", cast.getLocation(), false);
        methodInfo.placeLabel(done);
        methodInfo.releaseLocal(strValue);

    }
}

// Copyright (c) 2011-2022 Saxonica Limited