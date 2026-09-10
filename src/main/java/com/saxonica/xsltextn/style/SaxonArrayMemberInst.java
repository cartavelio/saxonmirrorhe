package com.saxonica.xsltextn.style;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.functions.extfn.VendorFunctionSetPE;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.XSLSequence;
import net.sf.saxon.trans.XPathException;

/**
 * saxon:array-member element in stylesheet.
 * The saxon:array-member element has the same content model as xsl:sequence. It evaluates the expression
 * in its select attribute or its contained sequence constructor, and wraps the resulting sequence
 * into an external object. This external object is recognized by the saxon:array instruction, and is
 * converted into a single member of the array.
 * The instruction also allows xsl:extension-element-prefixes etc.
 */

public class SaxonArrayMemberInst extends XSLSequence {


    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        if (getSelectExpression() == null) {
            setSelectExpression(compileSequenceConstructor(exec, decl, false));
        }
        ProfessionalConfiguration config = (ProfessionalConfiguration)exec.getConfiguration();
        SystemFunction arrayMem = config.getVendorFunctionSet().makeFunction("array-member", 1);
        return arrayMem.makeFunctionCall(getSelectExpression());
    }

}

// Copyright (c) 2018-2020 Saxonica Limited

