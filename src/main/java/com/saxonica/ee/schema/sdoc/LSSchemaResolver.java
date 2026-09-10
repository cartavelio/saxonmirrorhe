package com.saxonica.ee.schema.sdoc;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.trans.XPathException;
import org.w3c.dom.ls.LSInput;
import org.w3c.dom.ls.LSResourceResolver;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.net.URI;
import java.net.URISyntaxException;

/**
 * This class is a SchemaURIResolver implemented as a wrapper around a JAXP LSResourceResolver
 */
public class LSSchemaResolver implements SchemaURIResolver {

    private final LSResourceResolver resourceResolver;
    private Configuration config;

    public LSSchemaResolver(LSResourceResolver resolver, Configuration config) {
        resourceResolver = resolver;
        this.config = config;
    }

    /**
     * Set the configuration information for use by the resolver
     *
     * @param config the Saxon Configuration (which will always be an {@link com.saxonica.config.EnterpriseConfiguration})
     */

    @Override
    public void setConfiguration(Configuration config) {
        this.config = config;
    }

    /**
     * Resolve a URI identifying a schema document, given the target namespace URI and
     * a set of associated location hints.
     *
     * @param targetNamespace the target namespaces of the module to be imported. The "null namesapce"
     *                        is identified by a zero-length string. In the case of an xsd:include directive, where no
     *                        target namespace is specified, the parameter is null.
     * @param baseURI         The base URI of the module containing the "import schema" declaration;
     *                        null if no base URI is known
     * @param locations       The set of URIs identified as schema location hints. In most cases (xsd:include, xsd:import,
     *                        xsi:schemaLocation, xsl:import-schema) there is only one URI in this list. With an XQuery "import module"
     *                        declaration, however, a list of URIs may be specified.
     * @return an array of Source objects each identifying a schema document to be loaded.
     *         These need not necessarily correspond one-to-one with the location hints provided.
     * @throws net.sf.saxon.trans.XPathException
     *          if the module cannot be located, and if delegation to the default
     *          module resolver is not required.
     */

    @Override
    public Source[] resolve(String targetNamespace, String baseURI, String[] locations) throws XPathException {

        Source[] sources = new Source[locations.length];

        for (int i = 0; i < locations.length; i++) {

            LSInput in = resourceResolver.resolveResource(NamespaceConstant.SCHEMA,
                    targetNamespace,
                    null,
                    locations[i],
                    baseURI);
            if (in != null) {
                StreamSource ss = new StreamSource();
                ss.setInputStream(in.getByteStream());
                ss.setReader(in.getCharacterStream());
                ss.setSystemId(in.getSystemId());
                ss.setPublicId(in.getPublicId());
                if (ss.getSystemId() == null) {
                    try {
                        URI buri = new URI(baseURI);
                        ss.setSystemId(buri.resolve(locations[i]).toString());
                    } catch (URISyntaxException err) {
                        ss.setSystemId(baseURI);
                    }
                }
                sources[i] = ss;
            } else {
                sources[i] = new StandardURIResolver(config).resolve(locations[i], baseURI);
            }
        }
        return sources;
    }
}

// Copyright (c) 2006-2023 Saxonica Limited
