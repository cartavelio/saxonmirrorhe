package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface StringEvaluator {

    String eval(XPathContext context) throws XPathException;
}

