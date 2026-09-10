package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.Converter;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.StringValue;

/**
 * Generate code for a Double to String CastExpression
 */
public class DoubleToStringCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToStringConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, UnicodeString.class, new OnEmpty.UnwindAndJump(empty));
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, UnicodeString.class, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
        }
        return StringValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Converter converter = cast.getConverter();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToStringConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            LabelInfo exit = methodInfo.newLabel("exit_DoubleToStringPrim");
            compiler.compileToItem(base);
            ga.dup();
            ga.ifNonNull(exit.label());
            ExpressionCompiler.handleEmptyStringResult(onEmpty, ga, methodInfo, exit);
            methodInfo.placeLabel(exit);
        } else {
            compiler.compileToPrimitive(base, Double.TYPE, onEmpty);
            ga.invokeStaticMethod(DoubleValue.class, "doubleToString", Double.TYPE);
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(UnicodeString.class, "toString");
            }
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited