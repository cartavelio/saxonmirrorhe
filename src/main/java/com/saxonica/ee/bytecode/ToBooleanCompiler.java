package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.BooleanValue;

/**
 * Abstract superclass for expression compilers that handle boolean expressions, that is,
 * expressions that return a boolean result. Compilers for such expressions must implement
 * the compileToBoolean method; other methods such as compileToItem and compileToIterator
 * invoke the compileToBoolean method.
 */
public abstract class ToBooleanCompiler extends ExpressionCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileToBoolean(compiler, expression);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
    }

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        compileToItem(compiler, expression);
        Generator ga = compiler.getCurrentGenerator();
        ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
    }

    @Override
    public void compileToPush(CompilerService compiler, final Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        compiler.generateGetOutputter();
        compileToItem(compiler, expression);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited



