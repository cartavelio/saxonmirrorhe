package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.PendingUpdateList;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface UpdateEvaluator {

    void registerUpdates(XPathContext context, PendingUpdateList updates) throws XPathException;
}

