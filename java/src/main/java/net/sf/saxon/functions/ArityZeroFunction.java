package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

public interface ArityZeroFunction {

    /**
     * Call a function with no arguments
     *
     * @param context the dynamic evaluation context
     * @return the result of the function call
     * @throws XPathException if the call fails with a dynamic error
     */

    Sequence call0(XPathContext context) throws XPathException;
}


