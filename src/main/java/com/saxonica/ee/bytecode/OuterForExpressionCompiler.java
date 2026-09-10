package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.map.ForSequenceMappingAction;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.flwor.OuterForExpression;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.tree.iter.LookaheadIteratorImpl;
import net.sf.saxon.value.EmptySequence;


/**
 * Generate bytecode for a call to the OuterForExpression()
 */
public class OuterForExpressionCompiler extends ToIteratorCompiler {


    /**
     * Generate bytecode to evaluate the OuterForExpression in push mode
     * Precondition: none.
     * Postcondition: the stack is empty
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */

    @Override
    public void compileToPush(CompilerService compiler, final Expression expression) throws CannotCompileException {
        Expression sequence = ((OuterForExpression) expression).getSequence();
        Expression action = ((OuterForExpression) expression).getAction();
        int slotNum = ((OuterForExpression) expression).getLocalSlotNumber();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "OuterForExpression-push");
        int positionVar = methodInfo.allocateLocal(Integer.TYPE);
        ga.push(1);
        ga.storeLocal(positionVar);

        LabelInfo noNextItem = methodInfo.newLabel("noNextItem");
        LabelInfo end = methodInfo.newLabel("end_OuterFor");
        LabelInfo itemNull = methodInfo.newLabel("itemNull_OuterFor");

        compiler.compileToIterator(sequence);
        ga.invokeStaticMethod(LookaheadIteratorImpl.class, "makeLookaheadIterator", SequenceIterator.class);
        int iterVar = methodInfo.allocateLocal(LookaheadIterator.class);
        ga.storeLocal(iterVar);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(LookaheadIterator.class, "hasNext");

        ga.ifFalse(noNextItem);
        LabelInfo loop = methodInfo.placeNewLabel("loopFor");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(LookaheadIterator.class, "next");
        ga.dup();
        ga.ifNull(itemNull.label());
        compiler.generateGetContext();
        ga.swap();
        ga.push(slotNum);
        ga.swap();
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        compiler.compileToPush(action);
        ga.goTo(loop);

        methodInfo.placeLabel(itemNull);
        ga.pop();
        ga.goTo(end);

        methodInfo.placeLabel(noNextItem);
        compiler.generateGetContext();
        ga.push(slotNum);
        ga.invokeStaticMethod(EmptySequence.class, "getInstance");
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);

        compiler.compileToPush(action);

        methodInfo.placeLabel(end);
        methodInfo.releaseLocal(iterVar);
        methodInfo.releaseLocal(positionVar);
    }


    /**
     * Generate bytecode to evaluate the OuterForExpression as an iterator
     * Precondition: none.
     * Postcondition: the stack contains an iterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {

        Expression sequence = ((OuterForExpression) expression).getSequence();
        Expression action = ((OuterForExpression) expression).getAction();
        int slotNum = ((OuterForExpression) expression).getLocalSlotNumber();
        visitAnnotation(compiler, "OuterForexpression-itr");
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo noNextItem = methodInfo.newLabel("noNextItem");
        LabelInfo end = methodInfo.newLabel("end_OuterFor");

        compiler.compileToIterator(sequence);
        ga.invokeStaticMethod(LookaheadIteratorImpl.class, "makeLookaheadIterator", SequenceIterator.class);
        int iterVar = methodInfo.allocateLocal(LookaheadIterator.class);
        ga.storeLocal(iterVar);
        ga.loadLocal(iterVar);

        ga.invokeInstanceMethod(LookaheadIterator.class, "hasNext");

        ga.ifFalse(noNextItem);
        ga.newInstance(MappingIterator.class);
        ga.dup();
        ga.loadLocal(iterVar);
        Class mappingActionClass = compiler.getCompiledClass(expression);
        if (mappingActionClass == null) {
            mappingActionClass = ForExpressionCompiler.generateSequenceMappingAction(compiler, (ForExpression) expression, action, slotNum);
            compiler.setCompiledClass(expression, mappingActionClass);
        }
        allocateStatic(compiler, mappingActionClass);
        ga.invokeInstanceMethod(Class.class, "newInstance");
        ga.checkClass(ForSequenceMappingAction.class);

        // assert stack : MappingIterator MappingIterator BaseIterator MappingFunction

        ga.dup();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(ForSequenceMappingAction.class, "setContext", XPathContext.class);
        ga.invokeConstructor(MappingIterator.class, SequenceIterator.class, MappingFunction.class);
        ga.goTo(end);

        methodInfo.placeLabel(noNextItem);
        compiler.generateGetContext();
        ga.push(slotNum);
        ga.invokeStaticMethod(EmptySequence.class, "getInstance");
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        compiler.compileToIterator(action);
        ga.checkClass(SequenceIterator.class);
        methodInfo.placeLabel(end);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
