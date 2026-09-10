package com.saxonica.ee.elab.eval;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface BooleanEvaluator {

    boolean eval(XPathContext context) throws XPathException;
}

