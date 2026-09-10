package com.saxonica.config;

import net.sf.saxon.Configuration;
import net.sf.saxon.jaxp.StreamingTransformerImpl;
import net.sf.saxon.jaxp.TemplatesImpl;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.sax.TransformerHandler;

/**
 * The is a customized JAXP TransformerFactory implementation for the enterprise
 * edition of Saxon. It is designed specifically for running XSLT 3.0 streaming
 * transformations, where the Source object supplied to the Transformer.transform()
 * method is supplied as a SAXSource or StreamSource. Because it is designed for
 * streaming, the source document does not become the global context item (specifically,
 * if a global variable references the value of the context item ".", a dynamic error
 * XPDY0002 occurs). This means that this JAXP TransformerFactory is not a 100% compatible
 * replacement for a "standard" TransformerFactory.
 */
public class StreamingTransformerFactory extends EnterpriseTransformerFactory {

    /**
     * Create a StreamingTransformerFactory. An EnterpriseConfiguration will
     * be created automatically.
     */

    public StreamingTransformerFactory() {
        super();
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT, "XSLT streaming", -1);
    }

    /**
     * Construct a StreamingTransformerFactory using an existing Configuration.
     *
     * @param config the configuration to be used. This must be an EnterpriseConfiguration.
     */

    public StreamingTransformerFactory(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    @Override
    public void setConfiguration(Configuration config) {
        config.checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT, "XSLT streaming", -1);
        super.setConfiguration(config);
    }

    @Override
    public synchronized Templates newTemplates(Source source) throws TransformerConfigurationException {
        Templates t = super.newTemplates(source);
        ((TemplatesImpl) t).setForceStreaming(true);
        return t;
    }

    @Override
    public synchronized Templates newTemplates(Source source, CompilerInfo info) throws TransformerConfigurationException {
        Templates t = super.newTemplates(source, info);
        ((TemplatesImpl) t).setForceStreaming(true);
        return t;
    }

    /**
     * Get a TransformerHandler object that can process SAX
     * ContentHandler events into a Result, based on the Templates argument.
     *
     * @param templates The compiled transformation instructions.
     * @return TransformerHandler ready to transform SAX events.
     * @throws javax.xml.transform.TransformerConfigurationException If for some reason the
     *                                                               TransformerHandler can not be created.
     */

    @Override
    public TransformerHandler newTransformerHandler(Templates templates)
            throws TransformerConfigurationException {
        if (!(templates instanceof TemplatesImpl)) {
            throw new TransformerConfigurationException("Templates object was not created by Saxon");
        }
        StreamingTransformerImpl transformer = (StreamingTransformerImpl) templates.newTransformer();
        try {
            return transformer.newTransformerHandler();
        } catch (XPathException e) {
            throw new TransformerConfigurationException(e);
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
