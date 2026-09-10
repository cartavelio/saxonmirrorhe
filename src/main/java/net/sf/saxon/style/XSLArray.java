package net.sf.saxon.style;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.trans.XPathException;

/**
 * xsl:array element in stylesheet - proposed XSLT 4.0 instruction
 * The xsl:array element has the same content model as xsl:sequence. It evaluates the expression
 * in its select attribute or its contained sequence constructor, and turns the resulting sequence
 * into an array of single-item members.
 */

public class XSLArray extends XSLSequence {


    @Override
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

// Copyright (c) 2018-2020 Saxonica Limited

