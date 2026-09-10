package net.sf.saxon.trans;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.XPathContext;

public interface IElaborationService {

    void push(Component component, Outputter out, XPathContext context) throws XPathException;
}


