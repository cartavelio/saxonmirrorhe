package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.TailCallLoop;
import net.sf.saxon.om.Item;

/**
 *
 */
public class TailCallLoopCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo tailCallLabel = methodInfo.newLabel("tailCallLabel");
        methodInfo.placeLabel(tailCallLabel);
        methodInfo.setTailCallLabel(tailCallLabel);
        return compiler.compileToIterator(((TailCallLoop) expression).getBaseExpression());
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
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo tailCallLabel = methodInfo.newLabel("tailCallLabel");
        methodInfo.placeLabel(tailCallLabel);
        methodInfo.setTailCallLabel(tailCallLabel);
        return compiler.compileToItem(((TailCallLoop) expression).getBaseExpression());
    }

    // TODO: we're not currently optimizing stack usage for non-recursive tail calls
}

// Copyright (c) 2011-2022 Saxonica Limited