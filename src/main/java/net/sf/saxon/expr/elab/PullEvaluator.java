package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface PullEvaluator {

    SequenceIterator iterate(XPathContext context) throws XPathException;
}

