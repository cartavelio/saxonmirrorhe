package com.saxonica.ee.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface StringEvaluator {

    UnicodeString eval(XPathContext context) throws XPathException;
}

