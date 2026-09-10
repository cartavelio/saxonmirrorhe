package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.EmptyTextNodeRemover;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.om.SequenceIterator;


/**
 * Generate code for EmptyTextNodeRemover expression
 */
public class EmptyTextNodeRemoverCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the EmptyTextNodeRemover as an ItemMappingIterator
     * Precondition: none.
     * Postcondition: the stack contains an ItemMappingIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {

        Expression arg = ((EmptyTextNodeRemover) expression).getBaseExpression();
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "EmptyTextNodeRemoverCompiler-Itr");
        // return new ItemMappingIterator(getBaseExpression().iterate(context), this);
        ga.newInstance(ItemMappingIterator.class);
        ga.dup();
        compiler.compileToIterator(arg);
        allocateStatic(compiler, expression);
        ga.invokeConstructor(ItemMappingIterator.class, SequenceIterator.class, ItemMappingFunction.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
