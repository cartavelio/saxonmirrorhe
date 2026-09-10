package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.AttributeWildcard;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:anyAttribute element in an XML Schema document
 */

public class XSDAnyAttribute extends AnnotationParent {


    /**
     * The wildcard we are constructing
     */
    private AttributeWildcard wildcard = null;


    public XSDAnyAttribute() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        wildcard = new AttributeWildcard((EnterpriseConfiguration) getConfiguration());
        wildcard.setLocator(this);

        AttributeMap atts = attributes();

        String[] allowed = {"id", "namespace", "notNamespace", "notQName", "processContents"};
        allowAttributes(atts, allowed);
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

        //-- @processContents
        attValue = atts.getValue(NamespaceUri.NULL, "processContents");

        if (attValue != null) {
            if (XSDAny.isProcessName(attValue)) {
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
    }

    /**
     * Returns the Wildcard
     *
     * @return the Wildcard
     */
    /*@Nullable*/
    public AttributeWildcard getWildcard() {
        return wildcard;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited