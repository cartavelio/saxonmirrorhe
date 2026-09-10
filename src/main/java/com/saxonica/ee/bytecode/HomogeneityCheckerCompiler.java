package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import com.saxonica.ee.bytecode.util.IteratorDetails;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.HomogeneityChecker;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.HomogeneityCheckerIterator;

public class HomogeneityCheckerCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {
        Expression base = ((HomogeneityChecker) expression).getBaseExpression();
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "HomogeneityCheckerCompiler-itr");
        ga.newInstance(HomogeneityCheckerIterator.class);
        ga.dup();
        compiler.compileToIterator(base);
        allocateStatic(compiler, base.getLocation());
        ga.invokeConstructor(HomogeneityCheckerIterator.class, SequenceIterator.class, Location.class);
        return new IteratorDetails(HomogeneityCheckerIterator.class, Item.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
