package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.StringValue;

import java.util.Stack;

/**
 * Generate code for a Double to UntypedAtomic CastExpression
 */
public class DoubleToUntypedAtomicCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToUntypedAtomicConverter-Item");
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
        return StringValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "DoubleToUntypedAtomicConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }

        compiler.compileToPrimitive(base, Double.TYPE, onEmpty);
        ga.invokeStaticMethod(DoubleValue.class, "doubleToString", Double.TYPE);
        if (requiredClass == String.class) {
            ga.invokeInstanceMethod(UnicodeString.class, "toString");
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited