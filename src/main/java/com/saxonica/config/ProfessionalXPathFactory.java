package com.saxonica.config;

import net.sf.saxon.Configuration;
import net.sf.saxon.xpath.XPathFactoryImpl;

/**
 * An implementation of the JAXP XPath Factory that creates a Professional configuration. This implementation
 * is used when Saxon-PE is present.
 */
public class ProfessionalXPathFactory extends XPathFactoryImpl {

    /**
     * Default constructor: this creates a Configuration as well as creating the professional
     * XPathFactory. Any documents accessed using this XPathFactory must be built using this same Configuration.
     */

    public ProfessionalXPathFactory() {
        Configuration config = new ProfessionalConfiguration();
        config.setProcessor(this);
        setConfiguration(config);
    }

    /**
     * Constructor using a user-supplied Configuration.
     * This constructor is useful if the document to be queried already exists, as it allows the configuration
     * associated with the document to be used with this XPathFactory.
     *
     * @param config the configuration to be used. This must be a ProfessionalConfiguration.
     */

    public ProfessionalXPathFactory(ProfessionalConfiguration config) {
        setConfiguration(config);
    }


}

// Copyright (c) 2005-2023 Saxonica Limited