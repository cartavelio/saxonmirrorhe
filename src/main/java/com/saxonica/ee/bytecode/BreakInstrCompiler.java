package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.instruct.BreakInstr;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.type.TypeHierarchy;

/**
 * Compiler for xsl:break within xsl:iterate (XSLT 3.0)
 */
public class BreakInstrCompiler extends ToPushCompiler {

    // This relies on the generated code for xsl:break being part of the same
    // compilation unit as xsl:iterate, which gives problems e.g. if there is a try/catch
    // intervening.  We detect this case and fall back to interpreted mode.

    @Override
    public void compileToPush(/*@NotNull*/ CompilerService compiler, Expression expression) throws CannotCompileException {
        BreakInstr instr = (BreakInstr) expression;
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        if (methodInfo.getXslIterateBreakLabelStack().isEmpty()) {
            throw new CannotCompileException("Nested xsl:break", true);
        }
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "BreakInstr-push");

        ga.goTo(methodInfo.getXslIterateBreakLabelStack().peek());

    }
}

// Copyright (c) 2011-2022 Saxonica Limited