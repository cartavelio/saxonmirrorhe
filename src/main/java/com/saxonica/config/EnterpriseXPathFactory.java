package com.saxonica.config;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XsdCompiler;
import net.sf.saxon.s9api.XsdSchema;
import net.sf.saxon.xpath.XPathFactoryImpl;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;

/**
 * An implementation of the JAXP XPath Factory that creates an enterprise configuration. This implementation
 * is used when Saxon-EE is present, and when the object model selected is the Saxon native object model.
 */
public class EnterpriseXPathFactory extends XPathFactoryImpl {

    private XsdSchema schema;

    /**
     * Default constructor: this creates a Configuration as well as creating the enterprise
     * XPathFactory. Any documents accessed using this XPathFactory must be built using this same Configuration.
     * The Configuration has validation mode set to Lax, so validation of input documents will be attempted,
     * provided that a suitable schema is available.
     */

    public EnterpriseXPathFactory() {
        processor = newProcessor();
        initConfiguration(processor.getUnderlyingConfiguration());
    }

    @Override
    protected Processor newProcessor() {
        return new Processor(true);
    }

    /**
     * Constructor using a user-supplied Configuration.
     * This constructor is useful if the document to be queried already exists, as it allows the configuration
     * associated with the document to be used with this XPathFactory.
     *
     * @param config the configuration to be used. This must be an EnterpriseConfiguration.
     * @throws ClassCastException if the supplied configuration is not schema-aware.
     */

    public EnterpriseXPathFactory(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Load a schema, which will be available for use by all stylesheets subsequently
     * compiled using this TransformerFactory.
     *
     * <p>If this method is called more than once, the resulting schema will be the union
     * of all schemas supplied. This union must be a valid schema in its own right. This
     * means, for example, that two schemas supplied via this interface must not contain
     * duplicate type definitions.</p>
     *
     * @param schemaSource the JAXP Source object identifying the schema document to be loaded
     * @throws TransformerException if the schema cannot be read or parsed or if it is invalid
     */

    public void addSchema(Source schemaSource) throws TransformerException {
        try {
            EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();
            XsdCompiler compiler = getProcessor().newXsdCompiler();
            XsdSchema newSchema = compiler.compile(schemaSource);
            if (schema == null) {
                schema = newSchema;
            } else {
                schema = schema.combine(newSchema);
            }
        } catch (SaxonApiException e) {
            throw new TransformerException(e);
        }
    }


}

// Copyright (c) 2005-2022 Saxonica Limited
