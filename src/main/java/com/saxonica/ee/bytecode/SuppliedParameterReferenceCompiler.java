package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SuppliedParameterReference;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;

/**
 * Compiler for LocalParam instructions in XSLT (representing an xsl:param in a template)
 */
public class SuppliedParameterReferenceCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        SuppliedParameterReference param = (SuppliedParameterReference) expression;

        if (param.getSlotNumber() == -1) {
            // used for dynamic streaming parameters
            throw new CannotCompileException(param);
        }
        final Generator ga = compiler.getCurrentGenerator();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "SuppliedParameterReference-Itr");

        // int wasSupplied = context.useLocalParameter(parameterId, slotNumber, isTunnelParam());

        compiler.generateGetContext();
        ga.push(param.getSlotNumber());
        ga.invokeInstanceMethod(XPathContext.class, "evaluateLocalVariable", Integer.TYPE);
        ga.invokeInstanceMethod(Sequence.class, "iterate");

    }

}

// Copyright (c) 2011-2022 Saxonica Limited