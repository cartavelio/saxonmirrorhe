package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import java.util.Objects;

public class ResourceResolverWrappingURIResolver implements ResourceResolver {

    private final URIResolver uriResolver;

    public ResourceResolverWrappingURIResolver(URIResolver uriResolver) {
        Objects.requireNonNull(uriResolver);
        this.uriResolver = uriResolver;
    }

    /**
     * Process a resource request to deliver a resource
     *
     * @param request the resource request
     * @return the returned Source; or null to delegate resolution to another resolver
     * @throws XPathException if the request is invalid in some way, or if the identified resource is unsuitable,
     *                        or if resolution is to fail rather than being delegated to another resolver.
     */
    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        if (request.relativeUri != null && request.baseUri != null) {
            try {
                return uriResolver.resolve(request.relativeUri, request.baseUri);
            } catch (TransformerException e) {
                throw XPathException.makeXPathException(e);
            }
        }
        if (request.uri != null) {
            try {
                return uriResolver.resolve(request.uri, request.baseUri);
            } catch (TransformerException e) {
                throw XPathException.makeXPathException(e);
            }
        }
        return null;
    }

    public URIResolver getWrappedURIResolver() {
        return uriResolver;
    }
}


