package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.DoInstr;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for saxon:do elements in stylesheet.
 * The saxon:do element has a mandatory attribute action, an expression typically with side-effects.
 * The effect of the instruction is to evaluate the action expression and discard the result. It
 * is also designed to resist attempts at aggressive optimization, e.g loop-lifting
 * @since 9.9
 */

public class SaxonDo extends StyleElement {

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
        String actionAtt = null;

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            if (f.equals("action")) {
                actionAtt = atts.getValue(a);
                action = makeExpression(actionAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }
    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:while",
                                                getPackageData().getLocalLicenseId());
        action = typeCheck("action", action);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
//        Expression action = compileSequenceConstructor(exec, decl, true);
//        if (action == null) {
//            action = Literal.makeEmptySequence();
//        }
        return new DoInstr(action);
    }

}

// Copyright (c) 2018 Saxonica Limited.
