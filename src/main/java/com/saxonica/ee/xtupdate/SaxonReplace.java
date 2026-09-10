package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.ReplaceNodeExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:replace is an XSLT version of the XQuery Update "replace node" expression.
 */

public class SaxonReplace extends ExtensionInstruction {

    private Expression select;

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
            if (f.equals("select")) {
                selectAtt = att.getValue();
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
                                                "saxon:replace",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
        SaxonUpdate.checkContainment(this);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression content = compileSequenceConstructor(exec, decl, false);
        return new ReplaceNodeExpression(select, content, Validation.PRESERVE, true);
    }

}

// Copyright (c) 2018-2022 Saxonica Limited


