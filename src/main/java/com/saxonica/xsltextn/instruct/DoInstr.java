package com.saxonica.xsltextn.instruct;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;

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


    @Override
    public Elaborator getElaborator() {
        return new DoInstructionElaborator();
    }

    public static class DoInstructionElaborator extends PushElaborator {

        @Override
        public PushEvaluator elaborateForPush() {
            DoInstr expr = (DoInstr) getExpression();
            SequenceEvaluator puller = expr.getActionExpr().makeElaborator().eagerly();
            return (output, context) -> {
                puller.evaluate(context);
                return null;
                // discard result
            };
        }

        //@SuppressWarnings("Convert2MethodRef")
        @Override
        public PullEvaluator elaborateForPull() {
            DoInstr expr = (DoInstr) getExpression();
            SequenceEvaluator puller = expr.getActionExpr().makeElaborator().eagerly();
            // discard result
            return context -> {
                puller.evaluate(context);
                return EmptyIterator.getInstance();
            };
        }

        //@SuppressWarnings("Convert2MethodRef")
        @Override
        public ItemEvaluator elaborateForItem() {
            DoInstr expr = (DoInstr) getExpression();
            SequenceEvaluator puller = expr.getActionExpr().makeElaborator().eagerly();
            // discard result
            return context -> {
                puller.evaluate(context);
                return null;
            };
        }
    }
}


// Copyright (c) 2018-2023 Saxonica Limited