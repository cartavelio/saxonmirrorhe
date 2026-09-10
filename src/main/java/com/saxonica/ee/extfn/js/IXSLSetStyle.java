package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:set-style elements in stylesheet.
 * The ixsl:set-style element has mandatory attributes name (the name of the style property to be set),
 * and select (the new value for the style property),
 * and an optional attribute object (the node to set the style property on, when no object attribute
 * is present, the current node is used)
 */

public class IXSLSetStyle extends StyleElement {

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
            object = makeExpression(".", -1);
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
        Expression e = IXSLFunctionSet.getInstance().makeFunction("set-style", 3).makeFunctionCall(name, select, object);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018 Saxonica Limited.
