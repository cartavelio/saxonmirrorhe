package com.saxonica.ee.extfn.js;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.*;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for ixsl:promise elements in stylesheet.
 * The ixsl:promise element has one mandatory attribute select (an expression which returns a promise),
 * and may also have the optional attributes on-completion and on-failure (expressions which return
 * single-argument XDM functions).
 *
 */

public class IXSLPromise extends StyleElement {

    private Expression select;
    private Expression onCompletion;
    private Expression onFailure;

    /**
     * Determine whether this node is an instruction.
     *
     * @return true - it is an instruction
     */

    @Override
    public boolean isInstruction() {
        return true;
    }

    /**
     * Determine whether this type of element is allowed to contain a template-body
     *
     * @return false: no, the element must be empty
     */

    @Override
    protected boolean mayContainSequenceConstructor() {
        return false;
    }

    @Override
    protected void prepareAttributes() {

        AttributeMap atts = attributes();
        for (AttributeInfo att : atts) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            if (f.equals("select")) {
                String selectAtt = att.getValue();
                select = makeExpression(selectAtt, att);
            } else if (f.equals("on-completion")) {
                String completionAtt = att.getValue();
                onCompletion = makeExpression(completionAtt, att);
            } else if (f.equals("on-failure")) {
                String failureAtt = att.getValue();
                onFailure = makeExpression(failureAtt, att);
            } else {
                checkUnknownAttribute(attName);
            }
        }
        if (select == null) {
            reportAbsence("select");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        select = typeCheck("select", select);
        onCompletion = typeCheck("on-completion", onCompletion);
        onFailure = typeCheck("on-failure", onFailure);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        if (!"JS3".equals(exec.getCompilerInfo().getTargetEdition())) {
            compileError("ixsl:promise is available only if the target edition is JS3");
        }
        return new PromiseInstr(select, onCompletion, onFailure);
    }

    private static class PromiseInstr extends Instruction {

        private final Operand selectOp;
        private Operand onCompletionOp;
        private Operand onFailureOp;

        public PromiseInstr(Expression select, Expression onCompletion, Expression onFailure) {
            selectOp = new Operand(this, select, OperandRole.SINGLE_ATOMIC);
            if (onCompletion != null) {
                onCompletionOp = new Operand(this, onCompletion, OperandRole.SINGLE_ATOMIC);
            }
            if (onFailure != null) {
                onFailureOp = new Operand(this, onFailure, OperandRole.SINGLE_ATOMIC);
            }
        }

        public Expression getSelect() {
            return selectOp.getChildExpression();
        }

        public Expression getOnCompletion() {
            return onCompletionOp == null ? null : onCompletionOp.getChildExpression();
        }

        public Expression getOnFailure() {
            return onFailureOp == null ? null : onFailureOp.getChildExpression();
        }

        @Override
        public int getImplementationMethod() {
            return Expression.PROCESS_METHOD;
        }

        @Override
        public Iterable<Operand> operands() {
            return operandSparseList(selectOp, onCompletionOp, onFailureOp);
        }

        @Override
        public String getExpressionName() {
            return "schedule";
        }

        public String toString() {
            return "promise";
        }

        @Override
        public void export(ExpressionPresenter out) throws XPathException {
            out.startElement("schedule", this);
            out.setChildRole("select");
            selectOp.getChildExpression().export(out);
            if (onCompletionOp != null) {
                out.setChildRole("on-completion");
                onCompletionOp.getChildExpression().export(out);
            }
            if (onFailureOp != null) {
                out.setChildRole("on-failure");
                onFailureOp.getChildExpression().export(out);
            }
            out.endElement();
        }

        @Override
        public Expression copy(RebindingMap rm) {
            PromiseInstr p2 = new PromiseInstr(copy(getSelect(), rm), copy(getOnCompletion(), rm), copy(getOnFailure(), rm));
            ExpressionTool.copyLocationInfo(this, p2);
            return p2;
        }

        private Expression copy(Expression exp, RebindingMap rebindings) {
            return exp == null ? null : exp.copy(rebindings);
        }
    }

}

// Copyright (c) 2009-2023 Saxonica Limited
