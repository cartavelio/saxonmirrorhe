package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.AttributeDecl;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SingleNamespaceSchema;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.BuiltInListType;

/**
 * This module constructs synthetically (that is, without reading an XML schema document) the
 * schema components for the XML namespace
 */

public class XSINamespaceSchema {

    /**
     * Build the schema components for the XML namespace
     *
     * @param config the Saxon configuration
     * @return a PreparedSchema containing the schema components for the XML namespace
     */

    public static PreparedSchema buildSchema(EnterpriseConfiguration config) {


        SingleNamespaceSchema schema = new SingleNamespaceSchema(config, NamespaceUri.SCHEMA_INSTANCE);

        // xsi:type attribute

        AttributeDecl xsiType = new AttributeDecl(schema.getConfiguration(), true);
        xsiType.setAttributeName(StandardNames.getStructuredQName(StandardNames.XSI_TYPE));
        xsiType.setSimpleType(BuiltInAtomicType.QNAME);
        schema.addAttributeDecl(xsiType);

        // xsi:nil attribute

        AttributeDecl xsiNil = new AttributeDecl(schema.getConfiguration(), true);
        xsiNil.setAttributeName(StandardNames.getStructuredQName(StandardNames.XSI_NIL));
        xsiNil.setSimpleType(BuiltInAtomicType.BOOLEAN);
        schema.addAttributeDecl(xsiNil);

        // xsi:noNamespaceSchemaLocation attribute

        AttributeDecl xsiNoNamespaceSchemaLocation = new AttributeDecl(schema.getConfiguration(), true);
        xsiNoNamespaceSchemaLocation.setAttributeName(StandardNames.getStructuredQName(StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION));
        xsiNoNamespaceSchemaLocation.setSimpleType(BuiltInAtomicType.QNAME);
        schema.addAttributeDecl(xsiNoNamespaceSchemaLocation);

        // xsi:noNamespaceSchemaLocation attribute

        AttributeDecl xsiSchemaLocation = new AttributeDecl(schema.getConfiguration(), true);
        xsiSchemaLocation.setAttributeName(StandardNames.getStructuredQName(StandardNames.XSI_SCHEMA_LOCATION));
        xsiSchemaLocation.setSimpleType(BuiltInListType.ANY_URIS);
        schema.addAttributeDecl(xsiNoNamespaceSchemaLocation);

        return schema;
    }


}

// Copyright (c) 2010-2022 Saxonica Limited

