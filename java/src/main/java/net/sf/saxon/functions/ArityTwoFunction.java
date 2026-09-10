package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

public interface ArityTwoFunction {

    /**
     * Call a function with two arguments
     * @param context the dynamic evaluation context
     * @param arg0 the first argument
     * @param arg1 the second argument
     * @return the result of the function call
     * @throws XPathException if the call fails with a dynamic error
     */

    Sequence call2(XPathContext context, Sequence arg0, Sequence arg1) throws XPathException;
}


