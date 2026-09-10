package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.expr.Expression;

/**
 * Generate bytecode for a call to a FunctionLiteral
 */
public class FunctionLiteralCompiler extends ToItemCompiler {

    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        allocateStatic(compiler, ((FunctionLiteral) expression).getGroundedValue());
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
