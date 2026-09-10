package com.saxonica.xsltextn.style;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.instruct.Doctype;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;

/**
 * A saxon:doctype element in the stylesheet.
 */

public class SaxonDoctype extends StyleElement {

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
     * @return true: yes, it may contain a template-body
     */

    @Override
    protected boolean mayContainSequenceConstructor() {
        return true;
    }

    @Override
    protected void prepareAttributes() {

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            checkUnknownAttribute(attName);
        }

    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:doctype",
                                                getPackageData().getLocalLicenseId());
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression content = compileSequenceConstructor(exec, decl, true);
        if (content == null) {
            content = Literal.makeEmptySequence();
        }
        Doctype inst = new Doctype(content);
        return inst;
    }


}

// Copyright (c) 2009-2022 Saxonica Limited