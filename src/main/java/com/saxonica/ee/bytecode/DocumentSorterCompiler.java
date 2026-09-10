package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import org.objectweb.asm.Type;

import java.util.Comparator;

/**
 * Generate bytecode for a call to the DocumentSorter
 */
public class DocumentSorterCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "DocumentSorterCompiler");
        DocumentSorter docSorterExpr = ((DocumentSorter) expression);
        Comparator<? super NodeInfo> comparer = docSorterExpr.getComparer();
        Expression operand = docSorterExpr.getBaseExpression();
        ga.newInstance(Type.getType(DocumentOrderIterator.class));
        ga.dup();
        compiler.compileToIterator(operand);
        ExpressionCompiler.allocateStatic(compiler, comparer);
        ga.invokeConstructor(DocumentOrderIterator.class, SequenceIterator.class, Comparator.class);
    }
}

// Copyright (c) 2011-2020 Saxonica Limited
