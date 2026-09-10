package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.functions.Count;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for a call to the count() function
 */
public class CountCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        Expression arg = fnc.getArg(0);
        final Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "count");
        visitLineNumber(compiler, ga, expression);

        if (requiredClass != Integer.TYPE && requiredClass != Long.TYPE) {
            throw new IllegalArgumentException();
        }

        if (arg instanceof Literal || arg instanceof VariableReference) {
            compiler.compileToSequence(arg);
            ga.invokeStaticMethod(SequenceTool.class, "getLength", Sequence.class);

        } else {
            compiler.compileToIterator(arg);
            ga.invokeStaticMethod(Count.class, "count", SequenceIterator.class);
        }

        if (requiredClass == Long.TYPE) {
            ga.cast(Type.INT_TYPE, Type.LONG_TYPE);
        }
    }

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileItemFromInt(compiler, expression);
    }

    public static boolean maybeLastPositionFinder(Expression expression) {
        // Some common kinds of expression never return a LastPositionFinder. So don't bother generating the code that
        // tests for one.
        return !(
                expression instanceof AxisExpression ||
                        expression instanceof FilterExpression ||
                        expression instanceof SlashExpression ||
                        expression instanceof Atomizer ||
                        expression instanceof ItemChecker ||
                        expression instanceof CardinalityChecker
        );
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


