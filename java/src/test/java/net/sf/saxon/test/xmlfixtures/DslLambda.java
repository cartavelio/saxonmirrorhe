package net.sf.saxon.test.xmlfixtures;

import net.sf.saxon.trans.XPathException;

@FunctionalInterface
public interface DslLambda {
    abstract void buildWith(XmlFixtureBuilder builder) throws XPathException;
}
