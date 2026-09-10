package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

/**
 * Bytecode generator for instructions that calls back to the interpreter. This generates code which simply invokes the
 * interpreter for this instruction, including interpreting its subexpressions and patterns
 */
public class InterpretedExpressionCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        allocateStatic(compiler, expression);
        compiler.generateGetContext();
        compiler.getCurrentGenerator().invokeInstanceMethod(Expression.class, "evaluateItem", XPathContext.class);
        return Item.class;
    }

    /**
     * Generate bytecode to evaluate the expression as an Iterator
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an iterator (the result of the expression)
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        allocateStatic(compiler, expression);
        compiler.generateGetContext();
        compiler.getCurrentGenerator().invokeInstanceMethod(Expression.class, "iterate", XPathContext.class);
        return new IteratorDetails(SequenceIterator.class, Item.class);
    }

    /**
     * Generate bytecode to evaluate the expression
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an iterator (the result of the expression)
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        allocateStatic(compiler, expression);
        ga.loadArgOrLocal(methodInfo.getOutputterPosition());
        compiler.generateGetContext();
        ga.invokeInstanceMethod(Expression.class, "process", Outputter.class, XPathContext.class);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited


