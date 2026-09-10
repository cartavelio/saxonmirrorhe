package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaException;

import java.util.ArrayList;
import java.util.List;

/**
 * This module constructs synthetically (that is, without reading an XML schema document) the
 * schema components for the XML namespace
 */

public class XMLNamespaceSchema {

    /**
     * Build the schema components for the XML namespace
     *
     * @param config the Saxon configuration
     * @return a PreparedSchema containing the schema components for the XML namespace
     */

    public static PreparedSchema buildSchema(EnterpriseConfiguration config) {

        SchemaCompiler compiler = new SchemaCompiler(config);
        SingleNamespaceSchema schema = new SingleNamespaceSchema(config, NamespaceUri.XML);

        // xml:lang attribute

        List<TypeReference> members = new ArrayList<>(2);
        members.add(new TypeReference(StandardNames.XS_LANGUAGE, config, null));

        UserAtomicType zls = new UserAtomicType(config);
        int zlsFingerprint = StandardNames.XML_ZERO_LENGTH_STRING_TYPE;
        zls.setBaseTypeReference(new TypeReference(StandardNames.XS_STRING, config, null));
        zls.setTypeName(config.getNamePool().getStructuredQName(zlsFingerprint), zlsFingerprint);
        schema.addType(zls);

        List<Facet> facetList = new ArrayList<>(1);
        LengthFacet lengthFacet = new LengthFacet();
        lengthFacet.setNumericValue(0);
        facetList.add(lengthFacet);
        zls.setFacetList(facetList);
        final TypeReference zlsRef = new TypeReference(zlsFingerprint, config, null);
        zlsRef.setTarget(zls);
        members.add(zlsRef);

        UserUnionType u = new UserUnionType(config);
        u.setMemberTypeReferences(members);
        u.setTypeName(StandardNames.getStructuredQName(StandardNames.XML_LANG_TYPE), StandardNames.XML_LANG_TYPE);
        schema.addType(u);

        AttributeDecl xmlLang = new AttributeDecl(schema.getConfiguration(), true);
        xmlLang.setAttributeName(new StructuredQName("xml", NamespaceUri.XML, "lang"));
        xmlLang.setSimpleType(u);
        schema.addAttributeDecl(xmlLang);

        // xml:space attribute

        UserAtomicType sp = new UserAtomicType(config);
        sp.setBaseTypeReference(new TypeReference(StandardNames.XS_NCNAME, config, null));
        sp.setTypeName(StandardNames.getStructuredQName(StandardNames.XML_SPACE_TYPE), StandardNames.XML_SPACE_TYPE);
        facetList = new ArrayList<>(1);
        EnumerationFacet defaultFacet = new EnumerationFacet();
        defaultFacet.setValue("default");
        EnumerationFacet preserveFacet = new EnumerationFacet();
        preserveFacet.setValue("preserve");
        facetList.add(defaultFacet);
        facetList.add(preserveFacet);
        sp.setFacetList(facetList);
        try {
            sp.mergeEnumerationFacets(compiler);
        } catch (SchemaException err) {
            throw new AssertionError(err.getMessage());
        }
        schema.addType(sp);

        AttributeDecl xmlSpace = new AttributeDecl(schema.getConfiguration(), true);
        xmlSpace.setAttributeName(StandardNames.getStructuredQName(StandardNames.XML_SPACE));
        xmlSpace.setSimpleType(sp);
        schema.addAttributeDecl(xmlSpace);

        // xml:base attribute

        AttributeDecl xmlBase = new AttributeDecl(schema.getConfiguration(), true);
        xmlBase.setAttributeName(StandardNames.getStructuredQName(StandardNames.XML_BASE));
        xmlBase.setSimpleType(BuiltInAtomicType.ANY_URI);
        schema.addAttributeDecl(xmlBase);

        // xml:id attribute

        AttributeDecl xmlId = new AttributeDecl(schema.getConfiguration(), true);
        xmlId.setAttributeName(StandardNames.getStructuredQName(StandardNames.XML_ID));
        xmlId.setSimpleType(BuiltInAtomicType.ID);
        schema.addAttributeDecl(xmlId);

        return schema;
    }


}

//
// Copyright (c) 2006-2023 Saxonica Limited
//

