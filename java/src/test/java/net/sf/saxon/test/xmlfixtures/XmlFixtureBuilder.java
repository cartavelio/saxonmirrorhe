package net.sf.saxon.test.xmlfixtures;

import net.sf.saxon.Configuration;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.nodetest.AnyGNode;
import net.sf.saxon.sapling.SaplingDocument;
import net.sf.saxon.sapling.Saplings;
import net.sf.saxon.sapling.dsl.DocumentDsl;
import net.sf.saxon.trans.XPathException;

public class XmlFixtureBuilder {
    public Item doc(DocumentDsl builder) throws XPathException {
        SaplingDocument document = Saplings.doc(staticBaseUri, builder);
        return document.toNodeInfo(configuration);
    }

    public Item elem(DocumentDsl builder) throws XPathException {
        return ((NodeInfo) doc(builder)).iterateChildAxis()   .next();
    }

    private final Configuration configuration;
    private final String staticBaseUri;

    public XmlFixtureBuilder(Configuration configuration) {
        this.configuration = configuration;
        this.staticBaseUri = "http://www.example.org/";
    }
}
