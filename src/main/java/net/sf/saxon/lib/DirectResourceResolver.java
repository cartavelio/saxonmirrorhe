package net.sf.saxon.lib;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.FilterFactory;
import net.sf.saxon.event.IDFilter;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;

public class DirectResourceResolver implements ResourceResolver {

    private final Configuration config;

    public DirectResourceResolver(Configuration config) {
        this.config = config;
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
        String relativeURI = request.relativeUri;
        String id = null;

        // Extract any fragment identifier. Note, this code is no longer used to
        // resolve fragment identifiers in URI references passed to the document()
        // function: the code of the document() function handles these itself.

        int hash = relativeURI.indexOf('#');
        if (hash >= 0) {
            request = request.copy();
            request.relativeUri = relativeURI.substring(0, hash);
            id = relativeURI.substring(hash + 1);
            // System.err.println("StandardURIResolver, href=" + href + ", id=" + id);
        }

        Source ss;
        if (ResourceRequest.XSLT_NATURE.equals(request.nature)) {
            ss = new SAXSource(config.getStyleParser(), new InputSource(request.uri));
        } else {
            ss = new StreamSource(request.uri);
        }

        if (id != null) {
            final String idFinal = id;
            FilterFactory factory = next -> new IDFilter(next, idFinal);
            ss = AugmentedSource.makeAugmentedSource(ss);
            ((AugmentedSource) ss).addFilter(factory);
        }

        return ss;

    }
}

