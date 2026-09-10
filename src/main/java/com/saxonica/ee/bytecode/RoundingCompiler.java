package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.NumericValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Compiler for the functions floor(), ceiling(), round(), round-half-to-even(), and abs()
 */

public class RoundingCompiler extends ToItemCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        SystemFunction rounding = fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notNull = methodInfo.newLabel("roundNotNull");
        LabelInfo end = methodInfo.newLabel("endRound");
        compiler.compileToItem(fnc.getArg(0));
        ga.checkClass(AtomicValue.class);
        ga.dup();
        ga.ifNonNull(notNull.label());
        ga.pop();
        ga.pushNull();
        ga.goTo(end);

        methodInfo.placeLabel(notNull);
        ga.checkClass(NumericValue.class);
        int valVar = methodInfo.allocateLocal(NumericValue.class);
        ga.storeLocal(valVar);
        if (rounding instanceof Floor) {
            ga.loadLocal(valVar);
            ga.invokeInstanceMethod(NumericValue.class, "floor");
        } else if (rounding instanceof Ceiling) {
            ga.loadLocal(valVar);
            ga.invokeInstanceMethod(NumericValue.class, "ceiling");
        } else if (rounding instanceof Round) {
            // TODO: need to check for out-of-range values of second argument, as with round-half-to-even.
            ga.loadLocal(valVar);
            if (rounding.getArity() == 2) {
                compiler.compileToItem(fnc.getArg(1));
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "longValue");
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            } else {
                ga.push(0);
            }
            ga.invokeInstanceMethod(NumericValue.class, "round", Integer.TYPE);

        } else if (rounding instanceof RoundHalfToEven) {
            ga.loadLocal(valVar);
            LabelInfo doRounding = null;
            if (rounding.getArity() == 2) {
                compiler.compileToItem(fnc.getArg(1));
                ga.checkClass(NumericValue.class);
                IntegerValue[] bounds = fnc.getArg(1).getIntegerBounds();
                if (bounds == null || bounds[0].compareTo(Integer.MAX_VALUE) > 0) {
                    LabelInfo notTooBig = methodInfo.newLabel("notTooBig");
                    ga.dup();
                    ga.push((long) Integer.MAX_VALUE);
                    ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
                    ga.ifZCmp(GeneratorAdapter.LE, notTooBig.label());
                    ga.pop();
                    ga.goTo(end);
                    methodInfo.placeLabel(notTooBig);
                }
                if (bounds == null || bounds[0].compareTo(Integer.MIN_VALUE) < 0) {
                    LabelInfo notTooSmall = methodInfo.newLabel("notTooBig");
                    doRounding = methodInfo.newLabel("doRounding");
                    ga.dup();
                    ga.push((long) Integer.MIN_VALUE);
                    ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
                    ga.ifZCmp(GeneratorAdapter.GE, notTooSmall.label());
                    ga.pop();
                    ga.push(Integer.MIN_VALUE);
                    ga.goTo(doRounding);
                    methodInfo.placeLabel(notTooSmall);
                }
                ga.invokeInstanceMethod(NumericValue.class, "longValue");
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            } else {
                ga.push(0);
            }
            if (doRounding != null) {
                methodInfo.placeLabel(doRounding);
            }
            ga.invokeInstanceMethod(NumericValue.class, "roundHalfToEven", Integer.TYPE);
        } else if (rounding instanceof Abs) {
            ga.loadLocal(valVar);
            ga.invokeInstanceMethod(NumericValue.class, "abs");
        } else {
            throw new UnsupportedOperationException("Unknown rounding function");
        }
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(valVar);

    }

}

// Copyright (c) 2011-2022 Saxonica Limited
