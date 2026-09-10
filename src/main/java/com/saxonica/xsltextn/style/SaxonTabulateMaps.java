package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.TabulateMaps;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for saxon:tabulate-maps elements in stylesheet.
 */

public class SaxonTabulateMaps extends ExtensionInstruction {

    private Expression root;
    private Expression select;

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

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            String value = att.getValue();
            switch (f) {
                case "root":
                    root = makeExpression(value, att);
                    break;
                case "select":
                    select = makeExpression(value, att);
                    break;
                default:
                    checkUnknownAttribute(attName);
                    break;
            }
        }

        if (root == null) {
            reportAbsence("root");
        }
        if (select == null) {
            reportAbsence("select");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:tabulate",
                                                getPackageData().getLocalLicenseId());
        root = typeCheck("root", root);
        select = typeCheck("select", select);

        getContainingPackage().markNonExportable("Contains saxon:tabulate-maps instruction", SaxonErrorCode.SXST0071);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new TabulateMaps(root, select);
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
