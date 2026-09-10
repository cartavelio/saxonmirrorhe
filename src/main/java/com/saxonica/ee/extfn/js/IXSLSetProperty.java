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
 * Handler for ixsl:set-property elements in stylesheet.
 * The ixsl:set-property element has mandatory attributes name (the name of the property to be set,
 * or dot-separated composite name), and select (the new property value),
 * and optional attributes object (the object that the property belongs to, when no object attribute
 * is present, the window object is used) and convert-value (available from JS3, specifies whether
 * the supplied new property value is converted from XDM to JS or retained in SaxonJS internal form.)
 */

public class IXSLSetProperty extends StyleElement {

    private Expression name;
    private Expression select;
    private Expression object;
    private Expression convert;
    private boolean convertAttSupplied = false;

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

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            String value = att.getValue();
            switch (f) {
                case "name":
                    String nameAtt = Whitespace.trim(value);
                    name = makeAttributeValueTemplate(nameAtt, att);
                    break;
                case "select":
                    String selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break;
                case "object":
                    String objectAtt = value;
                    object = makeExpression(objectAtt, att);
                    break;
                case "convert-value":
                    boolean convertVal = processBooleanAttribute("convert-value", value);
                    if (convertVal) {
                        convert = makeExpression("true()", att);
                    } else {
                        convert = makeExpression("false()", att);
                    }
                    convertAttSupplied = true;
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
        if (select == null) {
            reportAbsence("select");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        name = typeCheck("name", name);
        select = typeCheck("select", select);
        object = typeCheck("object", object);
        convert = typeCheck("convert-value", convert);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        if (convertAttSupplied) {
            if (!"JS3".equals(exec.getCompilerInfo().getTargetEdition())) {
                compileError("ixsl:set-property/@convert-value is available only if the target edition is JS3");
            } else {
                Expression e = IXSL3FunctionSet.getInstance().makeFunction("set-property", 4).makeFunctionCall(name, select, object, convert);
                e.setRetainedStaticContext(makeRetainedStaticContext());
                return e;
            }
        }
        Expression e = IXSLFunctionSet.getInstance().makeFunction("set-property", 3).makeFunctionCall(name, select, object);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
