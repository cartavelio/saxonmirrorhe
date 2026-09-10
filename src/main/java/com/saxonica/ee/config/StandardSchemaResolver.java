package com.saxonica.ee.config;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.util.ArrayList;

/**
 * The default schema resolver used when no other is requested
 */
public class StandardSchemaResolver implements SchemaURIResolver {

    private EnterpriseConfiguration config;
    private static final StreamSource[] EMPTY_SOURCE_ARRAY = new StreamSource[0];
    private URIResolver uriResolver;

    /**
     * Default constructor. Must be followed by a call of setConfiguration()
     */

    public StandardSchemaResolver() {
    }

    /**
     * Create a StandardSchemaResolver and set the Configuration
     *
     * @param config the Configuration, which must be
     */

    public StandardSchemaResolver(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Set the configuration information for use by the resolver
     *
     * @param config the Saxon Configuration
     */

    @Override
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
     * Set the URIResolver to be used. Defaults to the URIResolver associated with the {@link Configuration}
     * @param resolver the URIResolver to be used
     */

    public void setURIResolver(URIResolver resolver) {
        this.uriResolver = resolver;
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

    @Override
    public Source[] resolve(String targetNamespace, String baseURI, String[] locations) throws XPathException {
        if (config == null) {
            throw new NullPointerException("No configuration supplied to schema URI resolver");
        }
        if (targetNamespace != null &&
                config.isSchemaAvailable(targetNamespace) &&
                !config.getConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS)) {
            return EMPTY_SOURCE_ARRAY;
        }
        if (NamespaceConstant.XLINK.equals(targetNamespace)) {
            // Use a local copy of the XLink schema rather than fetching it from the W3C site
            return new Source[]{fetchResource("w3c/xlink/xlink.xsd", config)};
        } else if (NamespaceConstant.FN.equals(targetNamespace)) {
            // Use a local copy of the schema rather than fetching it from the W3C site
            return new Source[]{fetchResource("xpath-functions.scm", config)};
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

    public static Source fetchResource(String pathName, Configuration config) {
        InputStream inputStream = Version.platform.locateResource(pathName, new ArrayList<>());
        if (inputStream == null) {
            throw new AssertionError("Failed to load " + pathName + " from the Saxon library");
        }
        if (config.isTiming()) {
            config.getLogger().info("Loading internal schema: " + pathName);
        }
        return new StreamSource(inputStream);
    }

}


// Copyright (c) 2006-2020 Saxonica Limited