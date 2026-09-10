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
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;

/**
 * The default schema resolver used when no other is requested
 */
public class StandardSchemaResolver implements SchemaURIResolver {
    public static final String SCHEMA_NATURE = "http://www.w3.org/2001/XMLSchema ";
    public static final String VALIDATION_PURPOSE = "http://www.rddl.org/purposes#validation";

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
        if (NamespaceConstant.FN.equals(targetNamespace)) {
            // Use a local copy of the schema rather than fetching it from the W3C site
            return new Source[]{fetchResource("xpath-functions.scm", config)};
        }

        // The previous version of this method explicitly mapped the XLINK namespace to
        // a local resource. We can rely on the catalog resolver to do the mapping,
        // so all we have to do is make sure the right public URI is in place. We could
        // easily do this mapping for several common APIs, but perhaps that should
        // be in the entity resolver. For now, just continue to map this one for
        // backwards compatibility.
        if (NamespaceConstant.XLINK.equals(targetNamespace)) {
            locations = new String[]{"https://www.w3.org/1999/xlink.xsd"};
        }

        //#if CSHARP==false
        if ((locations == null || locations.length == 0) && targetNamespace != null) {
            Source source = resolvedNamespaceURI(targetNamespace, baseURI);
            if (source != null) {
                return new Source[]{source};
            }
        }
        //#endif

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

    private Source resolveLocationHint(String baseURI, String location) throws TransformerException {
        Source s;

        if (config.getURIResolver() != null) {
            // If the user has installed an explicit URI resolver, try using it first
            URIResolver uresolver = config.getURIResolver();
            s = uresolver.resolve(location, baseURI);
            if (s != null) {
                return s;
            }
        }

        //#if CSHARP==true
        s = config.getURIResolver().resolve(location, baseURI);
        //#endif
        //#if CSHARP==false
        s = config.getCommonResourceResolver().resolve(location, baseURI);
        //#endif
        // if a user URI resolver returns null, try the standard one
        // (Note, the standard URI resolver never returns null)
        if (s == null) {
            s = config.getSystemURIResolver().resolve(location, baseURI);
        }
        return s;
    }

    //#if CSHARP==false
    private Source resolvedNamespaceURI(String moduleURI, String baseURI) {
        Source source = null;
        try {
            if (config != null) {
                source = config.getCommonResourceResolver().resolveNamespace(moduleURI, SCHEMA_NATURE, VALIDATION_PURPOSE);
                if (source == null && baseURI != null) {
                    String resuri = new URI(baseURI).resolve(moduleURI).toString();
                    source = config.getCommonResourceResolver().resolveNamespace(resuri, SCHEMA_NATURE, VALIDATION_PURPOSE);
                }
            }
        } catch (TransformerException | URISyntaxException ex) {
            // nevermind
        }
        return source;
    }
    //#endif

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