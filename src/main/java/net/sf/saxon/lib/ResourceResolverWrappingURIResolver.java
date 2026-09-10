////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import java.util.Objects;

/**
 * A {@link ResourceResolver} implemented by wrapping a supplied {@link URIResolver}
 */

public class ResourceResolverWrappingURIResolver implements ResourceResolver {

    private final URIResolver uriResolver;

    /**
     * Create a {@link ResourceResolver} by wrapping a supplied {@link URIResolver}
     * @param uriResolver the {@link URIResolver} to which this {@link ResourceResolver} will delegate
     */
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


