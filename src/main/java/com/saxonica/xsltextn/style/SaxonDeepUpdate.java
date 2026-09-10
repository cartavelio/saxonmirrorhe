package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.DeepUpdate;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for saxon:deep-update elements in stylesheet.
 */

public class SaxonDeepUpdate extends StyleElement {

    private Expression root;
    private Expression select;
    private Expression action;

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
        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            if (f.equals("root")) {
                String rootAtt = atts.getValue(a);
                root = makeExpression(rootAtt, a);
            } else if (f.equals("select")) {
                String selectAtt = atts.getValue(a);
                select = makeExpression(selectAtt, a);
            } else if (f.equals("action")) {
                String actionAtt = atts.getValue(a);
                action = makeExpression(actionAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }

        if (root == null) {
            reportAbsence("root");
        }
        if (select == null) {
            reportAbsence("select");
        }
        if (action == null) {
            reportAbsence("action");
        }
    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:deep-update",
                                                getPackageData().getLocalLicenseId());
        root = typeCheck("root", root);
        select = typeCheck("select", select);
        action = typeCheck("action", action);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new DeepUpdate(root, select, action);
    }

}

// Copyright (c) 2018 Saxonica Limited.
