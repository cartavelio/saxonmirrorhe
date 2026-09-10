package com.saxonica.config;

import com.saxonica.config.pe.ConfigurationReaderPE;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.sdoc.SchemaDocument;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.File;

/**
 * The is the JAXP TransformerFactory implementation for the enterprise
 * edition of Saxon. It can be invoked by setting the system property
 * javax.xml.transform.TransformerFactory to the value com.saxonica.EnterpriseTransformerFactory,
 * and calling TransformerFactory.newInstance(), or it can be instantiated directly.
 */
public class EnterpriseTransformerFactory extends ProfessionalTransformerFactory {

    /**
     * Create an EnterpriseTransformerFactory. An EnterpriseConfiguration will
     * be created automatically.
     */

    public EnterpriseTransformerFactory() {
        super(new EnterpriseConfiguration());
        EnterpriseConfiguration config = (EnterpriseConfiguration)getConfiguration();
        config.setProcessor(this);
        //setConfiguration(config);
    }

    /**
     * Construct a TransformerFactory using an existing Configuration.
     *
     * @param config the configuration to be used. This must be an EnterpriseConfiguration.
     */

    public EnterpriseTransformerFactory(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Load a schema, which will be available for use by all subsequent operations using
     * this TransformerFactory.
     *
     * @param schemaSource the JAXP Source object identifying the schema document to be loaded
     * @throws TransformerException if the schema cannot be read or parsed or if it is invalid
     */

    public void addSchema(Source schemaSource) throws TransformerException {
        EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();
        SchemaCompiler compiler = config.newSchemaCompiler();
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        SchemaDocument schemadoc = SchemaReader.read(schemaSource, compiler, pipe, null);
        compiler.validate(schemadoc);
    }

    /**
     * Allows the user to set specific attributes on the underlying
     * implementation.  An attribute in this context is defined to
     * be an option that the implementation provides.
     *
     * @param name  The name of the attribute. This must be one of the constants
     *              defined in class {@link net.sf.saxon.lib.FeatureKeys}.
     * @param value The value of the attribute.
     * @throws IllegalArgumentException thrown if Saxon
     *                                  doesn't recognize the attribute.
     * @see net.sf.saxon.lib.FeatureKeys
     */
    @Override
    public void setAttribute(String name, Object value) throws IllegalArgumentException {
        if (name.equals(FeatureKeys.CONFIGURATION_FILE)) {
            ConfigurationReaderPE reader = new ConfigurationReaderPE();
            try {
                setConfiguration(reader.makeConfiguration(new StreamSource(new File((String) value))));
            } catch (XPathException err) {
                throw new IllegalArgumentException(err);
            }
        } else {
            super.setAttribute(name, value);
        }
    }
}

// Copyright (c) 2004-2022 Saxonica Limited
