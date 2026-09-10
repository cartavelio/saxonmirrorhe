package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:set-attribute elements in stylesheet.
 * The ixsl:set-attribute element has mandatory attributes name (the name of the attribute to be set)
 * and select (the new value), and optional attribute object (the element to set the attribute on, when no object attribute
 * is present, the current element node is used)
 */

public class IXSLSetAttribute extends StyleElement {

    private Expression name;
    private Expression select;
    private Expression object;

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
     * Determine whether this type of element is allowed to contain a sequence constructor
     *
     * @return false: no, the element must be empty
     */

    @Override
    protected boolean mayContainSequenceConstructor() {
        return false;
    }

    @Override
    protected void prepareAttributes() {

        AttributeMap atts = attributes();

        for (AttributeInfo att : atts) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            if (f.equals("name")) {
                String nameAtt = Whitespace.trim(att.getValue());
                name = makeAttributeValueTemplate(nameAtt, att);
            } else if (f.equals("select")) {
                String selectAtt = att.getValue();
                select = makeExpression(selectAtt, att);
            } else if (f.equals("object")) {
                String objectAtt = att.getValue();
                object = makeExpression(objectAtt, att);
            } else {
                checkUnknownAttribute(attName);
            }
        }
        if (object == null) {
            object = makeExpression(".", null);
        }
        if (name == null) {
            reportAbsence("name");
        }
        if (select == null) {
            reportAbsence("select");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        checkEmpty();
        name = typeCheck("name", name);
        select = typeCheck("select", select);
        object = typeCheck("object", object);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression e = IXSLFunctionSet.getInstance().makeFunction("set-attribute", 3).makeFunctionCall(name, select, object);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
