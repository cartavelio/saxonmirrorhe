package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.sort.ConditionalSorter;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.om.SequenceIterator;

import java.util.Comparator;


/**
 * Generate bytecode for evaluation of a ConditionalSorterCompiler
 */
public class ConditionalSorterCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        ConditionalSorter sortExpr = (ConditionalSorter) expression;
        DocumentSorter docSortExpr = sortExpr.getDocumentSorter();
        Expression condition = sortExpr.getCondition();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ConditionalSorterCompiler");
        visitLineNumber(compiler, ga, expression);
        LabelInfo end = methodInfo.newLabel("endCondSort");

        compiler.compileToIterator(docSortExpr.getBaseExpression());
        compiler.compileToBoolean(condition);
        ga.ifFalse(end);  // no sorting needed

        // stack: baseIterator

        ga.newInstance(DocumentOrderIterator.class);
        ga.dup();

        // stack: baseIterator sortIterator sortIterator
        ga.dupX2();
        // stack: sortIterator baseIterator sortIterator sortIterator
        ga.pop();
        // stack: sortIterator baseIterator sortIterator
        ga.swap();
        // stack: sortIterator sortIterator baseIterator

        ExpressionCompiler.allocateStatic(compiler, docSortExpr.getComparer());
        ga.invokeConstructor(DocumentOrderIterator.class, SequenceIterator.class, Comparator.class);

        methodInfo.placeLabel(end);
        ga.checkClass(SequenceIterator.class);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited