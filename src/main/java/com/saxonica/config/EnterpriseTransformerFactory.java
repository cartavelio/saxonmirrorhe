package com.saxonica.config;

import com.saxonica.config.pe.ConfigurationReaderPE;
import net.sf.saxon.jaxp.TemplatesImpl;
import net.sf.saxon.lib.ErrorReporterToListener;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.TransformerConfigurationException;
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

    XsdSchema schema = null;

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

    /**
     * Process the Source into a Templates object, which is a
     * a compiled representation of the source. This Templates object
     * may then be used concurrently across multiple threads.  Creating
     * a Templates object allows the TransformerFactory to do detailed
     * performance optimization of transformation instructions, without
     * penalizing runtime transformation.
     *
     * @param source An object that holds a URL, input stream, etc.
     * @return A Templates object capable of being used for transformation purposes,
     * never null.
     * @throws TransformerConfigurationException May throw this during the parse when it
     *                                           is constructing the Templates object and fails.
     */
    @Override
    public synchronized Templates newTemplates(Source source) throws TransformerConfigurationException {
        try {
            XsltCompiler compiler = getProcessor().newXsltCompiler();
            if (getErrorListener() != null) {
                compiler.setErrorReporter(new ErrorReporterToListener(getErrorListener()));
            }
            if (schema != null) {
                compiler.useSchema(schema);
            }
            XsltExecutable executable = compiler.compile(source);
            return new TemplatesImpl(this, executable);
        } catch (SaxonApiException e) {
            throw new TransformerConfigurationException(e);
        }
    }

    /**
     * Process the Source into a Templates object, which is a
     * a compiled representation of the source. This Templates object
     * may then be used concurrently across multiple threads.  Creating
     * a Templates object allows the TransformerFactory to do detailed
     * performance optimization of transformation instructions, without
     * penalizing runtime transformation.
     *
     * @param source An object that holds a URL, input stream, etc.
     * @param info   compile-time options for this stylesheet compilation
     * @return A Templates object capable of being used for transformation purposes,
     * never null.
     * @throws TransformerConfigurationException May throw this during the parse when it
     *                                           is constructing the Templates object and fails.
     */
    @Override
    public synchronized Templates newTemplates(Source source, CompilerInfo info) throws TransformerConfigurationException {
        return super.newTemplates(source, info);
    }
}

// Copyright (c) 2004-2023 Saxonica Limited
