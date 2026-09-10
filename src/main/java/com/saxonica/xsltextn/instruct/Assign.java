package com.saxonica.xsltextn.instruct;

import net.sf.saxon.Controller;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;

/**
 * Compiled instruction deriving from saxon:assign element in stylesheet.
 * <p>The saxon:assign element has mandatory attribute name and optional attribute expr.
 * It also allows xsl:extension-element-prefixes etc.</p>
 */

public class Assign extends Instruction implements BindingReference {

    private final Operand selectOp;
    private GlobalVariable binding;    // link to the variable declaration

    public Assign(Expression select) {
        selectOp = new Operand(this, select, OperandRole.NAVIGATE);
    }

    @Override
    public void setStaticType(SequenceType type, GroundedValue constantValue, int properties) {
    }

    public void setSelectExpr(Expression select) {
        selectOp.setChildExpression(select);
    }

    public Expression getSelectExpr() {
        return selectOp.getChildExpression();
    }

    @Override
    public Iterable<Operand> operands() {
        return selectOp;
    }

    @Override
    public void fixup(Binding binding) {
        this.binding = (GlobalVariable) binding;
    }

    public Expression typeCheck(ExpressionVisitor visitor,
                                ContextItemStaticInfo contextInfo) throws XPathException {
        typeCheckChildren(visitor, contextInfo);
        TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
        Supplier<RoleDiagnostic> role =
                () -> new RoleDiagnostic(RoleDiagnostic.VARIABLE, binding.getObjectName().getDisplayName(), 0);
        selectOp.setChildExpression(
                tc.staticTypeCheck(getSelectExpr(), binding.getRequiredType(), role, visitor));
        return this;
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
        Assign a2 = new Assign(getSelectExpr().copy(rebindings));
        ExpressionTool.copyLocationInfo(this, a2);
        a2.binding = binding;
        return a2;
    }

    /**
     * Get the name of this instruction for diagnostic and tracing purposes
     */

    @Override
    public int getInstructionNameCode() {
        return StandardNames.SAXON_ASSIGN;
    }


    /*@Nullable*/
    @Override
    public TailCall processLeavingTail(Outputter output, XPathContext context) throws XPathException {
        if (binding == null) {
            throw new IllegalStateException("saxon:assign binding has not been fixed up");
        }
        GroundedValue value = SequenceTool.toGroundedValue(getSelectExpr().iterate(context));
        final Controller controller = context.getController();
        assert controller != null;
        controller.getBindery(getPackageData()).setGlobalVariable(binding, value);
        return null;
    }

    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        if ("JS".equals(((ExpressionPresenter.ExportOptions) out.getOptions()).target)) {
            throw new XPathException("Export failure: SaxonJS does not implement saxon:assign");
        }
        out.startElement("assign", this);
        out.emitAttribute("name", binding.getVariableQName());
        getSelectExpr().export(out);
        out.endElement();
    }
}

// Copyright (c) 2011-2022 Saxonica Limited