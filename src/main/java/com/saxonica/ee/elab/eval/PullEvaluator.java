package com.saxonica.ee.elab.eval;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface PullEvaluator {

    SequenceIterator eval(XPathContext context) throws XPathException;
}

