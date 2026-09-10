package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:remove-property elements in stylesheet.
 * The ixsl:remove-property element has a mandatory attribute name (the name of the property to be removed,
 * or dot-separated composite name),
 * and an optional attribute object (the object that the property belongs to, when no object attribute
 * is present, the window object is used)
 */

public class IXSLRemoveProperty extends StyleElement {

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
     * @return true: no, the element must be empty
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
                case "name":
                    String nameAtt = Whitespace.trim(value);
                    name = makeAttributeValueTemplate(nameAtt, att);
                    break;
                case "object":
                    String objectAtt = value;
                    object = makeExpression(objectAtt, att);
                    break;
                default:
                    checkUnknownAttribute(attName);
                    break;
            }
        }
        if (object == null) {
            try {
                object = IXSLFunctionSet.getInstance().makeFunction("window", 0).makeFunctionCall();
            } catch (XPathException e) {
                compileError(e);
            }
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
        Expression e = IXSLFunctionSet.getInstance().makeFunction("remove-property", 2).makeFunctionCall(name, object);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
