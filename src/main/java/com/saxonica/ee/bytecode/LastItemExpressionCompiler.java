package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.LastItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.ReversibleIterator;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;


/**
 * Generate bytecode for a call to the LastItemExpression()
 */
public class LastItemExpressionCompiler extends ToItemCompiler {


    /**
     * Generate bytecode to evaluate the LastItemExpression as an item
     * Precondition: none.
     * Postcondition: the stack is empty
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return the class of item generated
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        Expression expr = ((LastItemExpression) expression).getBaseExpression();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        IteratorDetails argIter = compiler.compileToIterator(expr);
        visitLineNumber(compiler, ga, expression);
        int forwardsVar = methodInfo.allocateLocal(argIter.iteratorClass);
        ga.storeLocal(forwardsVar);
        LabelInfo notReversible = methodInfo.newLabel("notReversible");
        LabelInfo end = methodInfo.newLabel("endLast");
        ga.loadLocal(forwardsVar);
        ga.instanceOf(Type.getType(ReversibleIterator.class));
        ga.ifZCmp(GeneratorAdapter.EQ, notReversible.label());
        ga.loadLocal(forwardsVar);
        ga.checkClass(ReversibleIterator.class);
        ga.invokeInstanceMethod(ReversibleIterator.class, "getReverseIterator");
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.goTo(end);
        visitAnnotation(compiler, "LastItemExpression");
        methodInfo.placeLabel(notReversible);
        LabelInfo done = methodInfo.newLabel("doneLast");
        ga.pushNull();
        ga.loadLocal(forwardsVar);
        ga.invokeInstanceMethod(argIter.iteratorClass, "next");
        ga.dup();

        LabelInfo loop = methodInfo.placeNewLabel("loopLast");
        ga.ifNull(done.label());
        ga.swap();
        ga.pop();
        ga.loadLocal(forwardsVar);
        ga.invokeInstanceMethod(argIter.iteratorClass, "next");
        ga.dup();
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(forwardsVar);
        return argIter.itemClass;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited