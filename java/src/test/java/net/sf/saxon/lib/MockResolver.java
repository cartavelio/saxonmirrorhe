package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.math.BigInteger;
import java.net.URI;

public class MockResolver implements ResourceResolver {
    private final URI mockSourceUri;
    public MockResolver(URI mockSourceUri) {
        this.mockSourceUri = mockSourceUri;
    };

    public static String encode(String requestedUri) {
        return new BigInteger(requestedUri.getBytes()).toString(16);
    }

    public String resolvedMockUri(String requestedUri) {
        return mockSourceUri.resolve("#" + encode(requestedUri)).toASCIIString();
    }

    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        return new StreamSource(resolvedMockUri(request.uri));
    }
}