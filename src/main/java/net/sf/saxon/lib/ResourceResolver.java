package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;

@FunctionalInterface
public interface ResourceResolver {

    /**
     * Process a resource request to deliver a resource
     * @param request the resource request
     * @return the returned Source; or null to delegate resolution to another resolver
     * @throws XPathException if the request is invalid in some way, or if the identified resource is unsuitable,
     * or if resolution is to fail rather than being delegated to another resolver.
     */

    Source resolve(ResourceRequest request) throws XPathException;

}

