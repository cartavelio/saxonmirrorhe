package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.StringValue;

import java.util.Stack;

/**
 * Generate code for a Integer to Untyped Atomic CastExpression
 */
public class IntegerToUntypedAtomicCompiler extends ConverterCompiler {
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final AtomicType targetType = cast.getTargetType();
        final Converter converter = cast.getConverter();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToUntypedAtomicConverter-Item");
        if (allowsEmpty) {
            LabelInfo empty = methodInfo.newLabel("returnEmpty");
            Stack<Class<?>> classStack = new Stack<>();
            classStack.push(StringValue.class);
            classStack.push(StringValue.class);
            OnEmpty.UnwindAndJump unwindAndJump = new OnEmpty.UnwindAndJump(classStack, empty);
            LabelInfo end = methodInfo.newLabel("end");
            compileToPrimitive(compiler, expression, UnicodeString.class, unwindAndJump);
            ga.invokeStaticMethod(StringValue.class, "makeUntypedAtomic", UnicodeString.class);
            unwindAndJump.getUnwindStack().pop();
            unwindAndJump.getUnwindStack().pop();
            ga.goTo(end);
            methodInfo.placeLabel(empty);
            ga.pushNull();
            methodInfo.placeLabel(end);
        } else {
            compileToPrimitive(compiler, expression, UnicodeString.class, OnEmpty.RETURN_NULL);
            ga.invokeStaticMethod(StringValue.class, "makeUntypedAtomic", UnicodeString.class);
        }
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "IntegerToUntypedAtomicConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        if (compiler.isInRangeForLong(base)) {
            compiler.compileToPrimitive(base, Long.TYPE, onEmpty);
            ga.invokeStaticMethod(Long.class, "toString", Long.TYPE);
            if (requiredClass == UnicodeString.class) {
                ga.invokeStaticMethod(BMPString.class, "of", String.class);
            }
        } else {
            compiler.compileToItem(base);
            ga.checkClass(IntegerValue.class);
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(IntegerValue.class, "getStringValue");
            } else {
                ga.invokeInstanceMethod(IntegerValue.class, "getUnicodeStringValue");
            }
        }

    }
}

// Copyright (c) 2011-2022 Saxonica Limited