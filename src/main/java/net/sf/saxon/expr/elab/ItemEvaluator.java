package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface ItemEvaluator {

    Item eval(XPathContext context) throws XPathException;
}

