package com.saxonica.xsltextn.style;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * A saxon:entity-ref element in the stylesheet. This causes an entity reference
 * to be output to the XML or HTML output stream. <br>
 */

public class SaxonEntityRef extends ExtensionInstruction {

    String nameAttribute;

    @Override
    protected void prepareAttributes() {

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            String f = attName.getDisplayName();
            if (f.equals("name")) {
                nameAttribute = Whitespace.trim(value);
            } else {
                checkUnknownAttribute(attName);
            }
        }

        if (nameAttribute == null) {
            reportAbsence("name");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        checkEmpty();
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:entity-ref",
                                                getPackageData().getLocalLicenseId());
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new ValueOf(new StringLiteral('&' + nameAttribute + ';'), true, false);
    }

}

// Copyright (c) 2009-2022 Saxonica Limited