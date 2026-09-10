package net.sf.saxon.style;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;

/**
 * xsl:array element in stylesheet - proposed XSLT 4.0 instruction
 * The xsl:array element has the same content model as xsl:sequence. It evaluates the expression
 * in its select attribute or its contained sequence constructor, and turns the resulting sequence
 * into an array of single-item members.
 */

public class XSLArray extends XSLSequence {

    private boolean composite;

    @Override
    protected void prepareAttributes() {

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            String f = attName.getDisplayName();
            if (f.equals("select")) {
                setSelectExpression(makeExpression(value, att));
            } else if (f.equals("composite")) {
                composite = processBooleanAttribute("composite", value);
            } else {
                checkUnknownAttribute(attName);
            }
        }
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        if (getURI().equals(NamespaceConstant.XSLT)) {
            requireXslt40(null);
        } else if (!getConfiguration().isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            compileError("saxon:array requires a Saxon-PE or -EE license");
        }
        if (getSelectExpression() == null) {
            setSelectExpression(compileSequenceConstructor(exec, decl, false));
        }

        Expression functionCall;
        if (composite) {
            SystemFunction arrayOf = ArrayFunctionSet.getInstance().makeFunction("of", 1);
            functionCall = arrayOf.makeFunctionCall(getSelectExpression());
        } else {
            SystemFunction arrayFromSeq = ArrayFunctionSet.getInstance().makeFunction("_from-sequence", 1);
            functionCall = arrayFromSeq.makeFunctionCall(getSelectExpression());
        }
        ExpressionTool.copyLocationInfo(getSelectExpression(), functionCall);
        return functionCall;
    }

}

// Copyright (c) 2018-2020 Saxonica Limited

