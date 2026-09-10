package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.AdjacentTextNodeMerger;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.AdjacentTextNodeMergingIterator;
import org.objectweb.asm.Type;

/**
 * Generate byte code for an adjacent text node merger expression
 */
public class AdjacentTextNodeMergerCompiler extends ToIteratorCompiler {

    /**
     * Generate bytecode to evaluate the expression as a {@link net.sf.saxon.om.SequenceIterator}
     * Precondition: none.
     * Postcondition: at execution time, the stack contains a SequenceIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler, /*@NotNull*/ Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        Expression base = ((AdjacentTextNodeMerger) expression).getBaseExpression();
        ga.newInstance(Type.getType(AdjacentTextNodeMergingIterator.class));
        ga.dup();
        compiler.compileToIterator(base);
        ga.invokeConstructor(AdjacentTextNodeMergingIterator.class, SequenceIterator.class);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited