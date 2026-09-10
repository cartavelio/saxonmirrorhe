package com.saxonica.xsltextn.style;

import com.saxonica.expr.DeepUpdate2;
import com.saxonica.expr.UpdateTarget;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for saxon:deep-update elements in stylesheet.
 */

public class SaxonDeepUpdate extends ExtensionInstruction {

    private Expression root;
    private Expression select;
    private Expression action;

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
        return new ForEach(root,
                           new DeepUpdate2(UpdateTarget.UNKNOWN, new ContextItemExpression(), select, action));
    }



}

// Copyright (c) 2018-2023 Saxonica Limited
