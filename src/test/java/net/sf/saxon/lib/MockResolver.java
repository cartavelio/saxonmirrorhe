package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.net.URI;

public class MockResolver implements ResourceResolver {
    private final URI mockSourceUri;
    public MockResolver(URI mockSourceUri) {
        this.mockSourceUri = mockSourceUri;
    };

    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        return new StreamSource(mockSourceUri.toASCIIString());
    }
}