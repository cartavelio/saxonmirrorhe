package com.saxonica.ee.jaxp;

import org.w3c.dom.ls.LSInput;
import org.w3c.dom.ls.LSResourceResolver;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.io.Reader;
import java.io.StringReader;

/**
 * This class implements the JAXP URIResolver as a wrapper around
 * a DOM Level 3 LSResourceResolver. This serves two purposes: it allows the
 * same underlying object to be used in both roles, and it allows an LSResourceResolver
 * to be passed around the system in places where a URIResolver is expected, for
 * example in the PipelineConfiguration
 */

public class URIResourceResolver implements URIResolver {

    private LSResourceResolver resolver;

    public URIResourceResolver(LSResourceResolver resolver) {
        this.resolver = resolver;
    }

    public LSResourceResolver getLSResourceResolver() {
        return resolver;
    }

    /**
     * Called by an XSLT processor when it encounters
     * an xsl:include, xsl:import, or document() function.
     *
     * @param href An href attribute, which may be relative or absolute.
     * @param base The base URI against which the first argument will be made
     *             absolute if the absolute URI is required.
     * @return A Source object, or null if the href cannot be resolved,
     *         and the processor should try to resolve the URI itself.
     * @throws javax.xml.transform.TransformerException
     *          if an error occurs when trying to
     *          resolve the URI.
     */
    /*@Nullable*/
    @Override
    public Source resolve(String href, String base) throws TransformerException {
        LSInput lsin = resolver.resolveResource(
                "http://www.w3.org/TR/REC-xml", null, null, href, base);
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

// Copyright (c) 2005-2020 Saxonica Limited