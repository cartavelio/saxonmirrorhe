package com.saxonica.ee.jaxp;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.event.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NoElementsSpaceStrippingRule;
import net.sf.saxon.type.SchemaException;
import org.xml.sax.*;

import javax.xml.transform.stream.StreamSource;
import java.io.IOException;

/**
 * This class is a Saxon implementation of XMLReader that performs schema validation using
 * Saxon's schema processor.
 * <p>This class is supplied for the benefit of applications that expect to perform validation
 * by creating a validating SAX parser; it allows such applications to take advantage of the Saxon
 * validator with no change to the application.</p>
 */
public class ValidatingReader implements XMLReader {

    private final EnterpriseConfiguration config;
    private final XMLReader parser;
    private ContentHandler contentHandler;

    /**
     * Create a ValidatingReader. When this option is used, the only way to supply the
     * schema to be used for validation is through the xsi:schemaLocation attribute in the source
     * document.
     */

    public ValidatingReader() {
        this(new EnterpriseConfiguration());
        //config.getParseOptions().setContinueAfterValidationErrors(true);
    }

    /**
     * Create a ValidatingReader that uses a specified Saxon Configuration.
     *
     * @param config the Saxon Configuration. The Configuration
     *               can be pre-initialized with configuration options; it can also contain a pre-loaded cache
     *               of schema components to be used for validation
     */

    public ValidatingReader(EnterpriseConfiguration config) {
        this(config, config.getSourceParser());
    }

    /**
     * Create a ValidatingReader based on a supplied Saxon Configuration and a supplied
     * (presumably non-validating) XMLReader to perform the underlying XML parsing.
     *
     * @param config the Saxon Configuration. The Configuration
     *               can be pre-initialized with configuration options; it can also contain a pre-loaded cache
     *               of schema components to be used for validation
     * @param parser The XML parser to be used
     */

    public ValidatingReader(EnterpriseConfiguration config, XMLReader parser) {
        this.config = config;
        this.parser = parser;

    }

    /**
     * Look up the value of a feature flag.
     *
     * @param name The names that are recognized are the union of (a) the flags recognized by the underlying SAX XMLReader,
     *             and (b) the properties of the Saxon Configuration that have boolean values. A feature name starting with
     *             "http://saxon.sf.net" is assumed to be in the second category; these features are listed in
     *             {@link net.sf.saxon.lib.FeatureKeys}.
     * @return the value of the feature if the name is recognized
     * @throws SAXNotRecognizedException if the feature name is not recognized
     */
    @Override
    public boolean getFeature(String name) throws SAXNotRecognizedException, SAXNotSupportedException {
        if (name.startsWith("http://saxon.sf.net/")) {
            try {
                Object result = config.getConfigurationProperty(name);
                if (result instanceof Boolean) {
                    return (Boolean) result;
                } else {
                    throw new SAXNotRecognizedException("Feature name " + name + " is recognized, but is not boolean");
                }
            } catch (IllegalArgumentException e) {
                throw new SAXNotRecognizedException(e.getMessage());
            }
        } else {
            return parser.getFeature(name);
        }
    }

    /**
     * Set the value of a feature flag.
     *
     * @param name  The names that are recognized are the union of (a) the flags recognized by the underlying SAX XMLReader,
     *              and (b) the properties of the Saxon Configuration that have boolean values. A feature name starting with
     *              "http://saxon.sf.net" is assumed to be in the second category; these features are listed in
     *              {@link net.sf.saxon.lib.FeatureKeys}.
     * @param value true or false
     * @throws SAXNotRecognizedException if the feature name is not recognized, or if it is the name of a
     *                                   Saxon configuration property whose value is not boolean
     */
    @Override
    public void setFeature(String name, boolean value) throws SAXNotRecognizedException, SAXNotSupportedException {
        //System.err.println("Set feature " + name + " = " + value);
        if (name.startsWith("http://saxon.sf.net/")) {
            try {
                config.setBooleanProperty(name, value);
            } catch (IllegalArgumentException err) {
                throw new SAXNotRecognizedException(err.getMessage());
            }
        } else if (name.equals("http://xml.org/sax/features/validation")) {
            // ignore this option. The ant xmlvalidate task sets it, causing failure if there's no DTD.
        } else if (name.equals("http://apache.org/xml/features/validation/schema")) {
            // ignore this option. The ant schemavalidate task sets it, to tell Xerces to do schema validation
        } else if (name.equals("http://apache.org/xml/features/validation/schema-full-checking")) {
            // ignore this option. The ant schemavalidate task sets it, to tell Xerces to do full schema checking
        } else if (name.equals("http://apache.org/xml/features/disallow-doctype-decl")) {
            // ignore this option. The ant schemavalidate task sets it, to tell Xerces to disallow a DTD DOCTYPE
        } else {
            parser.setFeature(name, value);
        }
    }

    /**
     * Look up the value of a property.
     *
     * @param name The names that are recognized are the union of (a) the properties recognized by the underlying SAX XMLReader,
     *             and (b) the properties of the Saxon Configuration. A feature name starting with
     *             "http://saxon.sf.net" is assumed to be in the second category; these features are listed in
     *             {@link net.sf.saxon.lib.FeatureKeys}.
     * @return the value of the property if the name is recognized
     * @throws SAXNotRecognizedException if the property name is not recognized
     */
    @Override
    public Object getProperty(String name) throws SAXNotRecognizedException, SAXNotSupportedException {
        if (name.startsWith("http://saxon.sf.net/")) {
            try {
                return config.getConfigurationProperty(name);
            } catch (IllegalArgumentException err) {
                throw new SAXNotRecognizedException(err.getMessage());
            }
        } else {
            return parser.getProperty(name);
        }
    }

    /**
     * Set the value of a property.
     *
     * @param name  The names that are recognized are the union of
     *              (a) the properties recognized by the underlying SAX XMLReader,
     *              and (b) the properties of the Saxon Configuration. A feature name starting with
     *              "http://saxon.sf.net" is assumed to be in the second category; these features are listed in
     *              {@link net.sf.saxon.lib.FeatureKeys}.
     *              <p>Saxon accepts the Apache-defined properties
     *              <code>http://apache.org/xml/properties/schema/external-noNamespaceSchemaLocation</code> and
     *              <code>http://apache.org/xml/properties/schema/external-schemaLocation</code> to define the
     *              location of schema documents. These properties may be set repeatedly to load more than one
     *              schema document.</p>
     *              <p><b>To enable schema validation using XSD 1.1, the property {@link net.sf.saxon.lib.FeatureKeys#XSD_VERSION}
     *              must be set to the value "1.1".</b></p>
     * @param value the value of the property
     * @throws SAXNotRecognizedException if the property name is not recognized, or if the value is inappropriate
     */
    @Override
    public void setProperty(String name, Object value) throws SAXNotRecognizedException, SAXNotSupportedException {
        //System.err.println("Set property " + name + " = " + value);
        if (name.startsWith("http://saxon.sf.net/")) {
            try {
                config.setConfigurationProperty(name, value);
            } catch (IllegalArgumentException err) {
                throw new SAXNotRecognizedException(err.getMessage());
            }
        } else if (name.equals("http://apache.org/xml/properties/schema/external-noNamespaceSchemaLocation")) {
            try {
                config.addSchemaSource(new StreamSource((String) value));
            } catch (SchemaException err) {
                throw new SAXNotSupportedException("Error in no-namespace schema document: " + err.getMessage());
            }
        } else if (name.equals("http://apache.org/xml/properties/schema/external-schemaLocation")) {
            int space = value.toString().indexOf(' ');
            if (space <= 0) {
                throw new SAXNotSupportedException("Value of property " + name + " contains no space. " +
                        "Value should contain namespace URI and schema location URI, space-separated.");
            }
            String locationURI = value.toString().substring(space + 1);
            try {
                config.addSchemaSource(new StreamSource(locationURI));
            } catch (SchemaException err) {
                throw new SAXNotSupportedException("Error in schema document: " + err.getMessage());
            }
        } else {
            parser.setProperty(name, value);
        }

    }

    /**
     * Allow an application to register an entity resolver.
     * <p>If the application does not register an entity resolver,
     * the XMLReader will perform its own default resolution.</p>
     * <p>Applications may register a new or different resolver in the
     * middle of a parse, and the SAX parser must begin using the new
     * resolver immediately.</p>
     *
     * @param resolver The entity resolver.
     * @see #getEntityResolver
     */
    @Override
    public void setEntityResolver(EntityResolver resolver) {
        parser.setEntityResolver(resolver);
    }

    /**
     * Return the current entity resolver.
     *
     * @return The current entity resolver, or null if none
     *         has been registered.
     * @see #setEntityResolver
     */
    @Override
    public EntityResolver getEntityResolver() {
        return parser.getEntityResolver();
    }

    /**
     * Allow an application to register a DTD event handler.
     * <p>If the application does not register a DTD handler, all DTD
     * events reported by the SAX parser will be silently ignored.</p>
     * <p>Applications may register a new or different handler in the
     * middle of a parse, and the SAX parser must begin using the new
     * handler immediately.</p>
     *
     * @param handler The DTD handler.
     * @see #getDTDHandler
     */
    @Override
    public void setDTDHandler(DTDHandler handler) {
        parser.setDTDHandler(handler);
    }

    /**
     * Return the current DTD handler.
     *
     * @return The current DTD handler, or null if none
     *         has been registered.
     * @see #setDTDHandler
     */
    @Override
    public DTDHandler getDTDHandler() {
        return parser.getDTDHandler();
    }

    /**
     * Allow an application to register a content event handler.
     * <p>If the application does not register a content handler, all
     * content events reported by the SAX parser will be silently
     * ignored.</p>
     * <p>Applications may register a new or different handler in the
     * middle of a parse, and the SAX parser must begin using the new
     * handler immediately.</p>
     *
     * @param handler The content handler.
     * @see #getContentHandler
     */
    @Override
    public void setContentHandler(ContentHandler handler) {
        this.contentHandler = handler;
    }

    /**
     * Return the current content handler.
     *
     * @return The current content handler, or null if none
     *         has been registered.
     * @see #setContentHandler
     */
    @Override
    public ContentHandler getContentHandler() {
        return contentHandler;
    }

    /**
     * Allow an application to register an error event handler.
     * <p>If the application does not register an error handler, all
     * error events reported by the SAX parser will be silently
     * ignored; however, normal processing may not continue.  It is
     * highly recommended that all SAX applications implement an
     * error handler to avoid unexpected bugs.</p>
     * <p>Applications may register a new or different handler in the
     * middle of a parse, and the SAX parser must begin using the new
     * handler immediately.</p>
     *
     * @param handler The error handler.
     * @see #getErrorHandler
     */
    @Override
    public void setErrorHandler(ErrorHandler handler) {
        parser.setErrorHandler(handler);
    }

    /**
     * Return the current error handler.
     *
     * @return The current error handler, or null if none
     *         has been registered.
     * @see #setErrorHandler
     */
    @Override
    public ErrorHandler getErrorHandler() {
        return parser.getErrorHandler();
    }

    /**
     * Get the Saxon Configuration object used by this Validating Reader (allowing it to be
     * shared by another ValidatingReader)
     * @return the Saxon Configuration object
     */

    public EnterpriseConfiguration getConfiguration() {
        return config;
    }

    /**
     * Parse an XML document.
     * <p>The application can use this method to component the XML
     * reader to begin parsing an XML document from any valid input
     * source (a character stream, a byte stream, or a URI).</p>
     * <p>Applications may not invoke this method while a parse is in
     * progress (they should create a new XMLReader instead for each
     * nested XML document).  Once a parse is complete, an
     * application may reuse the same XMLReader object, possibly with a
     * different input source.
     * Configuration of the XMLReader object (such as handler bindings and
     * values established for feature flags and properties) is unchanged
     * by completion of a parse, unless the definition of that aspect of
     * the configuration explicitly specifies other behavior.
     * (For example, feature flags or properties exposing
     * characteristics of the document being parsed.)</p>
     * <p>During the parse, the XMLReader will provide information
     * about the XML document through the registered event
     * handlers.</p>
     * <p>This method is synchronous: it will not return until parsing
     * has ended.  If a client application wants to terminate
     * parsing early, it should throw an exception.</p>
     *
     * @param input The input source for the top-level of the
     *              XML document.
     * @throws org.xml.sax.SAXException Any SAX exception, possibly
     *                                  wrapping another exception.
     * @throws java.io.IOException      An IO exception from the parser,
     *                                  possibly from a byte stream or character stream
     *                                  supplied by the application.
     * @see org.xml.sax.InputSource
     * @see #parse(String)
     * @see #setEntityResolver
     * @see #setDTDHandler
     * @see #setContentHandler
     * @see #setErrorHandler
     */
    @Override
    public void parse(InputSource input) throws IOException, SAXException {
        setValidator(input.getSystemId());
        parser.parse(input);
    }

    /**
     * Parse an XML document from a system identifier (URI).
     * <p>This method is a shortcut for the common case of reading a
     * document from a system identifier.  It is the exact
     * equivalent of the following:</p>
     * <pre>
     * parse(new InputSource(systemId));
     * </pre>
     * <p>If the system identifier is a URL, it must be fully resolved
     * by the application before it is passed to the parser.</p>
     *
     * @param systemId The system identifier (URI).
     * @throws org.xml.sax.SAXException Any SAX exception, possibly
     *                                  wrapping another exception.
     * @throws java.io.IOException      An IO exception from the parser,
     *                                  possibly from a byte stream or character stream
     *                                  supplied by the application.
     * @see #parse(org.xml.sax.InputSource)
     */
    @Override
    public void parse(String systemId) throws IOException, SAXException {
        setValidator(systemId);
        parser.parse(systemId);
    }

    private void setValidator(String systemId) {
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        pipe.setErrorReporter(new SchemaFactoryImpl.ErrorReporterWrappingErrorHandler(parser.getErrorHandler()));

        Receiver output;
        if (contentHandler == null) {
            output = new Sink(pipe);
        } else {
            ContentHandlerProxy chp = new ContentHandlerProxy();
            chp.setUnderlyingContentHandler(contentHandler);
            chp.setPipelineConfiguration(pipe);
            output = chp;
        }

        ParseOptions options = pipe.getParseOptions();
        options.setCheckEntityReferences(true);
        options.setSchemaValidationMode(config.getSchemaValidationMode() == Validation.LAX ? Validation.LAX : Validation.STRICT);
        options.setTopLevelElement(config.getParseOptions().getTopLevelElement());
        options.setTopLevelType(config.getParseOptions().getTopLevelType());
        options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());

        Receiver receiver = config.getDocumentValidator(output, systemId, options, null);

        ReceivingContentHandler rch = new ReceivingContentHandler();
        rch.setPipelineConfiguration(output.getPipelineConfiguration());
        rch.setReceiver(receiver);
        parser.setContentHandler(rch);
    }
}

//
// Copyright (c) 2010-2022 Saxonica Limited

