package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;

/**
 * Generate code for a String to Integer CastExpression
 */
public class StringToIntegerCompiler extends ConverterCompiler.GenericConverterCompiler {
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "StringToIntegerConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compiler.compileToPrimitive(base, String.class, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(IntegerValue.class, "stringToInteger", String.class);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            ga.checkClass(IntegerValue.class);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compiler.compileToPrimitive(base, String.class, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(IntegerValue.class, "stringToInteger", String.class);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            ga.checkClass(IntegerValue.class);
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited