package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:set-attribute elements in stylesheet.
 * The ixsl:set-attribute element has mandatory attributes name (the name of the attribute to be set)
 * and select (the new value)
 */

public class IXSLSetAttribute extends StyleElement {

    private Expression name;
    private Expression select;

    /**
     * Determine whether this node is an instruction.
     *
     * @return true - it is an instruction
     */

    public boolean isInstruction() {
        return true;
    }

    /**
     * Determine whether this type of element is allowed to contain a sequence constructor
     *
     * @return true: no, the element must be empty
     */

    public boolean mayContainSequenceConstructor() {
        return false;
    }

    public void prepareAttributes() throws XPathException {

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            if (f.equals("name")) {
                String nameAtt = Whitespace.trim(atts.getValue(a));
                name = makeAttributeValueTemplate(nameAtt, a);
            } else if (f.equals("select")) {
                String selectAtt = atts.getValue(a);
                select = makeExpression(selectAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }
        if (name == null) {
            reportAbsence("name");
        }
        if (select == null) {
            reportAbsence("select");
        }
    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        name = typeCheck("name", name);
        select = typeCheck("select", select);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression e = IXSLFunctionSet.getInstance().makeFunction("set-attribute", 2).makeFunctionCall(name, select);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018 Saxonica Limited.
