package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NamespaceMap;

/**
 * Generate bytecode for a fixed element constructor (or XSLT literal result element)
 */

public class FixedElementCompiler extends ElementCreatorCompiler {

    /**
     * Callback from the superclass ElementCreatorCompiler to get the nameCode.
     * Leaves the name code for the element name on the stack
     * for the element name
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    protected void pushNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        allocateStatic(compiler, ((FixedElement) expression).getFixedElementName());
    }

    @Override
    protected void outputNamespaceNodes(CompilerService compiler, Expression expression) {

        NamespaceMap bindings = ((FixedElement) expression).getActiveNamespaces();
        Generator ga = compiler.getCurrentGenerator();

        if (bindings != null) {
            for (NamespaceBinding ns : bindings) {
                compiler.generateGetOutputter();
                ga.push(ns.getPrefix());
                ga.push(ns.getURI());
                ga.push(ReceiverOption.NONE);
                ga.invokeInstanceMethod(Outputter.class, "namespace", String.class, String.class, Integer.TYPE);
            }
        }
    }

    @Override
    public void identifyExpression(CompilerService compiler, Expression expression, String evalMode) {
        visitAnnotation(compiler, "FixedElement-" +
                ((FixedElement) expression).getFixedElementName().getDisplayName());

    }
}

// Copyright (c) 2011-2022 Saxonica Limited
