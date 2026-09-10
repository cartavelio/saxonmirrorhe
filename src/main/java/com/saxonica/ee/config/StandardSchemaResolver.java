package com.saxonica.ee.config;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.net.URISyntaxException;
import java.util.ArrayList;

/**
 * The default schema resolver used when no other is requested
 */
public class StandardSchemaResolver implements SchemaURIResolver {
    private EnterpriseConfiguration config;
    private static final StreamSource[] EMPTY_SOURCE_ARRAY = new StreamSource[0];
    private ResourceResolver resourceResolver;

//    /**
//     * Default constructor. Must be followed by a call of setConfiguration()
//     */
//
//    public StandardSchemaResolver() {
//    }

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
     * Set the ResourceResolver to be used. Defaults to the ResourceResolver associated with the {@link Configuration}
     * @param resolver the ResourceResolver to be used
     */

    @CSharpModifiers(code={"internal"})
    public void setResourceResolver(ResourceResolver resolver) {
        this.resourceResolver = resolver;
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
                config.isSchemaAvailable(NamespaceUri.of(targetNamespace)) &&
                !config.getConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS)) {
            return EMPTY_SOURCE_ARRAY;
        }
        if (NamespaceConstant.FN.equals(targetNamespace)) {
            // Use a local copy of the schema rather than fetching it from the W3C site
            return new Source[]{fetchResource("xpath-functions.scm", config)};
        }

        // For the XLink namespace, ignore any location hint pointing to a local copy; but
        // the request can be redirected using a resolver or a catalog. See bug #5652.
        if (NamespaceConstant.XLINK.equals(targetNamespace)) {
            locations = null;
        }

        if ((locations == null || locations.length == 0) && targetNamespace != null) {
            Source source = resolveNamespaceURI(targetNamespace, baseURI);
            if (source != null) {
                return new Source[]{source};
            }
        }

        if (locations == null || locations.length == 0) {
            return EMPTY_SOURCE_ARRAY;
        }

        try {
            Source[] sources = new Source[locations.length];
            for (int i = 0; i < locations.length; i++) {
                sources[i] = resolveLocationHint(baseURI, locations[i]);
            }
            return sources;
        } catch (TransformerException e) {
            throw XPathException.makeXPathException(e);
        }
    }

    private Source resolveLocationHint(String baseURI, String location) throws XPathException {

        ResourceRequest rr = new ResourceRequest();
        rr.relativeUri = location;
        rr.baseUri = baseURI;
        rr.nature = ResourceRequest.XSD_NATURE;
        rr.purpose = ResourceRequest.VALIDATION_PURPOSE;
        try {
            rr.uri = ResolveURI.makeAbsolute(location, baseURI).toString();
        } catch (URISyntaxException err) {
            // ignore this location hint if invalid
            throw new XPathException("Unable to reserve schemaLocation URI " + location, err);
        }

        return rr.resolve(config.getResourceResolver(), new DirectResourceResolver(config));
    }

    private Source resolveNamespaceURI(String moduleURI, String baseURI) {
        ResourceRequest rr = new ResourceRequest();
        rr.uri = moduleURI;
        rr.baseUri = baseURI;
        rr.uriIsNamespace = true;
        rr.nature = ResourceRequest.XSD_NATURE;
        rr.purpose = ResourceRequest.VALIDATION_PURPOSE;
        try {
            return rr.resolve(config.getResourceResolver());
        } catch (TransformerException ex) {
            // nevermind
        }
        return null;
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

// Copyright (c) 2006-2023 Saxonica Limited