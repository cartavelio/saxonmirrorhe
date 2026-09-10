package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;

/**
 * Generate bytecode for a call to a FunctionLiteral
 */
public class FunctionLiteralCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        allocateStatic(compiler, ((FunctionLiteral) expression).getGroundedValue());
        return Function.class;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
