package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.FirstItemExpression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;


/**
 * Generate code for the FirstItemExpression
 */
public class FirstItemExpressionCompiler extends ToItemCompiler {


    /**
     * Generate bytecode to evaluate the expression as a Item
     * Precondition: none.
     * Postcondition: the stack contains a Item representing the result of the expression
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "FirstItemExpression");
        Expression expr = ((FirstItemExpression) expression).getBaseExpression();
        compiler.compileToIterator(expr);
        visitLineNumber(compiler, ga, expression);
        ga.dup();
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.swap();
        ga.invokeInstanceMethod(SequenceIterator.class, "close");
        return Item.class;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
