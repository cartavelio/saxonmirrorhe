package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.NumericValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Compiler for the functions floor(), ceiling(), round(), round-half-to-even(), and abs()
 */

public class RoundingCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        SystemFunction rounding = fnc.getTargetFunction();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notNull = methodInfo.newLabel("roundNotNull");
        LabelInfo end = methodInfo.newLabel("endRound");
        Class<? extends Item> operandClass = compiler.compileToItem(fnc.getArg(0));
        operandClass = ga.downCast(operandClass, NumericValue.class, true);
        //ga.checkClass(AtomicValue.class);
        ga.dup();
        ga.ifNonNull(notNull.label());
        ga.pop();
        ga.pushNull();
        ga.goTo(end);

        methodInfo.placeLabel(notNull);
        //ga.checkClass(NumericValue.class);
        int valVar = methodInfo.allocateLocal(operandClass);
        ga.storeLocal(valVar);
        if (rounding instanceof Floor) {
            ga.loadLocal(valVar);
            ga.invokeInstanceMethod(operandClass, "floor");
        } else if (rounding instanceof Ceiling) {
            ga.loadLocal(valVar);
            ga.invokeInstanceMethod(operandClass, "ceiling");
        } else if (rounding instanceof Round) {
            // TODO: need to check for out-of-range values of second argument, as with round-half-to-even.
            ga.loadLocal(valVar);
            if (rounding.getArity() == 2) {
                Class<? extends Item> secondOperandClass = compiler.compileToItem(fnc.getArg(1));
                secondOperandClass = ga.downCast(secondOperandClass, NumericValue.class, true);
                //ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(secondOperandClass, "longValue");
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            } else {
                ga.push(0);
            }
            ga.invokeInstanceMethod(operandClass, "round", Integer.TYPE);

        } else if (rounding instanceof RoundHalfToEven) {
            ga.loadLocal(valVar);
            LabelInfo doRounding = null;
            if (rounding.getArity() == 2) {
                Class<? extends Item> secondOperandClass = compiler.compileToItem(fnc.getArg(1));
                secondOperandClass = ga.downCast(secondOperandClass, NumericValue.class, true);
                IntegerValue[] bounds = fnc.getArg(1).getIntegerBounds();
                if (bounds == null || bounds[0].compareTo(Integer.MAX_VALUE) > 0) {
                    LabelInfo notTooBig = methodInfo.newLabel("notTooBig");
                    ga.dup();
                    ga.push((long) Integer.MAX_VALUE);
                    ga.invokeInstanceMethod(secondOperandClass, "compareTo", Long.TYPE);
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
            ga.invokeInstanceMethod(operandClass, "roundHalfToEven", Integer.TYPE);
        } else if (rounding instanceof Abs) {
            ga.loadLocal(valVar);
            ga.invokeInstanceMethod(operandClass, "abs");
        } else {
            throw new UnsupportedOperationException("Unknown rounding function");
        }
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(valVar);
        return NumericValue.class;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
