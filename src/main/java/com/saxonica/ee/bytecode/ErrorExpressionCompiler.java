package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.IteratorDetails;
import com.saxonica.ee.bytecode.util.OnEmpty;
import net.sf.saxon.expr.ErrorExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

/**
 * Generate bytecode for an error expression (an expression that throws a dynamic error if evaluated)
 */
public class ErrorExpressionCompiler extends ExpressionCompiler {

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
        ErrorExpression exp = (ErrorExpression) expression;
        compiler.generateDynamicError(exp.getMessage(), exp.getErrorCodeLocalPart(), exp.getLocation(), exp.isTypeError());
        return Item.class;
    }

    /**
     * Generate bytecode to evaluate the expression as a {@link net.sf.saxon.om.SequenceIterator}
     * Precondition: none.
     * Postcondition: at execution time, the stack contains a SequenceIterator representing the result of the expression
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileToItem(compiler, expression);
        return new IteratorDetails(SequenceIterator.class, Item.class);
    }

    /**
     * Generate bytecode to evaluate the expression as a boolean
     * Precondition: none.
     * Postcondition: at execution time, the stack contains an integer (0=false, 1=true) representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileToItem(compiler, expression);
    }

    /**
     * Generate bytecode to evaluate the expression in push mode
     * Precondition: none.
     * Postcondition: at execution time, the stack is unchanged, and the value of the expression has been
     * written to the current receiver
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileToItem(compiler, expression);
    }

    /**
     * Generate bytecode to evaluate the expression as a String or other CharSequence. This method
     * must be implemented for all expressions whose static type is xs:string or a type derived from xs:string
     * Precondition: none.
     * Postcondition: the stack contains a CharSequence value
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        compileToItem(compiler, expression);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited