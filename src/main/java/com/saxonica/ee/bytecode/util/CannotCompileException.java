package com.saxonica.ee.bytecode.util;

import net.sf.saxon.expr.Expression;

/**
 * Exception indicating that constructs were encountered in the expression tree that
 * cannot be compiled to bytecode
 */
public class CannotCompileException extends Exception {

    private Expression uncompilableExpression;
    private String reason;
    private boolean recoverable;

    public CannotCompileException() {
        assert false;
    }

    public CannotCompileException(Expression expr) {
        assert false;
        this.uncompilableExpression = expr;
    }

    public CannotCompileException(String reason, boolean recoverable) {
        this.reason = reason;
        this.recoverable = recoverable;
        assert recoverable;
    }

    public CannotCompileException(Expression expr, String reason) {
        assert false;
        this.uncompilableExpression = expr;
        this.reason = reason;
    }

    @Override
    public String getMessage() {
        if (uncompilableExpression == null) {
            return (reason == null ? "" : reason);
        } else {
            return this.uncompilableExpression.getClass().toString()
                    + ": " + this.uncompilableExpression.toString()
                    + (reason == null ? "" : " (" + reason + ")");
        }
    }

    public Expression getExpression() {
        return uncompilableExpression;
    }

    public boolean isRecoverable() {
        return recoverable;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited

