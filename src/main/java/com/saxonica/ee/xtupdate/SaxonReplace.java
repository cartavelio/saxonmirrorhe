package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.ReplaceNodeExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:replace is an XSLT version of the XQuery Update "replace node" expression.
 */

public class SaxonReplace extends StyleElement {

    private Expression select;

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
     * @return true: yes, it may contain a template-body
     */

    public boolean mayContainSequenceConstructor() {
        return true;
    }

    public void prepareAttributes() throws XPathException {
        String selectAtt = null;
        String contentAtt = null;

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            if (f.equals("select")) {
                selectAtt = atts.getValue(a);
                select = makeExpression(selectAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }

        if (selectAtt == null) {
            select = new ContextItemExpression();
            select.setLocation(allocateLocation());
        }

    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT,
                                                "saxon:replace",
                                                getPackageData().getLocalLicenseId());
        select = typeCheck("select", select);
        SaxonUpdate.checkContainment(this);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression content = compileSequenceConstructor(exec, decl, false);
        return new ReplaceNodeExpression(select, content, Validation.PRESERVE);
    }

}

// Copyright (c) 2018 Saxonica Ltd


