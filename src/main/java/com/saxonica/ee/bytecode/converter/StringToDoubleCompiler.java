package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;

/**
 * Generate code for a String to Double CastExpression
 */
public class StringToDoubleCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToDoubleConverter-Item");
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
        return DoubleValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final AtomicType targetType = cast.getTargetType();
        final Converter converter = cast.getConverter();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        AtomicType sourceType = (AtomicType) base.getItemType();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToDoubleConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        StringToDouble stringToDouble = compiler.getConfiguration().getConversionRules().getStringToDoubleConverter();

        /* if ((onEmpty instanceof OnEmpty.UnwindAndJump)) {
            ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().push(StringToDouble.class);
        }*/
        compiler.compileToPrimitive(base, UnicodeString.class, onEmpty);
        final int strVar = ga.newLocal(UnicodeString.class);
        ga.storeLocal(strVar);
        LabelInfo L0 = methodInfo.newLabel("L0_castExpr");
        LabelInfo L1 = methodInfo.newLabel("L1_castExpr");
        LabelInfo L2 = methodInfo.newLabel("L2_castExpr");
        LabelInfo done = methodInfo.newLabel("doneCast");
        ga.visitTryCatchBlock(L0, L1, L2, "java/lang/NumberFormatException");
        /* try block*/
        methodInfo.placeLabel(L0);
        ExpressionCompiler.allocateStatic(compiler, stringToDouble);
        ga.loadLocal(strVar);
        ga.invokeInstanceMethod(net.sf.saxon.type.StringToDouble.class, "stringToNumber", UnicodeString.class);
        methodInfo.placeLabel(L1);
        ga.goTo(done.label());
        /*catch block*/
        methodInfo.placeLabel(L2);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Cannot cast string to double: {$1}", strVar),
                "FORG0001", cast.getLocation(), false);
        methodInfo.placeLabel(done);
        methodInfo.releaseLocal(strVar);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited