package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.RangeExpression;
import net.sf.saxon.expr.RangeIterator;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.IntegerValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for an integer range expression (X to Y)
 */
public class RangeExpressionCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the expression as a {@link net.sf.saxon.om.SequenceIterator}
     * Precondition: none.
     * Postcondition: at execution time, the stack contains a SequenceIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        RangeExpression range = (RangeExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "RangeExpressionCompiler-Itr");

        Expression start = range.getLhsExpression();
        Expression end = range.getRhsExpression();

        // We require the length of a sequence to be < max int (32 bits), but the values in the
        // sequence can be higher than this. However, if either end of the sequence is in the int
        // range, then we know that all the members of the sequence are within the long range.

        boolean allLong = true;
        if (!is64bit(start.getIntegerBounds()) || !is64bit(end.getIntegerBounds())) {
            allLong = false;
        }
        if (!allLong) {
            if (is32bit(start.getIntegerBounds()) || is32bit(end.getIntegerBounds())) {
                allLong = true;
            }
        }

        if (allLong) {

            LabelInfo rangeReturn = methodInfo.newLabel("rangeReturn");
            int startVar = ga.newLocal(Type.LONG_TYPE);
            LabelInfo returnEmpty = methodInfo.newLabel("rangeReturnEmpty");
            compiler.compileToPrimitive(start, Long.TYPE, new OnEmpty.UnwindAndJump(returnEmpty));
            ga.storeLocal(startVar);

            int endVar = ga.newLocal(Type.LONG_TYPE);
            compiler.compileToPrimitive(end, Long.TYPE, new OnEmpty.UnwindAndJump(returnEmpty));
            ga.storeLocal(endVar);

            ga.loadLocal(startVar);
            ga.loadLocal(endVar);
            LabelInfo startBeforeEnd = methodInfo.newLabel("startBeforeEnd");
            ga.ifCmp(Type.LONG_TYPE, GeneratorAdapter.LE, startBeforeEnd.label());
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            ga.checkClass(SequenceIterator.class);
            ga.goTo(rangeReturn);

            methodInfo.placeLabel(returnEmpty);
            allocateStatic(compiler, EmptyIterator.getInstance());
            ga.goTo(rangeReturn);

            methodInfo.placeLabel(startBeforeEnd);
            ga.newInstance(Type.getType(RangeIterator.class));
            ga.dup();
            ga.showLongVariable(compiler, "RangeExpression.start", startVar);
            ga.showLongVariable(compiler, "RangeExpression.end", endVar);
            ga.loadLocal(startVar);
            ga.loadLocal(endVar);
            ga.invokeConstructor(RangeIterator.class, Long.TYPE, Long.TYPE);

            methodInfo.placeLabel(rangeReturn);

        } else {
            // Have to cater for some of the integers being outside the range of a long, even though
            // the length of the sequence as a whole is limited to a 32-bit int

            compiler.compileToItem(start);
            ga.checkClass(IntegerValue.class);
            compiler.compileToItem(end);
            ga.checkClass(IntegerValue.class);
            ga.invokeStaticMethod(RangeIterator.class, "makeRangeIterator", IntegerValue.class, IntegerValue.class);
        }


    }

    private static boolean is32bit(IntegerValue[] bounds) {
        return bounds != null && bounds[0].abs().compareTo(Integer.MAX_VALUE) <= 0 && bounds[1].abs().compareTo(Integer.MAX_VALUE) <= 0;
    }

    private static boolean is64bit(IntegerValue[] bounds) {
        return bounds != null && bounds[0].abs().compareTo(Long.MAX_VALUE) <= 0 && bounds[0].abs().compareTo(Long.MAX_VALUE) <= 0;
    }
}

// Copyright (c) 2011 Saxonica Limited.

