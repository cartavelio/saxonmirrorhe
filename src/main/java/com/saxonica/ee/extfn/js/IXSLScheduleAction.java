package com.saxonica.ee.extfn.js;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.om.*;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.style.XSLCallTemplate;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.value.Int64Value;


/**
 * Handler for ixsl:schedule-action elements in stylesheet.
 * The ixsl:schedule-action element should have at least one of the following optional attributes:
 * wait, an integer expression; document, a space-separated list of URIs;
 * http-request, an expression which evaluates to a map.
 *
 * <p>The content provides the delayed action.</p>
 */

public class IXSLScheduleAction extends StyleElement {

    private Expression wait;
    private Expression document;
    private Expression httpRequest;

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

        AttributeMap atts = attributes();
        int c = 0, d = 0;
        for (AttributeInfo att : atts) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            if (f.equals("wait")) {
                wait = makeExpression(att.getValue(), att);
                c++;
            } else if (f.equals("document")) {
                document = makeAttributeValueTemplate(att.getValue(), att);
                c++;
                d++;
            } else if (f.equals("http-request")) {
                httpRequest = makeExpression(att.getValue(), att);
                c++;
                d++;
            } else {
                checkUnknownAttribute(attName);
            }
        }
        if (c == 0) {
            compileError("At least one attribute should be set on ixsl:schedule-action", "XTSE0010");
        }
        if (d == 2) {
            compileError("Combination of attributes set on ixsl:schedule-action is not permitted", "XTSE0010");
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        wait = typeCheck("wait", wait);
        document = typeCheck("document", document);
        httpRequest = typeCheck("http-request", httpRequest);
        if (wait == null) {
            wait = Literal.makeLiteral(Int64Value.ZERO);
        }
        if (document == null) {
            document = Literal.makeEmptySequence();
        }

        // check that the only child is a xsl:call-template element
        AxisIterator kids = iterateAxis(AxisInfo.CHILD);
        NodeInfo child1 = kids.next();
        if (child1 != null && !(child1 instanceof XSLCallTemplate)) {
            compileError("Only xsl:call-template is allowed within ixsl:schedule-action", "XTSE0010");
        }
        NodeInfo child2 = kids.next();
        if (child2 != null) {
            compileError("Only one child permitted within ixsl:schedule-action", "XTSE0010");
        }
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression action = compileSequenceConstructor(exec, decl, true);
        if (action == null) {
            action = Literal.makeEmptySequence();
        }
        Expression e;
        if (httpRequest == null) {
            e = IXSLFunctionSet.getInstance().makeFunction("schedule-action", 3).makeFunctionCall(wait, document, action);
        } else {
            e = IXSLFunctionSet.getInstance().makeFunction("schedule-action", 4).makeFunctionCall(wait, document, action, httpRequest);
        }
        e.setRetainedStaticContext(makeRetainedStaticContext());
        return e;
    }

}

// Copyright (c) 2009-2022 Saxonica Limited
