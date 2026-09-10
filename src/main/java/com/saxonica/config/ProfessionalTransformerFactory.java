package com.saxonica.config;

import com.saxonica.config.pe.ConfigurationReaderPE;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.stream.StreamSource;
import java.io.File;

/**
 * The is the JAXP TransformerFactory implementation for the professional
 * edition of Saxon. It can be invoked by setting the system property
 * javax.xml.transform.TransformerFactory to the value com.saxonica.ProfessionalTransformerFactory,
 * and calling TransformerFactory.newInstance(), or it can be instantiated directly.
 */
public class ProfessionalTransformerFactory extends TransformerFactoryImpl {

    /**
     * Create a ProfessionalTransformerFactory. A ProfessionalConfiguration will
     * be created automatically.
     */

    public ProfessionalTransformerFactory() {
        ProfessionalConfiguration config = new ProfessionalConfiguration();
        config.setProcessor(this);
        setConfiguration(config);
    }

    /**
     * Construct a ProfessionalTransformerFactory using an existing Configuration.
     *
     * @param config the configuration to be used. This must be a ProfessionalConfiguration.
     */

    public ProfessionalTransformerFactory(ProfessionalConfiguration config) {
        super(config);
    }

    /**
     * Allows the user to set specific attributes on the underlying
     * implementation.  An attribute in this context is defined to
     * be an option that the implementation provides.
     *
     * @param name The name of the attribute. This must be one of the constants
     * defined in class {@link net.sf.saxon.lib.FeatureKeys}.
     * @param value The value of the attribute.
     * @throws IllegalArgumentException thrown if Saxon
     * doesn't recognize the attribute.
     * @see net.sf.saxon.lib.FeatureKeys
     */

    @Override
    public void setAttribute(String name, Object value) throws IllegalArgumentException {
        if (name.equals(FeatureKeys.CONFIGURATION_FILE)) {
            ConfigurationReaderPE reader = new ConfigurationReaderPE();
            try {
                setConfiguration(reader.makeConfiguration(new StreamSource(new File((String)(value)))));
            } catch (XPathException err) {
                throw new IllegalArgumentException(err);
            }
        } else {
            super.setAttribute(name, value);
        }
    }



}

// Copyright (c) 2004-2022 Saxonica Limited
