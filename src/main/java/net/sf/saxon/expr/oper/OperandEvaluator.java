package net.sf.saxon.expr.oper;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;

public interface OperandEvaluator {

    TailCall processLeavingTail(Outputter outputter, XPathContext context) throws XPathException;

    SequenceIterator iterate(XPathContext context) throws XPathException;

    Item evaluateItem(XPathContext context) throws XPathException;

    boolean effectiveBooleanValue(XPathContext context) throws XPathException;

    UnicodeString evaluateAsString(XPathContext context) throws XPathException;

}

