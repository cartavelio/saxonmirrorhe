package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.ElementWildcard;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:any element in an XML Schema document
 */

public class XSDAny extends AnnotationParent {

    /**
     * The wildcard we are constructing
     */
    private ElementWildcard wildcard = null;


    public XSDAny() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {
        wildcard = new ElementWildcard((EnterpriseConfiguration)getConfiguration());
        wildcard.setLocator(this);

        AttributeMap atts = attributes();
        if (getParent() instanceof XSDDefaultOpenContent || getParent() instanceof XSDOpenContent) {
            String[] allowed = {"id", "namespace", "notNamespace", "notQName", "processContents"};
            allowAttributes(atts, allowed);
        } else {
            String[] allowed = {"id", "maxOccurs", "minOccurs", "namespace",
                    "notNamespace", "notQName", "processContents"};
            allowAttributes(atts, allowed);
        }

        checkMutuallyExclusiveAttributes("namespace", "notNamespace");
        String attValue;

        //-- @namespace
        attValue = atts.getValue(NamespaceUri.NULL, "namespace");
        if (attValue != null) {
            processAllowedNamespaces(attValue, wildcard.getWildcard());
        }

        //-- @notNamespace
        attValue = atts.getValue(NamespaceUri.NULL, "notNamespace");
        if (attValue != null) {
            processDisallowedNamespaces(attValue, wildcard.getWildcard());
        }

        //-- @notQName
        attValue = atts.getValue(NamespaceUri.NULL, "notQName");
        if (attValue != null) {
            processDisallowedQNames(attValue, wildcard.getWildcard());
        }

        /*
         * @maxOccurs
         * If maxOccurs is present, the value is either unbounded
         * or the int value of the attribute, otherwise maxOccurs
         * equals the minOccurs value.
         */
        attValue = atts.getValue(NamespaceUri.NULL, "maxOccurs");
        if (attValue != null) {
            int max = processMaxOccurs(attValue);
            wildcard.setMaxOccurs(max);
        }

        //-- minOccurs
        attValue = atts.getValue(NamespaceUri.NULL, "minOccurs");
        if (attValue != null) {
            int minOccurs = processMinOccurs(attValue);
            wildcard.setMinOccurs(minOccurs);
        }

        int maxOccurs = wildcard.getMaxOccurs();
        if (maxOccurs != -1 && wildcard.getMinOccurs() > maxOccurs) {
            error("maxOccurs must not be less than minOccurs");
        }

        //-- processContents
        attValue = atts.getValue(NamespaceUri.NULL, "processContents");
        if (attValue != null) {
            if (isProcessName(attValue)) {
                wildcard.getWildcard().setProcessContents(attValue);
            } else {
                invalidAttributeValue("processContents", attValue, null);
            }
        }

        //-- @id
        processId();

    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        super.validate(compiler);
        if (getAttributeValue(NamespaceUri.NULL, "notNamespace") != null && compiler.getLanguageVersion() != Configuration.XSD11) {
            error("The notNamespace attribute requires XSD 1.1 to be enabled");
        }
        if (getAttributeValue(NamespaceUri.NULL, "notQName") != null && compiler.getLanguageVersion() != Configuration.XSD11) {
            error("The notQName attribute requires XSD 1.1 to be enabled");
        }
    }

    /**
     * Returns the Wildcard.
     *
     * @return the Wildcard
     */
    /*@Nullable*/
    public ElementWildcard getElementWildcard() {
        return wildcard;
    }

    /**
     * Determines whether or not the given name is the name
     * of an XML Schema Wildcard processContents attribute.
     *
     * @param name the name to test
     * @return true if the given name is a valid name of a processContents attribute
     */
    protected static boolean isProcessName(String name) {
        return ("lax".equals(name) ||
                "skip".equals(name) ||
                "strict".equals(name));
    }
}

// Copyright (c) 2011-2023 Saxonica Limited