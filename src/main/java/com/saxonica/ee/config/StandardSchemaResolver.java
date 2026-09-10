package com.saxonica.ee.config;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.lib.StandardEntityResolver;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;

/**
 * The default schema resolver used when no other is requested
 */
public class StandardSchemaResolver implements SchemaURIResolver {

    private EnterpriseConfiguration config;
    private URIResolver uriResolver;
    private static final StreamSource[] EMPTY_SOURCE_ARRAY = new StreamSource[0];

    /**
     * Default constructor. Must be followed by a call of setConfiguration()
     */

    public StandardSchemaResolver() {
    }

    /**
     * Create a StandardSchemaResolver and set the Configuration
     *
     * @param config the Configuration
     */

    public StandardSchemaResolver(EnterpriseConfiguration config) {
        this.config = config;
    }

    public StandardSchemaResolver(EnterpriseConfiguration config, URIResolver resolver) {
        this.config = config;
        this.uriResolver = resolver;
    }

    /**
     * Set the configuration information for use by the resolver
     *
     * @param config the Saxon Configuration
     */

    public void setConfiguration(Configuration config) {
        this.config = (EnterpriseConfiguration) config;
    }

    /**
     * Get the configuration used by the resolver
     * @return the configuration
     */

    public Configuration getConfiguration() {
        return config;
    }

    /**
     * Resolve a URI identifying a schema document, given the target namespace URI and
     * a set of associated location hints.
     *
     * @param targetNamespace the target namespaces of the module to be imported. The "null namespace"
     *                        is identified by a zero-length string. In the case of an xsd:include directive, where no
     *                        target namespace is specified, the parameter is null. For the standard schema resolver,
     *                        if the target namespace is already present in the schema cache, an empty array is
     *                        returned (so the existing schema is used unchanged); otherwise, the URIs listed
     *                        in the location hint(s) are dereferenced and returned in the form of StreamSource
     *                        objects.
     * @param baseURI         The base URI of the module containing the "import schema" declaration;
     *                        null if no base URI is known
     * @param locations       The set of URIs identified as schema location hints. In most cases (xsd:include, xsd:import,
     *                        xsi:schemaLocation, xsl:import-schema) there is only one URI in this list. With an XQuery "import module"
     *                        declaration, however, a list of URIs may be specified. The StandardSchemaResolver returns
     *                        one Source object for each location, using the URIResolver registered with the configuration
     *                        to interpret the location hint.
     * @return an array of Source objects each identifying a schema document to be loaded.
     *         These need not necessarily correspond one-to-one with the location hints provided. Any element of the returned
     *         array that is null is ignored.
     * @throws net.sf.saxon.trans.XPathException
     *          if the module cannot be located, and if delegation to the default
     *          module resolver is not required.
     */

    public Source[] resolve(String targetNamespace, String baseURI, String[] locations) throws XPathException {
        if (config == null) {
            throw new NullPointerException("No configuration supplied to schema URI resolver");
        }
        if (config.isSchemaAvailable(targetNamespace) && !config.getConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS)) {
            return EMPTY_SOURCE_ARRAY;
        }
        if (NamespaceConstant.XLINK.equals(targetNamespace)) {
            // Use a local copy of the XLink schema rather than fetching it from the W3C site
            InputSource is = StandardEntityResolver.fetch("w3c/xlink/xlink.xsd", config);
            return new Source[]{new SAXSource(is)};
        } else if (NamespaceConstant.FN.equals(targetNamespace)) {
            // Use a local copy of the schema rather than fetching it from the W3C site
            InputSource is = StandardEntityResolver.fetch("xpath-functions.xsd", config);
            return new Source[]{new SAXSource(is)};
        } else {
            try {
                Source[] sources = new Source[locations.length];
                for (int i = 0; i < locations.length; i++) {

                    // Now try a JAXP URIResolver if available

                    if (uriResolver == null) {
                        uriResolver = config.getURIResolver();
                    }
                    if (uriResolver != null) {
                        sources[i] = uriResolver.resolve(locations[i], baseURI);
                    }

                    // if a user URI resolver returns null, try the standard one
                    // (Note, the standard URI resolver never returns null)
                    if (sources[i] == null) {
                        sources[i] = config.getSystemURIResolver().resolve(locations[i], baseURI);
                    }
                }
                return sources;
            } catch (TransformerException e) {
                throw XPathException.makeXPathException(e);
            }
        }
    }

}


// Copyright (c) 2006-2012 Saxonica Limited.