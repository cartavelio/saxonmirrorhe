package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.ReplaceValueExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:change is an XSLT version of the XQuery Update "replace value of node" expression.
 */

public class SaxonChange extends ExtensionInstruction {

    private Expression select;
    private Expression newValue;

    /**
     * Determine whether this type of element is allowed to contain a template-body
     *
     * @return false: no, it may not contain a template-body
     */

    @Override
    protected boolean mayContainSequenceConstructor() {
        return false;
    }

    @Override
    protected void prepareAttributes() {
        String selectAtt = null;
        String toAtt = null;

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            String f = attName.getDisplayName();
            if (f.equals("select")) {
                selectAtt = value;
                select = makeExpression(selectAtt, att);
            } else if (f.equals("to")) {
                toAtt = value;
                newValue = makeExpression(toAtt, att);
            } else {
                checkUnknownAttribute(attName);
            }
        }

        if (selectAtt == null) {
            select = new ContextItemExpression();
            select.setLocation(allocateLocation());
        }

        if (toAtt == null) {
            reportAbsence("to");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT,
                                                "saxon:change",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
        newValue = typeCheck("to", newValue);
        SaxonUpdate.checkContainment(this);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new ReplaceValueExpression(select, newValue);
    }

}

// Copyright (c) 2018-2023 Saxonica Limited


