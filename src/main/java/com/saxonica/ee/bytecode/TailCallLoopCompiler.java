package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.LabelInfo;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.TailCallLoop;

/**
 *
 */
public class TailCallLoopCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo tailCallLabel = methodInfo.newLabel("tailCallLabel");
        methodInfo.placeLabel(tailCallLabel);
        methodInfo.setTailCallLabel(tailCallLabel);
        compiler.compileToIterator(((TailCallLoop) expression).getBaseExpression());
    }

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo tailCallLabel = methodInfo.newLabel("tailCallLabel");
        methodInfo.placeLabel(tailCallLabel);
        methodInfo.setTailCallLabel(tailCallLabel);
        compiler.compileToPush(((TailCallLoop) expression).getBaseExpression());
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo tailCallLabel = methodInfo.newLabel("tailCallLabel");
        methodInfo.placeLabel(tailCallLabel);
        methodInfo.setTailCallLabel(tailCallLabel);
        compiler.compileToItem(((TailCallLoop) expression).getBaseExpression());
    }

    // TODO: we're not currently optimizing stack usage for non-recursive tail calls
}

// Copyright (c) 2011-2022 Saxonica Limited