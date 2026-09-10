package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * Handler for ixsl:remove-attribute elements in stylesheet.
 * The ixsl:remove-attribute element has mandatory attribute name (the name of the attribute to be removed)
 * and optional attribute namespace (the namespace of the attribute to be removed)
 */

public class IXSLRemoveAttribute extends StyleElement {

    private Expression name;
    private Expression namespace;

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
            } else if (f.equals("namespace")) {
                String namespaceAtt = Whitespace.trim(atts.getValue(a));
                namespace = makeAttributeValueTemplate(namespaceAtt, a);
            } else {
                checkUnknownAttribute(atts.getNodeName(a));
            }
        }
        if (namespace == null) {
            namespace = new StringLiteral("");
        }
        if (name == null) {
            reportAbsence("name");
        }
    }

    public void validate(ComponentDeclaration decl) throws XPathException {
        name = typeCheck("name", name);
        namespace = typeCheck("namespace", namespace);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression e = IXSLFunctionSet.getInstance().makeFunction("remove-attribute", 2).makeFunctionCall(name, namespace);
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2018 Saxonica Limited.
