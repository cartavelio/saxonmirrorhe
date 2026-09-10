package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NamespaceBindingSet;

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
        allocateStatic(compiler, ((FixedElement) expression).getElementName());
    }

    protected void outputNamespaceNodes(CompilerService compiler, Expression expression) {

        NamespaceBinding[] bindings = ((FixedElement) expression).getActiveNamespaces();
        Generator ga = compiler.getCurrentGenerator();

        if (bindings != null) {
            for (NamespaceBinding binding : bindings) {
                //out.namespace(bindings[i], 0);
                compiler.generateGetReceiver();
                allocateStatic(compiler, binding);
                ga.push(0);
                ga.invokeInstanceMethod(Receiver.class, "namespace", NamespaceBindingSet.class, Integer.TYPE);
            }
        }
    }

    @Override
    public void identifyExpression(CompilerService compiler, Expression expression, String evalMode) {
        visitAnnotation(compiler, "FixedElement-" +
                ((FixedElement) expression).getElementName().getDisplayName());

    }
}

// Copyright (c) 2011 Saxonica Limited.
