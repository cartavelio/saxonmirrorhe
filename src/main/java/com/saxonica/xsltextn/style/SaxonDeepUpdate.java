package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.DeepUpdate;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
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

    @Override
    public boolean isInstruction() {
        return true;
    }

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
                case "action":
                    action = makeExpression(value, att);
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
        if (action == null) {
            reportAbsence("action");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:deep-update",
                                                getPackageData().getLocalLicenseId());
        root = typeCheck("root", root);
        select = typeCheck("select", select);
        action = typeCheck("action", action);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new DeepUpdate(root, select, action);
    }



}

// Copyright (c) 2018-2022 Saxonica Limited
