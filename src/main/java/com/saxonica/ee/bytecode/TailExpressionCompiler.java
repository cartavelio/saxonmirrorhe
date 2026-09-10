package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.TailExpression;
import net.sf.saxon.expr.TailIterator;
import net.sf.saxon.om.SequenceIterator;

/**
 * Generate bytecode for evaluation of a TailExpression
 */
public class TailExpressionCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler, final Expression expression) throws CannotCompileException {
        TailExpression tail = (TailExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        compiler.compileToIterator(tail.getBaseExpression());
        visitLineNumber(compiler, ga, expression);
        ga.push(tail.getStart());
        ga.invokeStaticMethod(TailIterator.class, "make", SequenceIterator.class, Integer.TYPE);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


