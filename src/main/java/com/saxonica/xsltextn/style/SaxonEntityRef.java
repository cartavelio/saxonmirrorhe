package com.saxonica.xsltextn.style;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * A saxon:entity-ref element in the stylesheet. This causes an entity reference
 * to be output to the XML or HTML output stream. <br>
 */

public class SaxonEntityRef extends StyleElement {

    String nameAttribute;

    /**
     * Determine whether this node is an instruction.
     *
     * @return true - it is an instruction
     */

    public boolean isInstruction() {
        return true;
    }


    public void prepareAttributes() throws XPathException {

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            if (f.equals("name")) {
                nameAttribute = Whitespace.trim(atts.getValue(a));
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }

        if (nameAttribute == null) {
            reportAbsence("name");
        }
    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        checkEmpty();
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:entity-ref",
                                                getPackageData().getLocalLicenseId());
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new ValueOf(new StringLiteral('&' + nameAttribute + ';'), true, false);
    }

}

// Copyright (c) 2009 Saxonica Limited.