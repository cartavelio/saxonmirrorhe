package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:set-property elements in stylesheet.
 * The ixsl:set-property element has mandatory attributes name (the name of the attribute to be set,
 * or dot-separated composite name), and select (the new property value),
 * and an optional attribute object (the object that the property belongs to, when no object attribute
 * is present, the window object is used)
 */

public class IXSLSetProperty extends StyleElement {

    private Expression name;
    private Expression select;
    private Expression object;

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
            } else if (f.equals("object")) {
                String objectAtt = atts.getValue(a);
                object = makeExpression(objectAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }
        if (object == null) {
            object = IXSLFunctionSet.getInstance().makeFunction("window", 0).makeFunctionCall();
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
        object = typeCheck("object", object);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression e = IXSLFunctionSet.getInstance().makeFunction("set-property", 3).makeFunctionCall(name, select, object);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018 Saxonica Limited.
