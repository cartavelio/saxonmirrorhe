////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package com.saxonica.trans;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.ma.map.MapFunctionSet;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.XSLSequence;
import net.sf.saxon.trans.XPathException;

/**
 * xsl:array element in stylesheet - proposed XSLT 4.0 instruction
 * The xsl:array element has the same content model as xsl:sequence. It evaluates the expression
 * in its select attribute or its contained sequence constructor, and turns the resulting sequence
 * into an array of single-item members.
 */

public class XSLArray extends XSLSequence {

    private Expression use;

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
        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            String f = attName.getDisplayName();
            switch (f) {
                case "select":
                    setSelectExpression(makeExpression(value, att));
                    break;
                case "use":
                    use = makeExpression(value, att);
                    break;
                case "composite":
                    compileWarning("XSLT 4.0 spec change: replace composite='yes' with use='?value'", "SXWN0001");
                    boolean composite = processBooleanAttribute("composite", value);
                    if (composite) {
                        use = makeExpression("?value", att);
                    }
                    break;
                default:
                    checkUnknownAttribute(attName);
                    break;
            }
        }
    }



    public void validate(ComponentDeclaration decl) throws XPathException {
        super.validate(decl);
        use = typeCheck("use", use);
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
            // compile xsl:array select="S" use="U" into array:of(S ! map:entry('value', U)).
            SystemFunction mapEntry = MapFunctionSet.getInstance(40).makeFunction("entry", 2);
            Expression mapEntryCall = mapEntry.makeFunctionCall(
                    new StringLiteral("value"), use);
            ForEach bangExpression = new ForEach(getSelectExpression(), mapEntryCall);
            SystemFunction arrayOf = ArrayFunctionSet.getInstance(40).makeFunction("of-members", 1);
            functionCall = arrayOf.makeFunctionCall(bangExpression);
        }
        ExpressionTool.copyLocationInfo(getSelectExpression(), functionCall);
        return functionCall;
    }

}



