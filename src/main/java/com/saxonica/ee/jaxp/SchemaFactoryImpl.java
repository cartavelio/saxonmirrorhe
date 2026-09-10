package com.saxonica.ee.jaxp;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.sdoc.LSSchemaResolver;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.*;
import net.sf.saxon.s9api.XmlProcessingError;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.type.SchemaException;
import org.w3c.dom.ls.LSResourceResolver;
import org.xml.sax.*;

import javax.xml.XMLConstants;
import javax.xml.transform.Source;
import javax.xml.transform.SourceLocator;
import javax.xml.transform.TransformerException;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import java.util.function.Supplier;

/**
 * This is the Saxon implementation of the JAXP 1.3 SchemaFactory interface.
 */

public class SchemaFactoryImpl extends SchemaFactory implements Configuration.ApiProvider {

    private static final String XSD_LANGUAGE_IDENTIFIER = "http://www.w3.org/XML/XMLSchema";
    private static final String XSD10_LANGUAGE_IDENTIFIER = "http://www.w3.org/XML/XMLSchema/v1.0";
    private static final String XSD11_LANGUAGE_IDENTIFIER = "http://www.w3.org/XML/XMLSchema/v1.1";

    /**
     * A mutable static variable (horrible but useful) allowing the use of this SchemaFactory to
     * be disabled. If the value of this variable is set to false, then when the JAXP classpath
     * search calls {@link #isSchemaLanguageSupported(String)}, the response will be false,
     * meaning that Saxon does not get used as the Schema validation provider even though it
     * is present on the classpath.
     */

    public static boolean ENABLED = true;

    /**
     * A static callback hook for instantiating the Configuration. This is useful if the Configuration
     * is to be loaded from a configuration file, or if it requires custom initialization, for example
     * supplying an OEM license key
     */

    private static Supplier<EnterpriseConfiguration> configurationFactory =
            CSharp.methodRef(EnterpriseConfiguration::new);

    static ErrorHandler DRACONIAN_ERROR_HANDLER = new ErrorHandler() {
        @Override
        public void warning(SAXParseException exception) throws SAXException {
            // no action
        }

        @Override
        public void error(SAXParseException exception) throws SAXException {
            throw exception;
        }

        @Override
        public void fatalError(SAXParseException exception) throws SAXException {
            throw exception;
        }
    };

    private final EnterpriseConfiguration config;
    private ErrorReporter errorReporter;

    /**
     * Default constructor. This is designed to be invoked via the JAXP SchemaFactory.newInstance()
     * mechanism, but it can also be invoked directly if the application will always be using the Saxon
     * implementation. Instantiating a SchemaFactory directly in this way is likely to be a lot faster
     * than the JAXP mechanism, because it avoids the expensive search of all JARs on the classpath.
     * <p>This constructor creates an {@code EnterpriseConfiguration}: to do so it calls the registered
     * Configuration factory. The Configuration factory can be customized using the (static) method
     * {@link #setConfigurationFactory(Supplier)}.</p>
     * <p>The constructor does not at this stage check that a license key is installed.</p>
     */

    public SchemaFactoryImpl() {
        config = configurationFactory.get();
        config.setProcessor(this);
        setErrorHandler(DRACONIAN_ERROR_HANDLER);
    }

    /**
     * Constructor supplying an EnterpriseConfiguration
     *
     * @param config The EnterpriseConfiguration. The method verifies that this is usable by checking that
     *               a license key is available.
     */

    public SchemaFactoryImpl(EnterpriseConfiguration config) {
        this.config = config;
        config.checkLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
        errorReporter = config.makeErrorReporter();
    }

    /**
     * Static customization method to register a factory method for instantiating the
     * {@link EnterpriseConfiguration}. This callback can be used to initialize a configuration (for example,
     * by reading it from a configuration file, or by setting configuration options and/or a license
     * key)
     * @param factory the class that will be called to instantiate or initialize the configuration,
     *                assuming the class is instantiated implicitly as a consequence of the JAXP search mechanism.
     *                (Despite the name, the callback is not required to create a new {@link EnterpriseConfiguration}.
     *                It can equally well return an existing one.)
     */

    public static void setConfigurationFactory(Supplier<EnterpriseConfiguration> factory) {
        configurationFactory = factory;
    }

    /**
     * Tests whether a particular schema language is supported.
     *
     * @param language URI identifying the language (see JAXP specifications for details).
     * @return true if the requested language is XML Schema (XMLConstants.W3C_XML_SCHEMA_NS_URI),
     * (or the language versions {@link #XSD10_LANGUAGE_IDENTIFIER} or
     * {@link #XSD11_LANGUAGE_IDENTIFIER}; otherwise false.
     * <p>Always returns false if the static variable {#ENABLED} is set to false.</p>
     * <p>Always returns false if no Saxon Enterprise license can be found.</p>
     */

    @Override
    public boolean isSchemaLanguageSupported(String language) {
        if (!ENABLED) {
            return false;
        }
        boolean b = isSchemaLanguageSupportedX(language);
        if (System.getProperty("jaxp.debug") != null) {
            config.getLogger().info(
                    "Calling com.saxonica.SchemaFactoryImpl.isSchemaLanguageSupported(\"" + language + "\"); language supported? " + b);
        }
        if (b) {
            boolean licensed = config.isLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION);
            if (System.getProperty("jaxp.debug") != null) {
                config.getLogger().info(
                        "Is Saxon-EE schema validation licensed? " + licensed);
            }
            return licensed;
        } else {
            return false;
        }
    }


    private boolean isSchemaLanguageSupportedX(String language) {
        if (language.equals(XSD11_LANGUAGE_IDENTIFIER)) {
            config.setConfigurationProperty(FeatureKeys.XSD_VERSION, "1.1");
            return true;
        }
        if (language.equals(XSD10_LANGUAGE_IDENTIFIER)) {
            config.setConfigurationProperty(FeatureKeys.XSD_VERSION, "1.0");
            return true;
        }
        return language.equals(XMLConstants.W3C_XML_SCHEMA_NS_URI) ||
                language.equals(XSD_LANGUAGE_IDENTIFIER);
    }

    /**
     * Set an error handler to which all schema processing
     * errors will be notified. May be null, in which case all schema processing errors are fatal.
     * The error handler is NOT inherited by instances of Validator or ValidatorHandler created
     * using this factory.
     *
     * @param errorHandler The requested error handler. Note: unlike releases prior to Saxon 9.6,
     * this does not change the error listener used by the Configuration.
     */

    @Override
    public void setErrorHandler(ErrorHandler errorHandler) {
        if (errorHandler == null) {
            errorHandler = DRACONIAN_ERROR_HANDLER;
        }
        errorReporter = new ErrorReporterWrappingErrorHandler(errorHandler);
    }

    /**
     * Get the error handler in use
     *
     * @return the error handler previously registered using {@link #setErrorHandler} if available; otherwise
     *         null.
     */

    @Override
    public ErrorHandler getErrorHandler() {
        if (errorReporter instanceof ErrorReporterWrappingErrorHandler) {
            ErrorHandler eh = ((ErrorReporterWrappingErrorHandler)errorReporter).getErrorHandler();
            return eh == DRACONIAN_ERROR_HANDLER ? null : eh;
        }
        return null;
    }

    /**
     * Set a resolver to be used for dereferencing URIs encountered during schema processing and validation.
     *
     * @param lsResourceResolver The resolver to be used. This is wrapped in a URIResolver and registered
     *                           with the Configuration, overwriting any URIResolver or SchemaURIResolver that was previously
     *                           registered with the Configuration. Equally, any subsequent call on setURIResolver() or
     *                           setSchemaURIResolver() on the Configuration object will replace the resolver registered using
     *                           this method.
     */

    @Override
    public void setResourceResolver(LSResourceResolver lsResourceResolver) {
        config.setResourceResolver(new ResourceResolverWrappingLSResourceResolver(lsResourceResolver));
        config.setSchemaURIResolver(new LSSchemaResolver(lsResourceResolver, config));
    }

    /**
     * Get the resolver used for dereferencing URIs encountered during schema processing and validation.
     *
     * @return the resolver in use. If no resolver has been registered, or if the resolver registered with
     *         the Configuration is not a wrapped LSResourceResolver, return null.
     */

    /*@Nullable*/
    @Override
    public LSResourceResolver getResourceResolver() {
        ResourceResolver resolver = config.getResourceResolver();
        if (resolver instanceof ResourceResolverWrappingLSResourceResolver) {
            return ((ResourceResolverWrappingLSResourceResolver) resolver).getLSResourceResolver();
        } else {
            return null;
        }
    }

    /**
     * Parses the specified source(s) as a schema and returns it as a schema.
     * <p>The callee will read all the {@link Source}s and combine them into a
     * single schema. The exact semantics of the combination depends on the schema
     * language that this {@link SchemaFactory} object is created for.</p>
     * <p>When an {@link ErrorHandler} is set, the callee will report all the errors
     * found in sources to the handler. If the handler throws an exception, it will
     * abort the schema compilation and the same exception will be thrown from
     * this method. Also, after an error is reported to a handler, the callee is allowed
     * to abort the further processing by throwing it. If an error handler is not set,
     * the callee will throw the first error it finds in the sources.</p>
     * <h2>W3C XML Schema 1.0</h2>
     * <p>The resulting schema contains components from the specified sources.
     * The same result would be achieved if all these sources were
     * imported, using appropriate values for schemaLocation and namespace,
     * into a single schema document with a different targetNamespace
     * and no components of its own, if the import elements were given
     * in the same order as the sources.  Section 4.2.3 of the XML Schema
     * recommendation describes the options processors have in this
     * regard.  While a processor should be consistent in its treatment of
     * JAXP schema sources and XML Schema imports, the behaviour between
     * JAXP-compliant parsers may vary; in particular, parsers may choose
     * to ignore all but the first &lt;import&gt; for a given namespace,
     * regardless of information provided in schemaLocation.</p>
     * <p>If the parsed set of schemas includes error(s) as
     * specified in the section 5.1 of the XML Schema spec, then
     * the error must be reported to the {@link ErrorHandler}.</p>
     * <h2>RELAX NG</h2>
     * <p>The Saxon implementation does not support RELAX NG.</p>
     *
     * @param sources inputs to be parsed as schema documents. The Saxon implementation recognizes not only
     *                the standard {@link javax.xml.transform.sax.SAXSource},
     *                {@link javax.xml.transform.stream.StreamSource}, and {@link javax.xml.transform.dom.DOMSource},
     *                but also any implementation of {@link net.sf.saxon.om.NodeInfo}
     * @return Always return a non-null valid {@link Schema} object.
     *         Note that when an error has been reported, there is no
     *         guarantee that the returned {@link Schema} object is
     *         meaningful.
     * @throws SAXException             If an error is found during processing the specified inputs.
     *                                  When an {@link ErrorHandler} is set, errors are reported to
     *                                  there first. See {@link #setErrorHandler(ErrorHandler)}. In the Saxon
     *                                  implementation, if a SAXException is thrown, it will always wrap
     *                                  an instance of {@link SchemaException}
     * @throws NullPointerException     If the <code>schemas</code> parameter itself is null or
     *                                  any item in the array is null.
     * @throws IllegalArgumentException If any item in the array is not recognized by this method.
     */

    @Override
    public Schema newSchema(Source[] sources) throws SAXException {
        SchemaCompiler compiler = config.newSchemaCompiler();
        try {
            for (Source source : sources) {
                config.addSchemaSource(source, errorReporter, compiler);
            }
        } catch (SchemaException e) {
            throw toSAXException(e);
        }
        return new SchemaReference(config.getSuperSchema());
    }

    /**
     * Returns the {@link Schema} object containing all the schema components
     * that have been loaded using the {@link #newSchema} method. If no schema
     * components have been loaded, the schema can still be used to validate
     * source documents provided that they specify the location of the required
     * schema components using the xsi:schemaLocation attribute.
     * <p>The returned {@link Schema} object assumes that if documents
     * refer to the same URL in the schema location hints,
     * they will always resolve to the same schema document. This
     * assumption allows implementations to reuse parsed results of
     * schema documents so that multiple validations against the same
     * schema will run faster.</p>
     * <p>Note that the use of schema location hints introduces a
     * vulnerability to denial-of-service attacks.</p>
     *
     * @return Always return a non-null valid {@link Schema} object.
     */

    @Override
    public Schema newSchema() {
        return new SchemaReference(config.getSuperSchema());
    }

    /**
     * Look up the value of a property.
     * <p>The property name is any fully-qualified URI.  It is
     * possible for a {@link javax.xml.validation.SchemaFactory} to recognize a property name but
     * temporarily be unable to return its value.</p>
     * <p>{@link javax.xml.validation.SchemaFactory}s are not required to recognize any specific
     * property names.</p>
     * <p>In the Saxon implementation, the property names that are recognized are the same as the
     * attribute names recognised by a {@link javax.xml.transform.TransformerFactory}.
     * These are listed as constant fields
     * in the class {@link net.sf.saxon.lib.FeatureKeys}</p>
     *
     * @param name The property name, which is a non-null fully-qualified URI.
     * @return The current value of the property.
     * @throws org.xml.sax.SAXNotSupportedException
     *                              When the
     *                              XMLReader recognizes the property name but
     *                              cannot determine its value at this time.
     * @throws NullPointerException if the name parameter is null.
     * @see #setProperty(String, Object)
     */

    @Override
    public Object getProperty(String name) throws SAXNotSupportedException {
        try {
            return config.getConfigurationProperty(name);
        } catch (IllegalArgumentException err) {
            throw new SAXNotSupportedException(err.getMessage());
        }
    }

    /**
     * Set the value of a property.
     * <p>The property name is any fully-qualified URI.  It is
     * possible for a {@link javax.xml.validation.SchemaFactory} to recognize a property name but
     * to be unable to change the current value.</p>
     * <p>In the Saxon implementation, the property names that are recognized are the same as the
     * attribute names recognised by a {@link javax.xml.transform.TransformerFactory}.
     * These are listed as constant fields
     * in the class {@link net.sf.saxon.lib.FeatureKeys}</p>
     *
     * @param name   The property name, which is a non-null fully-qualified URI.
     * @param object The requested value for the property.
     * @throws org.xml.sax.SAXNotRecognizedException
     *                              If the property
     *                              value can't be assigned or retrieved.
     * @throws org.xml.sax.SAXNotSupportedException
     *                              When the
     *                              {@link javax.xml.validation.SchemaFactory} recognizes the property name but
     *                              cannot set the requested value.
     * @throws NullPointerException if the name parameter is null.
     */
    @Override
    public void setProperty(String name, Object object) throws SAXNotRecognizedException, SAXNotSupportedException {
        switch (name) {
            case XMLConstants.ACCESS_EXTERNAL_SCHEMA:
                config.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, object.toString());
                return;
            default:
        }
        try {
            config.setConfigurationProperty(name, object);
        } catch (IllegalArgumentException err) {
            throw new SAXNotSupportedException(err.getMessage());
        }
    }

    /**
     * Look up the value of a feature flag.
     * <p>Saxon recognizes the feature name XMLConstants.FEATURE_SECURE_PROCESSING, and if this is requested,
     * it returns the value of the Configuration property named as the concatenation of
     * FeatureKeys.XML_PARSER_FEATURE + XMLConstants.FEATURE_SECURE_PROCESSING.</p>
     *
     * <p>In all other cases Saxon delegates to the superclass, which will generally result in a
     * {@code SAXNotRecognizedException}.</p>
     *
     * @param name The feature name, which is a non-null fully-qualified URI.
     * @return The current value of the feature (true or false).
     * @throws SAXNotRecognizedException If the feature
     *                                   value can't be assigned or retrieved.
     * @throws SAXNotSupportedException  When the
     *                                   {@link SchemaFactory} recognizes the feature name but
     *                                   cannot determine its value at this time.
     * @throws NullPointerException      If <code>name</code> is <code>null</code>.
     * @see #setFeature(String, boolean)
     */
    @Override
    public boolean getFeature(String name) throws SAXNotRecognizedException, SAXNotSupportedException {
        if (XMLConstants.FEATURE_SECURE_PROCESSING.equals(name)) {
            return (Boolean)getConfiguration().getConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + name);
        } else {
            return super.getFeature(name);
        }
    }

    /**
     * <p>Set a feature for this <code>SchemaFactory</code>.</p>
     * <p>The call is delegated to the superclass except when the feature is {@link XMLConstants#FEATURE_SECURE_PROCESSING} feature.
     * With this feature, the effect is to set the corresponding parser feature in the Configuration. (It thus affects all
     * operations under the control of that Configuration, not only schema processing). Setting a parser feature in the Configuration
     * affects all parsers created by that Configuration, but it does not affect the settings of a user-supplied parser, for example
     * an XMLReader supplied within a SAXSource object.</p>
     * <p>Note: in general, the way to set configuration options for Saxon is via the Configuration object, which can be obtained
     * using the {@link #getConfiguration()} method.</p>
     *
     * @param name  The feature name, which is a non-null fully-qualified URI.
     * @param value The requested value of the feature (true or false).
     * @throws SAXNotRecognizedException If the feature
     *                                   value can't be assigned or retrieved.
     * @throws SAXNotSupportedException  When the
     *                                   {@link SchemaFactory} recognizes the feature name but
     *                                   cannot set the requested value.
     * @throws NullPointerException      If <code>name</code> is <code>null</code>.
     * @see #getFeature(String)
     */
    @Override
    public void setFeature(String name, boolean value) throws SAXNotRecognizedException, SAXNotSupportedException {
        if (XMLConstants.FEATURE_SECURE_PROCESSING.equals(name)) {
            getConfiguration().setConfigurationProperty(FeatureKeys.XML_PARSER_FEATURE + name, value);
        } else {
            super.setFeature(name, value);
        }
    }

    /**
     * Get the underlying Saxon Configuration object
     *
     * @return the underlying Saxon Configuration object, which will always be an EnterpriseConfiguration
     */

    public EnterpriseConfiguration getConfiguration() {
        return config;
    }

    private static SAXParseException toSAXException(TransformerException err) {
        if (err.getException() instanceof SAXParseException) {
            return (SAXParseException) err.getException();
        } else {
            SourceLocator loc = err.getLocator();
            if (loc != null && !(loc instanceof Locator)) {
                loc = new Loc(loc);
            }
            return new SAXParseException(err.getMessage(), (Locator) loc, err);
        }
    }

    private static TransformerException toTransformerException(SAXException err) {
        if (err.getException() instanceof TransformerException) {
            return (TransformerException) err.getException();
        } else {
            return new TransformerException(err);
        }
    }


    /**
     * A JAXP ErrorReporter that wraps a SAX ErrorHandler
     */

    public static class ErrorReporterWrappingErrorHandler implements ErrorReporter {

        private final ErrorHandler handler;

        /**
         * Create an ErrorListener that wraps a supplied SAX ErrorHandler
         *
         * @param handler the SAX error handler to be wrapped
         */

        public ErrorReporterWrappingErrorHandler(ErrorHandler handler) {
            this.handler = handler;
        }

        @Override
        public void report(XmlProcessingError error) {
            final SAXParseException exception = toSAXException(XPathException.fromXmlProcessingError(error));
            try {
                if (error.isWarning()) {
                    handler.warning(exception);
                } else {
                    handler.error (exception);
                }
            } catch (SAXException e) {
                error.setTerminationMessage(e.getMessage());
            }
        }

        /**
         * Get the wrapped SAX ErrorHandler
         *
         * @return the wrapped SAX ErrorHandler
         */

        public ErrorHandler getErrorHandler() {
            return handler;
        }
    }
}

// Copyright (c) 2004-2023 Saxonica Limited
