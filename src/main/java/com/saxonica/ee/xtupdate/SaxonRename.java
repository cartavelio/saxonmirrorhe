package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.RenameExpression;
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
 * saxon:rename is an XSLT version of the XQuery Update rename expression.
 */

public class SaxonRename extends ExtensionInstruction {

    private Expression select;
    private Expression newName;

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
            String f = attName.getDisplayName();
            String value = att.getValue();
            if (f.equals("select")) {
                selectAtt = value;
                select = makeExpression(selectAtt, att);
            } else if (f.equals("to")) {
                toAtt = value;
                newName = makeExpression(toAtt, att);
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
                                                "saxon:rename",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
        newName = typeCheck("to", newName);
        SaxonUpdate.checkContainment(this);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new RenameExpression(select, newName,
                                    getAllNamespaces(),
                                    getDefaultXPathNamespace(),
                                    true);
    }

}

// Copyright (c) 2018-2023 Saxonica Limited


