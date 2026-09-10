package com.saxonica.xsltextn.instruct;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;


/**
 * Handler for saxon:while elements in stylesheet. <br>
 * The saxon:while element has a mandatory attribute test, a boolean expression.
 * The content is output repeatedly so long as the test condition is true.
 */

public class While extends Instruction {

    private final Operand conditionOp;
    private final Operand actionOp;


    public While(Expression test, Expression action) {
        conditionOp = new Operand(this, test, OperandRole.INSPECT);
        actionOp = new Operand(this, action, OperandRole.FOCUS_CONTROLLED_ACTION);
    }


    public Expression getTest() {
        return conditionOp.getChildExpression();
    }

    public void setTest(Expression test) {
        conditionOp.setChildExpression(test);
    }

    public Expression getAction() {
        return actionOp.getChildExpression();
    }

    public void setAction(Expression action) {
        actionOp.setChildExpression(action);
    }

    @Override
    public Iterable<Operand> operands() {
        return operandList(conditionOp, actionOp);
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     *
     * @return the string "saxon:while"
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.SAXON_WHILE;
    }

    /**
     * Get the action expression (the content of the for-each)
     */

    public Expression getActionExpression() {
        return getAction();
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
        While exp = new While(getTest().copy(rebindings), getAction().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
    }

    /**
     * Get the item type of the items returned by evaluating this instruction
     *
     * @return the static item type of the instruction
     */

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return getAction().getItemType();
    }

    /**
     * Determine whether this instruction creates new nodes.
     * This implementation returns true if the "action" creates new nodes.
     * (Nodes created by the condition can't contribute to the result).
     */

    @Override
    public final boolean mayCreateNewNodes() {
        return !getAction().hasSpecialProperty(StaticProperty.NO_NODES_NEWLY_CREATED);
    }


    /*@Nullable*/
    @Override
    public TailCall processLeavingTail(Outputter output, XPathContext context) throws XPathException {
        while (getTest().effectiveBooleanValue(context)) {
            getAction().process(output, context);
        }
        return null;
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("while", this);
        getTest().export(out);
        getAction().export(out);
        out.endElement();
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
