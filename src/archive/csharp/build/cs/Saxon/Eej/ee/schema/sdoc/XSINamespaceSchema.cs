using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSINamespaceSchema {
        public static Saxon.Eej.ee.schema.PreparedSchema buildSchema(Saxon.Eej.config.EnterpriseConfiguration config) {
            Saxon.Eej.ee.schema.SingleNamespaceSchema schema = new Saxon.Eej.ee.schema.SingleNamespaceSchema(config, Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE);
            Saxon.Eej.ee.schema.AttributeDecl xsiType = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xsiType.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XSI_TYPE));
            xsiType.setSimpleType(Saxon.Hej.type.BuiltInAtomicType.QNAME);
            schema.addAttributeDecl(xsiType);
            Saxon.Eej.ee.schema.AttributeDecl xsiNil = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xsiNil.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XSI_NIL));
            xsiNil.setSimpleType(Saxon.Hej.type.BuiltInAtomicType.BOOLEAN);
            schema.addAttributeDecl(xsiNil);
            Saxon.Eej.ee.schema.AttributeDecl xsiNoNamespaceSchemaLocation = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xsiNoNamespaceSchemaLocation.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION));
            xsiNoNamespaceSchemaLocation.setSimpleType(Saxon.Hej.type.BuiltInAtomicType.QNAME);
            schema.addAttributeDecl(xsiNoNamespaceSchemaLocation);
            Saxon.Eej.ee.schema.AttributeDecl xsiSchemaLocation = new Saxon.Eej.ee.schema.AttributeDecl(schema.getConfiguration(), true);
            xsiSchemaLocation.setAttributeName(Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XSI_SCHEMA_LOCATION));
            xsiSchemaLocation.setSimpleType(Saxon.Hej.type.BuiltInListType.ANY_URIS);
            schema.addAttributeDecl(xsiNoNamespaceSchemaLocation);
            return schema;
        }
    
    }
}