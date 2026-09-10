package com.saxonica.config.pe;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.xsltextn.ExtensionElementFactory;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.trans.ConfigurationReader;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;

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
    @CSharpReplaceBody(code="base.readExtensionElement(atts);")
    protected void readExtensionElement(AttributeMap atts) {
        String namespace = null;
        String className = null;
        for (AttributeInfo a : atts) {
            NamespaceUri uri = a.getNodeName().getURI();
            String attName = a.getNodeName().getLocalPart();
            String value = a.getValue();
            if (uri.isEmpty()) {
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
                ExtensionElementFactory factory = (ExtensionElementFactory)targetConfig.getDynamicLoader().getInstance(className, null);
                ((ProfessionalConfiguration) targetConfig).setExtensionElementNamespace(namespace, factory);
            } catch (XPathException e) {
                errorClass("extensionElement", null, className, ExtensionElementFactory.class, e);
            }
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


