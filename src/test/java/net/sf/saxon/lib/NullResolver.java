package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;

public class NullResolver implements ResourceResolver {
    public NullResolver() {};

    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        return null;
    }
}