package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

public interface ArityOneFunction {

    /**
     * Call a function with one argument
     *
     * @param context the dynamic evaluation context
     * @param arg0    the first argument
     * @return the result of the function call
     * @throws XPathException if the call fails with a dynamic error
     */

    Sequence call1(XPathContext context, Sequence arg0) throws XPathException;
}


