package com.saxonica.config;

import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.sdoc.SchemaDocument;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.xpath.XPathFactoryImpl;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;

/**
 * An implementation of the JAXP XPath Factory that creates an enterprise configuration. This implementation
 * is used when Saxon-EE is present, and when the object model selected is the Saxon native object model.
 */
public class EnterpriseXPathFactory extends XPathFactoryImpl {

    /**
     * Default constructor: this creates a Configuration as well as creating the enterprise
     * XPathFactory. Any documents accessed using this XPathFactory must be built using this same Configuration.
     * The Configuration has validation mode set to Lax, so validation of input documents will be attempted,
     * provided that a suitable schema is available.
     */

    public EnterpriseXPathFactory() {
        Configuration config = new EnterpriseConfiguration();
        config.setSchemaValidationMode(Validation.LAX);
        config.setProcessor(this);
        setConfiguration(config);
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
     * Load a schema, which will be available for use by all subsequent operations using
     * this TransformerFactory.
     *
     * @param schemaSource the JAXP Source object identifying the schema document to be loaded
     * @throws javax.xml.transform.TransformerException
     *          if the schema cannot be read or parsed or if it is invalid
     */

    public void addSchema(Source schemaSource) throws TransformerException {
        EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();
        SchemaCompiler compiler = config.newSchemaCompiler();
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        SchemaDocument schemadoc = SchemaReader.read(schemaSource, compiler, pipe, null);
        compiler.validate(schemadoc);
    }

}

// Copyright (c) 2005-2023 Saxonica Limited
