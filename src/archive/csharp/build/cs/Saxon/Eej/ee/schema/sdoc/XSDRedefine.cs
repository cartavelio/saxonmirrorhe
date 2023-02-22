using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDRedefine : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.SingleNamespaceSchema externalSchema;
        private Saxon.Eej.ee.schema.SingleNamespaceSchema redefinedSchema;
        private string externalSchemaDocumentURI;
        public XSDRedefine()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "schemaLocation"};
            allowAttributes(atts, allowed);
            processId();
            string schemaLocation = atts.getValue("", "schemaLocation");
            if ((schemaLocation == null)) {
                missingAttribute("schemaLocation");
                throw new Saxon.Hej.type.SchemaException("Processing abandoned");
            }
            string documentBase = getBaseURI();
            Saxon.Hej.@event.PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
            Saxon.Eej.ee.schema.SchemaCompiler compiler = getXSDSchema().getSchemaCompiler();
            Saxon.Ejavax.xml.transform.Source source = null;
            try {
                source = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.getSource(documentBase, schemaLocation, compiler, null);
            } catch (Saxon.Hej.type.SchemaException e) {
                handleSchemaException(schemaLocation, e);
            }
            if ((source == null)) {
                warning(("Failed to locate redefined schema document "+schemaLocation));
                externalSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), "");
            }
            if ((source != null)) {
                if (compiler.isBeingRead(source.getSystemId())) {
                    error((("The schema document "+source.getSystemId())+" includes or redefines itself recursively"));
                    return ;
                }
                if ((compiler.getExistingSchemaDocument(schemaLocation, getXSDSchema().getTargetNamespace()) != null)) {
                    error("Saxon can't handle redefinition of a schema document that has already been loaded by a different route");
                    return ;
                }
                try {
                    Saxon.Eej.ee.schema.sdoc.SchemaDocument schemaDoc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(source, compiler, pipe, this);
                    Saxon.Eej.ee.schema.sdoc.XSDSchema redefined = schemaDoc.getXSDSchema();
                    externalSchema = redefined.getSchema();
                    externalSchemaDocumentURI = redefined.getSystemId();
                } catch (Saxon.Hej.type.SchemaException e) {
                    handleSchemaException(schemaLocation, e);
                }
            }
        }
        private void handleSchemaException(string schemaLocation, Saxon.Hej.type.SchemaException e) {
            System.Exception cause = e.getException();
            if (/*134*/Saxon.Eej.ee.schema.sdoc.XSDInclude.isFileNotFoundException(cause)) {
                warning(("Failed to locate redefined schema document "+schemaLocation));
                externalSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), "");
            } else {
                throw e;
            }
        }
        public Saxon.Eej.ee.schema.SingleNamespaceSchema getRedefinedSchema() {
            return redefinedSchema;
        }
        public Saxon.Eej.ee.schema.SingleNamespaceSchema getExternalSchema() {
            return externalSchema;
        }
        public string getExternalSchemaDocumentURI() {
            return externalSchemaDocumentURI;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((externalSchema == null)) {
                return ;
            }
            System.Collections.Generic.HashSet<string> redefinedTypes = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedAttributeGroups = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedModelGroups = new System.Collections.Generic.HashSet<string>();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    {
                        string attributeGroupName = child.getAttributeValue("", "name");
                        if (((attributeGroupName != null) && !(redefinedAttributeGroups.Add(attributeGroupName)))) {
                            error("Attempting to redefine the same attribute group more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        string typeName = child.getAttributeValue("", "name");
                        if (((typeName != null) && !(redefinedTypes.Add(typeName)))) {
                            error("Attempting to redefine the same type more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    {
                        string modelGroupName = child.getAttributeValue("", "name");
                        if (((modelGroupName != null) && !(redefinedModelGroups.Add(modelGroupName)))) {
                            error("Attempting to redefine the same named model group more than once");
                        }
                        break ;
                    }
                  default:
                    {
                        illegalElement(child);
                        break ;
                    }
                }
            }
            redefinedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema(externalSchema.getConfiguration(), externalSchema.getTargetNamespace());
            externalSchema.copyTo(redefinedSchema, compiler);
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((redefinedSchema != null)) {
                redefinedSchema.copyTo(getSchema(), compiler);
            }
        }
    
    }
}