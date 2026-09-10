package com.saxonica.ee.jaxp;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.lib.ResourceResolver;
import net.sf.saxon.trans.XPathException;
import org.w3c.dom.ls.LSInput;
import org.w3c.dom.ls.LSResourceResolver;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.io.Reader;
import java.io.StringReader;

/**
 * This class implements the Saxon ResourceResolver interface as a wrapper around
 * a DOM Level 3 LSResourceResolver. This serves two purposes: it allows the
 * same underlying object to be used in both roles, and it allows an LSResourceResolver
 * to be passed around the system in places where a URIResolver is expected, for
 * example in the PipelineConfiguration
 */

public class ResourceResolverWrappingLSResourceResolver implements ResourceResolver {

    private final LSResourceResolver resolver;

    public ResourceResolverWrappingLSResourceResolver(LSResourceResolver resolver) {
        this.resolver = resolver;
    }

    public LSResourceResolver getLSResourceResolver() {
        return resolver;
    }

    /**
     * Called by an XSLT processor when it encounters
     * an xsl:include, xsl:import, or document() function.
     *
     *
     * @param request the resource request
     * @return A Source object, or null if the href cannot be resolved,
     *         and the processor should try to resolve the URI itself.
     * @throws XPathException
     *          if an error occurs when trying to
     *          resolve the URI.
     */
    /*@Nullable*/
    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        String type = request.nature;
        if (request.nature.equals(ResourceRequest.XML_NATURE)) {
            type = NamespaceConstant.XML;
        }

        LSInput lsin = resolver.resolveResource(type,
                                                request.publicId,
                                                request.uriIsNamespace ? request.uri : null,
                                                request.relativeUri,
                                                request.baseUri);
        if (lsin == null) {
            return null;
        }

        Reader reader = lsin.getCharacterStream();
        InputStream stream = lsin.getByteStream();
        String content = lsin.getStringData();
        String systemId = lsin.getSystemId();
        String publicId = lsin.getPublicId();

        if (content != null) {
            reader = new StringReader(content);
        }

        StreamSource source = new StreamSource();
        source.setSystemId(systemId);
        source.setPublicId(publicId);
        source.setReader(reader);
        source.setInputStream(stream);

        return source;

    }
}

// Copyright (c) 2005-2023 Saxonica Limited