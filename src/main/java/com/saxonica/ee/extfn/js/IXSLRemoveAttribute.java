package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:remove-attribute elements in stylesheet.
 * The ixsl:remove-attribute element has mandatory attribute name (the name of the attribute to be removed)
 * and optional attribute object (the element from which the attribute is removed, when no object attribute
 * is present, the current element node is used)
 */

public class IXSLRemoveAttribute extends StyleElement {

    private Expression name;
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
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        name = typeCheck("name", name);
        object = typeCheck("object", object);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression e = IXSLFunctionSet.getInstance().makeFunction("remove-attribute", 2).makeFunctionCall(name, object);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
