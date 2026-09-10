package com.saxonica.xsltextn.instruct;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;

/**
 * Compiled instruction deriving from saxon:do element in stylesheet.
 * <p>The instruction evaluates the action expression and discards the result.</p>
 */

public class DoInstr extends Instruction  {

    private final Operand actionOp;

    public DoInstr(Expression select) {
        actionOp = new Operand(this, select, OperandRole.NAVIGATE);
    }

    public Expression getActionExpr() {
        return actionOp.getChildExpression();
    }

    @Override
    public Iterable<Operand> operands() {
        return actionOp;
    }

    @Override
    public int getIntrinsicDependencies() {
        return StaticProperty.HAS_SIDE_EFFECTS;
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables that need to be re-bound
     */
    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        DoInstr a2 = new DoInstr(getActionExpr().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, a2);
        return a2;
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.SAXON_DO;
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("do", this);
        getActionExpr().export(out);
        out.endElement();
    }
}

// Copyright (c) 2018-2022 Saxonica Limited