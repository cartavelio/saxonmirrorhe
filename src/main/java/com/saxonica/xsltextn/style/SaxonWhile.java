package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.While;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.XPathException;


/**
 * Handler for saxon:while elements in stylesheet.
 * The saxon:while element has a mandatory attribute test, a boolean expression.
 * The content is output repeatedly so long as the test condition is true.
 */

public class SaxonWhile extends ExtensionInstruction {

    private Expression test;

    /**
     * Determine whether this type of element is allowed to contain a template-body
     *
     * @return true: yes, it may contain a template-body
     */

    @Override
    protected boolean mayContainSequenceConstructor() {
        return true;
    }

    @Override
    protected void prepareAttributes() {
        test = XSLIf.prepareTestAttribute(this);
        if (test == null) {
            reportAbsence("test");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:while",
                                                getPackageData().getLocalLicenseId());
        test = typeCheck("test", test);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression action = compileSequenceConstructor(exec, decl, true);
        if (action == null) {
            action = Literal.makeEmptySequence();
        }
        return new While(test, action);
    }

}

// Copyright (c) 2009-2022 Saxonica Limited
