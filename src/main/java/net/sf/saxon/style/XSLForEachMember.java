package net.sf.saxon.style;

import com.saxonica.expr.ForMemberExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.BindingReference;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;


/**
 * Handler for xsl:for-each-member elements in stylesheet (4.0 extension).
 * The xsl:for-each-member element has mandatory attributes select and bind-to.
 * The select expression must return an array. For each member of this array
 * it binds the variable named in the bind-to attribute, and executes the body
 * of the instruction with this additional variable binding.
 */

public class XSLForEachMember extends StyleElement implements ScopedBindingElement {

    private Expression select;
    private StructuredQName bindTo;
    private SourceBinding sourceBinding;
    private ForMemberExpression compiledExpression;

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
     * @return true: yes, it may contain a template-body
     */

    @Override
    public boolean mayContainSequenceConstructor() {
        return true;
    }

    @Override
    public void prepareAttributes() {
        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            String value = att.getValue();
            if (f.equals("select")) {
                select = makeExpression(value, att);
            } else if (f.equals("bind-to")) {
                bindTo = makeQName(value, null, "bind-to");
            } else {
                checkUnknownAttribute(attName);
            }
        }

        if (select == null) {
            reportAbsence("select");
            select = Literal.makeEmptySequence();
        }
        if (bindTo == null) {
            reportAbsence("bind-to");
            bindTo = new StructuredQName("saxon", NamespaceConstant.SAXON, "error-variable");
        }

        sourceBinding = new SourceBinding(this);
        sourceBinding.setVariableQName(bindTo);
        sourceBinding.setDeclaredType(SequenceType.ANY_SEQUENCE);
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:for-each-member",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression action = compileSequenceConstructor(exec, decl, true);
        if (action == null) {
            action = Literal.makeEmptySequence();
        }
        ForMemberExpression exp = new ForMemberExpression();
        exp.setSequence(select);
        exp.setVariableQName(bindTo);
        exp.setAction(action);
        compiledExpression = exp;
        for (BindingReference ref : sourceBinding.getReferences()) {
            ((LocalVariableReference)ref).setBinding(compiledExpression);
        }
        return exp;
    }

    /**
     * Ask whether this particular element implicitly binds a given variable (used for xsl:accumulator-rule)
     *
     * @param name
     */
    @Override
    protected SourceBinding hasImplicitBinding(StructuredQName name) {
        if (name.equals(bindTo)) {
            return sourceBinding;
        } else {
            return null;
        }
    }

    /**
     * Allocate slots to any variables used within the pattern
     *
     * @param slotManager the slot manager representing the stack frame for local variables
     * @param nextFree    the next slot that is free to be allocated
     * @return the next slot that is free to be allocated
     */

    @Override
    public int allocateSlots(SlotManager slotManager, int nextFree) {
        int slot = nextFree++;
        compiledExpression.setSlotNumber(slot);
        slotManager.allocateSlotNumber(bindTo);
        for (BindingReference ref : sourceBinding.getReferences()) {
            ref.fixup(compiledExpression);
            ((LocalVariableReference)ref).setSlotNumber(slot);
        }
        return nextFree;
    }
}

// Copyright (c) 2018-2020 Saxonica Limited
