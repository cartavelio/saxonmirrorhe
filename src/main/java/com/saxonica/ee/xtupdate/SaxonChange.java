package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.ReplaceValueExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:change is an XSLT version of the XQuery Update "replace value of node" expression.
 */

public class SaxonChange extends StyleElement {

    private Expression select;
    private Expression newValue;

    /**
     * Determine whether this node is an instruction.
     *
     * @return true - it is an instruction
     */

    public boolean isInstruction() {
        return true;
    }

    /**
     * Determine whether this type of element is allowed to contain a template-body
     *
     * @return false: no, it may not contain a template-body
     */

    public boolean mayContainSequenceConstructor() {
        return false;
    }

    public void prepareAttributes() throws XPathException {
        String selectAtt = null;
        String toAtt = null;

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            if (f.equals("select")) {
                selectAtt = atts.getValue(a);
                select = makeExpression(selectAtt, a);
            } else if (f.equals("to")) {
                toAtt = atts.getValue(a);
                newValue = makeExpression(toAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
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

    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT,
                                                "saxon:change",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
        newValue = typeCheck("to", newValue);
        SaxonUpdate.checkContainment(this);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new ReplaceValueExpression(select, newValue);
    }

}

// Copyright (c) 2018 Saxonica Ltd


