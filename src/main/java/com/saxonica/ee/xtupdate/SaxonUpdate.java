package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.CopyModifyExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.instruct.CopyOf;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:update is an XSLT version of the XQuery Update copy-modify expression. Unlike the XQuery version,
 * it does not bind any variables; instead, the contained updating instructions operate on the context
 * item established by the select expression.
 */

public class SaxonUpdate extends StyleElement {

    private Expression select;

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
    protected boolean mayContainSequenceConstructor() {
        return true;
    }

    @Override
    protected void prepareAttributes() {
        String selectAtt = null;

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            String value = att.getValue();
            if (f.equals("select")) {
                selectAtt = value;
                select = makeExpression(selectAtt, att);
            } else {
                checkUnknownAttribute(attName);
            }
        }

        if (selectAtt == null) {
            select = new ContextItemExpression();
            select.setLocation(allocateLocation());
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT,
                                                "saxon:modify",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression action = compileSequenceConstructor(exec, decl, true);
        if (action == null) {
            action = Literal.makeEmptySequence();
        }
        CopyModifyExpression exp = new CopyModifyExpression(action, new ContextItemExpression(), Validation.STRIP);
        CopyOf copy = new CopyOf(select, true, Validation.STRIP, null, false);
        copy.setCopyForUpdate(true);
        ForEach forEach = new ForEach(copy, exp);
        return forEach;
    }

    public static void checkContainment(StyleElement child) throws XPathException {
        NodeInfo s = child;
        while (true) {
            s = s.getParent();
            if (s instanceof SaxonUpdate) {
                return;
            }
            if (!(s instanceof XSLIf || s instanceof XSLChoose || s instanceof XSLWhen
                          || s instanceof XSLOtherwise || s instanceof XSLForEach)) {
                child.compileError("The " + child.getDisplayName() +
                                           " instruction must be contained within saxon:update; only xsl:if, xsl:choose, and xsl:for-each may intervene");
                return;
            }
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited


