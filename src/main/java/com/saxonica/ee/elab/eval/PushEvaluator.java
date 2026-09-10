package com.saxonica.ee.elab.eval;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface PushEvaluator {

    void process(Outputter out, XPathContext context) throws XPathException;
}

