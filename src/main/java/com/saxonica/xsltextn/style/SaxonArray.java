package com.saxonica.xsltextn.style;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.XSLSequence;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:array element in stylesheet.
 * The saxon:array element has the same content model as xsl:sequence. It evaluates the expression
 * in its select attribute or its contained sequence constructor, and turns the resulting sequence
 * into an array of single-item members.
 * The instruction also allows xsl:extension-element-prefixes etc.
 */

public class SaxonArray extends XSLSequence {


    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        if (getSelectExpression() == null) {
            setSelectExpression(compileSequenceConstructor(exec, decl, false));
        }
        SystemFunction sf = ArrayFunctionSet.getInstance().makeFunction("_from-sequence", 1);
        Expression result = sf.makeFunctionCall(getSelectExpression());
//        ArrayFromSequence defn = new ArrayFromSequence();
//        Expression result = IntegratedFunctionLibrary.makeFunctionCall(defn, new Expression[]{getSelectExpression()});
        ExpressionTool.copyLocationInfo(getSelectExpression(), result);
        return result;
    }

}

// Copyright (c) 2018 Saxonica Limited.

