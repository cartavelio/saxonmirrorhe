package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.DoInstr;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for saxon:do elements in stylesheet.
 * The saxon:do element has a mandatory attribute action, an expression typically with side-effects.
 * The effect of the instruction is to evaluate the action expression and discard the result. It
 * is also designed to resist attempts at aggressive optimization, e.g loop-lifting
 * @since 9.9
 */

public class SaxonDo extends ExtensionInstruction {

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
        String actionAtt = null;

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            String value = att.getValue();
            if (f.equals("action")) {
                actionAtt = value;
                action = makeExpression(actionAtt, att);
            } else {
                checkUnknownAttribute(attName);
            }
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:while",
                                                getPackageData().getLocalLicenseId());
        action = typeCheck("action", action);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new DoInstr(action);
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
