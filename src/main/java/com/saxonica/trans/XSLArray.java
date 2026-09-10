////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package com.saxonica.trans;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.SequenceEvaluator;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.om.*;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.XSLSequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;

/**
 * xsl:array element in stylesheet - proposed XSLT 4.0 instruction
 * The xsl:array element has the same content model as xsl:sequence. It evaluates the expression
 * in its select attribute or its contained sequence constructor, and turns the resulting sequence
 * into an array of single-item members.
 */

public class XSLArray extends XSLSequence {

    private FunctionItem use;

    /**
     * Ask whether the element is in the XSLT namespace
     *
     * @return true if the element is in the XSLT namespace
     */
    @Override
    public boolean isInXsltNamespace() {
        return getNodeName().hasURI(NamespaceUri.XSLT);
    }

    @Override
    protected void prepareAttributes() {
        Expression useExpression = null;
        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            String f = attName.getDisplayName();
            switch (f) {
                case "select":
                    setSelectExpression(makeExpression(value, att));
                    break;
                case "use":
                    useExpression = makeExpression(value, att);
                    break;
                case "composite":
                    compileWarning("XSLT 4.0 spec change: replace composite='yes' with use='?value'", "SXWN0001");
                    boolean composite = processBooleanAttribute("composite", value);
                    if (composite) {
                        useExpression = makeExpression("?value", att);
                    }
                    break;
                default:
                    checkUnknownAttribute(attName);
                    break;
            }
        }
        if (useExpression != null) {
            use = new CallableFunction(1, new FocusFunction(useExpression),
                                       new SpecificFunctionType(new SequenceType[]{SequenceType.SINGLE_ITEM},
                                                                SequenceType.ANY_SEQUENCE));
        }
    }

    private static class FocusFunction implements Callable {

        SequenceEvaluator evaluator;

        public FocusFunction(Expression expression) {
            this.evaluator = expression.makeElaborator().eagerly();
        }
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Item current = arguments[0].head();
            XPathContextMinor c2 = context.newMinorContext();
            c2.setCurrentIterator(new ManualIterator(current));
            return evaluator.evaluate(c2);
        }
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        if (isInXsltNamespace()) {
            requireXslt40Element();
        } else if (!getConfiguration().isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            compileError("saxon:array requires a Saxon-PE or -EE license");
        }
        if (getSelectExpression() == null) {
            setSelectExpression(compileSequenceConstructor(exec, decl, false));
        }

        Expression functionCall;
        if (use == null || use instanceof ContextItemExpression) {
            SystemFunction build = ArrayFunctionSet.getInstance(40).makeFunction("build", 1);
            functionCall = build.makeFunctionCall(getSelectExpression());
        } else {
            SystemFunction build = ArrayFunctionSet.getInstance(40).makeFunction("build", 2);
            functionCall = build.makeFunctionCall(getSelectExpression(), new FunctionLiteral(use));
        }
        ExpressionTool.copyLocationInfo(getSelectExpression(), functionCall);
        return functionCall;
    }

}



