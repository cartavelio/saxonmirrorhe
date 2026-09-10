package com.saxonica.config.pe;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.xsltextn.ExtensionElementFactory;
import net.sf.saxon.trans.ConfigurationReader;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.Attributes;

/**
 * Subclass of ConfigurationReader providing functionality particular to Saxon Professional Edition
 */
public class ConfigurationReaderPE extends ConfigurationReader {

    /**
     * Code to process details of user-defined XSLT extension elements
     *
     * @param atts the attributes of the element in the configuration file
     */

    @Override
    protected void readExtensionElement(Attributes atts) {
        String namespace = null;
        String className = null;
        for (int i = 0; i < atts.getLength(); i++) {
            if (atts.getURI(i).isEmpty()) {
                String attName = atts.getLocalName(i);
                String value = atts.getValue(i);
                if ("namespace".equals(attName) && value.length() > 0) {
                    namespace = value;
                }
                if ("factory".equals(attName) && value.length() > 0) {
                    className = value;
                }
            }
        }
        if (namespace != null && className != null) {
            try {
                ((ProfessionalConfiguration) config).setExtensionElementNamespace(namespace, className);
            } catch (XPathException e) {
                errorClass("extensionElement", null, className, ExtensionElementFactory.class, e);
            }
        }
    }

}

// Copyright (c) 2011-2020 Saxonica Limited


