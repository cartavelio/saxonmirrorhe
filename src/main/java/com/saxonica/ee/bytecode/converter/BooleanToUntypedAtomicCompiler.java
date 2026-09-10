package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.StringValue;

import java.util.Stack;

/**
 * Generate code for a Boolean to UntypedAtomic CastExpression
 */
public class BooleanToUntypedAtomicCompiler extends ConverterCompiler {
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "BooleanToUntypedAtomicConverter-Item");
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
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        boolean allowsEmpty = Cardinality.allowsZero(cast.getCardinality());
        ExpressionCompiler.visitAnnotation(compiler, "BooleanToUntypedAtomicConverter-Primitive");
        if (allowsEmpty && !(onEmpty instanceof OnEmpty.UnwindAndJump)) {
            throw new IllegalArgumentException();
        }
        compiler.compileToBoolean(base);
        LabelInfo isTrue = methodInfo.newLabel("isTrue");
        LabelInfo end = methodInfo.newLabel("castEnd");
        ga.ifTrue(isTrue);
        ga.getStaticField(StringConstants.class, "FALSE", UnicodeString.class);
        ga.goTo(end);
        methodInfo.placeLabel(isTrue);
        ga.getStaticField(StringConstants.class, "TRUE", UnicodeString.class);
        ga.goTo(end);
        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited