package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.AscendingRangeIterator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.RangeExpression;
import net.sf.saxon.value.IntegerValue;

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
//        if (range.getStepExpression() != null) {
//            throw new CannotCompileException("'by' operator", true);
//        }
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "RangeExpressionCompiler-Itr");

        Expression start = range.getStartExpression();
        Expression step = range.getStepExpression();
        Expression end = range.getEndExpression();

        // Code commented out because it doesn't work, and because the spec could change anyway.

//        // We require the length of a sequence to be < max int (32 bits), but the values in the
//        // sequence can be higher than this. However, if either end of the sequence is in the int
//        // range, then we know that all the members of the sequence are within the long range.
//
//        boolean allLong = true;
//        if (!is64bit(start.getIntegerBounds()) || !is64bit (step.getIntegerBounds()) || !is64bit(end.getIntegerBounds())) {
//            allLong = false;
//        }
//        if (!allLong) {
//            if (is32bit(start.getIntegerBounds()) || is32bit(end.getIntegerBounds())) {
//                allLong = true;
//            }
//        }
//
//        if (allLong) {
//
//            LabelInfo rangeReturn = methodInfo.newLabel("rangeReturn");
//            LabelInfo returnEmpty = methodInfo.newLabel("rangeReturnEmpty");
//            Stack<Class<?>> unwindStack = new Stack<>();
//            compiler.compileToPrimitive(start, Long.TYPE, new OnEmpty.UnwindAndJump(returnEmpty));
//            unwindStack.push(Long.TYPE);
//            compiler.compileToPrimitive(step, Long.TYPE, new OnEmpty.UnwindAndJump(unwindStack, returnEmpty));
//            unwindStack.push(Long.TYPE);
//            compiler.compileToPrimitive(end, Long.TYPE, new OnEmpty.UnwindAndJump(unwindStack, returnEmpty));
//            ga.invokeStaticMethod(AscendingRangeIterator.class, "makeLongRangeIterator", Long.TYPE, Long.TYPE, Long.TYPE);
//            ga.goTo(rangeReturn);
//            methodInfo.placeLabel(returnEmpty);
//            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
//            methodInfo.placeLabel(rangeReturn);
//
//        } else {
            // Have to cater for some of the integers being outside the range of a long, even though
            // the length of the sequence as a whole is limited to a 32-bit int

            compiler.compileToItem(start);
            ga.checkClass(IntegerValue.class);
            compiler.compileToItem(step);
            ga.checkClass(IntegerValue.class);
            compiler.compileToItem(end);
            ga.checkClass(IntegerValue.class);
            ga.invokeStaticMethod(AscendingRangeIterator.class, "makeRangeIterator",
                                  IntegerValue.class, IntegerValue.class, IntegerValue.class);
//        }


    }

    private static boolean is32bit(IntegerValue[] bounds) {
        return bounds != null && bounds[0].abs().compareTo(Integer.MAX_VALUE) <= 0 && bounds[1].abs().compareTo(Integer.MAX_VALUE) <= 0;
    }

    private static boolean is64bit(IntegerValue[] bounds) {
        return bounds != null && bounds[0].abs().compareTo(Long.MAX_VALUE) <= 0 && bounds[0].abs().compareTo(Long.MAX_VALUE) <= 0;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited

