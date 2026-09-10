package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;

/**
 * Compiler for a CompiledExpression. This arises when we need to compile an expression such as
 * F[E], and E has already been compiled. We generate code to invoke the already-existing bytecode
 * for E.
 */

public class CompiledExpressionCompiler extends ExpressionCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  /*@NotNull*/ Expression expression) throws CannotCompileException {
        CompiledExpression ce = (CompiledExpression)expression;
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, ce.getGeneratedCode());
        compiler.generateGetContext();
        ga.invokeInstanceMethod(GeneratedCode.class, "iterate", XPathContext.class);
    }

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        CompiledExpression ce = (CompiledExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, ce.getGeneratedCode());
        compiler.generateGetContext();
        ga.invokeInstanceMethod(GeneratedCode.class, "evaluateItem", XPathContext.class);
        return Item.class;
    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        CompiledExpression ce = (CompiledExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, ce.getGeneratedCode());
        compiler.generateGetContext();
        ga.invokeInstanceMethod(GeneratedCode.class, "effectiveBooleanValue", XPathContext.class);
    }

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        CompiledExpression ce = (CompiledExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, ce.getGeneratedCode());
        compiler.generateGetOutputter();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(GeneratedCode.class, "process", Outputter.class, XPathContext.class);
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
