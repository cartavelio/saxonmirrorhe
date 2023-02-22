using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XMLNamespaceSchema {
        public static Saxon.Eej.ee.schema.PreparedSchema buildSchema(Saxon.Eej.config.EnterpriseConfiguration config) {
            Saxon.Eej.ee.schema.SchemaCompiler compiler = new Saxon.Eej.ee.schema.SchemaCompiler(config);
            Saxon.Eej.ee.schema.SingleNamespaceSchema schema = new Saxon.Eej.ee.schema.SingleNamespaceSchema(config, Saxon.Hej.om.NamespaceUri.XML);
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeReference> members = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>(2);
            members.Add(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_LANGUAGE, config, null));
            Saxon.Eej.ee.schema.UserAtomicType zls = new Saxon.Eej.ee.schema.UserAtomicType(config);
            zls.setBaseTypeReference(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_STRING, config, null));
            zls.setTypeName(new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, "zero-length-string"), config.getNamePool().allocateFingerprint(Saxon.Hej.om.NamespaceUri.SAXON, "zero-length-string"));
            schema.addType(zls);
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facetList = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>(1);
            Saxon.Eej.ee.schema.LengthFacet lengthFacet = new Saxon.Eej.ee.schema.LengthFacet();
            lengthFacet.setNumericValue(0);
            facetList.Add(lengthFacet);
            zls.setFacetList(facetList);
            Saxon.Eej.ee.schema.TypeReference zlsRef = new Saxon.Eej.ee.schema.TypeReference(zls.getFingerprint(), config, null);
            zlsRef.setTarget(zls);
            members.Add(zlsRef);
            Saxon.Eej.ee.schema.UserUnionType u = new Saxon.Eej.ee.schema.UserUnionType(config);
            u.setMemberTypeReferences(members);
            u.setTypeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XML_LANG_TYPE), Saxon.Hej.om.StandardNames.XML_LANG_TYPE);
            schema.addType(u);
            Saxon.Eej.ee.schema.AttributeDecl xmlLang = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xmlLang.setAttributeName(new Saxon.Hej.om.StructuredQName("xml", Saxon.Hej.om.NamespaceUri.XML, "lang"));
            xmlLang.setSimpleType(u);
            schema.addAttributeDecl(xmlLang);
            Saxon.Eej.ee.schema.UserAtomicType sp = new Saxon.Eej.ee.schema.UserAtomicType(config);
            sp.setBaseTypeReference(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_NCNAME, config, null));
            sp.setTypeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XML_SPACE_TYPE), Saxon.Hej.om.StandardNames.XML_SPACE_TYPE);
            facetList = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>(1);
            Saxon.Eej.ee.schema.EnumerationFacet defaultFacet = new Saxon.Eej.ee.schema.EnumerationFacet();
            defaultFacet.setValue("default");
            Saxon.Eej.ee.schema.EnumerationFacet preserveFacet = new Saxon.Eej.ee.schema.EnumerationFacet();
            preserveFacet.setValue("preserve");
            facetList.Add(defaultFacet);
            facetList.Add(preserveFacet);
            sp.setFacetList(facetList);
            try {
                sp.mergeEnumerationFacets(compiler);
            } catch (Saxon.Hej.type.SchemaException err) {
                throw new Saxon.Ejava.lang.AssertionError(err.Message);
            }
            schema.addType(sp);
            Saxon.Eej.ee.schema.AttributeDecl xmlSpace = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xmlSpace.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XML_SPACE));
            xmlSpace.setSimpleType(sp);
            schema.addAttributeDecl(xmlSpace);
            Saxon.Eej.ee.schema.AttributeDecl xmlBase = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xmlBase.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XML_BASE));
            xmlBase.setSimpleType(Saxon.Hej.type.BuiltInAtomicType.ANY_URI);
            schema.addAttributeDecl(xmlBase);
            Saxon.Eej.ee.schema.AttributeDecl xmlId = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xmlId.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XML_ID));
            xmlId.setSimpleType(Saxon.Hej.type.BuiltInAtomicType.ID);
            schema.addAttributeDecl(xmlId);
            return schema;
        }
    
    }
}