using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDOverride : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.SingleNamespaceSchema originalSchema;
        private Saxon.Eej.ee.schema.sdoc.SchemaDocument originalSchemaDocument;
        private Saxon.Eej.ee.schema.SingleNamespaceSchema replacementSchema;
        public XSDOverride()  {
        }
        protected internal override void prepareAttributes() {
            if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                error("The xs:override element requires XSD 1.1 to be enabled");
            }
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "schemaLocation"};
            allowAttributes(atts, allowed);
            processId();
            string schemaLocation = atts.getValue("", "schemaLocation");
            if ((schemaLocation == null)) {
                missingAttribute("schemaLocation");
                return ;
            }
            string documentBase = getBaseURI();
            Saxon.Eej.ee.schema.SchemaCompiler compiler = getXSDSchema().getSchemaCompiler();
            Saxon.Ejavax.xml.transform.Source source = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.getSource(documentBase, schemaLocation, compiler, null);
            if (((source == null) || (source.getSystemId() == null))) {
                throw new Saxon.Hej.type.SchemaException("The xsl:override/@schemaLocation resolves to a source with no known system ID");
            }
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append(source.getSystemId());
            fsb.append("?referrer=");
            fsb.append(documentBase);
            fsb.append(";index=");
            fsb.append((Saxon.Hej.tree.util.Navigator.getNumberSimple(this, null)+""));
            string key = fsb.ToString();
            if (compiler.isBeingRead(key)) {
                replacementSchema = null;
                return ;
            }
            compiler.setBeingRead(key);
            if ((compiler.getExistingSchemaDocument(schemaLocation, getXSDSchema().getTargetNamespace()) != null)) {
                error("Saxon can't handle overriding of a schema document that has already been loaded by a different route");
                return ;
            }
            try {
                Saxon.Hej.@event.PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
                Saxon.Eej.ee.schema.sdoc.SchemaDocument schemaDoc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.buildSchemaDocument(source, compiler, pipe, this);
                originalSchemaDocument = schemaDoc;
                originalSchema = schemaDoc.getXSDSchema().getSchema();
                string originalTargetNS = schemaDoc.getXSDSchema().getTargetNamespace();
                if (!(getSchema().getTargetNamespace().Equals(originalTargetNS))) {
                    if (originalTargetNS.Length==0) {
                        originalSchemaDocument = schemaDoc = compiler.doChameleonTransformation(schemaDoc.getDocument().getRootNode(), this);
                        originalSchema = schemaDoc.getXSDSchema().getSchema();
                        try {
                            Saxon.Eej.ee.schema.sdoc.XSDSchema overridden = schemaDoc.getXSDSchema();
                            overridden.processAllAttributes();
                            overridden.validateSubtree(compiler);
                        } catch (Saxon.Hej.type.SchemaException err) {
                            compiler.warning("The target of the xs:override is not a valid schema", this);
                        }
                    } else {
                        error("The targetNamespace of the overridden schema document differs from that of the overriding schema document");
                        originalSchema = null;
                    }
                }
                Saxon.Eej.ee.schema.sdoc.SchemaDocument convertedSchemaDocument = compiler.doOverrideTransformation(schemaDoc.getDocument().getRootNode(), this);
                Saxon.Eej.ee.schema.sdoc.XSDSchema top = convertedSchemaDocument.getXSDSchema();
                top.processAllAttributes();
                top.validateSubtree(compiler);
                replacementSchema = top.getSchema();
            } catch (Saxon.Hej.type.SchemaException e) {
                System.Exception cause = e.getException();
                if ((cause is System.IO.IOException)) {
                    warning(("Failed to locate overridden schema document "+schemaLocation));
                    originalSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), "");
                } else {
                    throw;
                }
            }
        }
        public Saxon.Eej.ee.schema.SingleNamespaceSchema getReplacementSchema() {
            return replacementSchema;
        }
        public Saxon.Eej.ee.schema.SingleNamespaceSchema getOriginalSchema() {
            return originalSchema;
        }
        public Saxon.Eej.ee.schema.sdoc.SchemaDocument getOriginalSchemaDocument() {
            return originalSchemaDocument;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                error("xs:override requires XML Schema 1.1 to be enabled");
                return ;
            }
            if ((originalSchema == null)) {
                return ;
            }
            System.Collections.Generic.HashSet<string> redefinedTypes = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedAttributeGroups = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedModelGroups = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedElements = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedAttributes = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.HashSet<string> redefinedNotations = new System.Collections.Generic.HashSet<string>();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                    {
                        string attributeName = child.getAttributeValue("", "name");
                        if (((attributeName != null) && !(redefinedAttributes.Add(attributeName)))) {
                            error("Attempting to override the same attribute declaration more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                    {
                        string elementName = child.getAttributeValue("", "name");
                        if (((elementName != null) && !(redefinedElements.Add(elementName)))) {
                            error("Attempting to override the same element declaration more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    {
                        string attributeGroupName = child.getAttributeValue("", "name");
                        if (((attributeGroupName != null) && !(redefinedAttributeGroups.Add(attributeGroupName)))) {
                            error("Attempting to override the same attribute group more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        string typeName = child.getAttributeValue("", "name");
                        if (((typeName != null) && !(redefinedTypes.Add(typeName)))) {
                            error("Attempting to override the same type more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    {
                        string modelGroupName = child.getAttributeValue("", "name");
                        if (((modelGroupName != null) && !(redefinedModelGroups.Add(modelGroupName)))) {
                            error("Attempting to override the same named model group more than once");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_notation:
                    {
                        string notationName = child.getAttributeValue("", "name");
                        if (((notationName != null) && !(redefinedNotations.Add(notationName)))) {
                            error("Attempting to override the same notation declaration more than once");
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
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (((originalSchema == null) || (replacementSchema == null))) {
                return ;
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int childfp = child.getFingerprint();
                switch (childfp) {
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                    {
                        Saxon.Eej.ee.schema.AttributeDecl attribute = ((Saxon.Eej.ee.schema.sdoc.XSDAttribute)(child)).getAttributeDeclaration();
                        if ((attribute == null)) {
                            break ;
                        }
                        Saxon.Eej.ee.schema.AttributeDecl existing = originalSchema.getAttributeDecl(attribute.getComponentName());
                        if ((existing != null)) {
                            attribute.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    {
                        Saxon.Eej.ee.schema.AttributeGroupDecl ag = ((Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup)(child)).getAttributeGroupDecl();
                        if ((ag == null)) {
                            break ;
                        }
                        Saxon.Eej.ee.schema.AttributeGroupDecl existing = originalSchema.getAttributeGroup(ag.getAttributeGroupName());
                        if ((existing != null)) {
                            ag.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                    {
                        Saxon.Eej.ee.schema.UserComplexType typeDef = ((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(child)).getComplexType();
                        if ((typeDef == null)) {
                            break ;
                        }
                        Saxon.Hej.type.SchemaType existing = originalSchema.getType(typeDef.getTypeName());
                        if ((existing != null)) {
                            typeDef.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition typeDef;
                        try {
                            typeDef = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                        } catch (Saxon.Hej.type.SchemaException e) {
                            error(e.Message);
                            return ;
                        }
                        if ((typeDef == null)) {
                            break ;
                        }
                        Saxon.Hej.type.SimpleType existing = originalSchema.getSimpleType(typeDef.getTypeName());
                        if ((existing != null)) {
                            typeDef.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                    {
                        Saxon.Eej.ee.schema.ElementDecl element = ((Saxon.Eej.ee.schema.sdoc.XSDElement)(child)).getElementDeclaration();
                        if ((element == null)) {
                            break ;
                        }
                        Saxon.Eej.ee.schema.ElementDecl existing = originalSchema.getElementDecl(element.getComponentName());
                        if ((existing != null)) {
                            element.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    {
                        Saxon.Eej.ee.schema.ModelGroupDefinition group = ((Saxon.Eej.ee.schema.sdoc.XSDGroup)(child)).getGroupDeclaration();
                        if ((group == null)) {
                            break ;
                        }
                        Saxon.Eej.ee.schema.ModelGroupDefinition existing = originalSchema.getGroup(group.getModelGroupName());
                        if ((existing != null)) {
                            group.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_notation:
                    {
                        Saxon.Eej.ee.schema.Notation n = ((Saxon.Eej.ee.schema.sdoc.XSDNotation)(child)).getNotation();
                        if ((n == null)) {
                            break ;
                        }
                        Saxon.Eej.ee.schema.Notation existing = originalSchema.getNotation(n.getNotationName());
                        if ((existing != null)) {
                            n.setRedefinitionLevel((existing.getRedefinitionLevel()+1));
                        }
                        break ;
                    }
                }
            }
            if ((replacementSchema != null)) {
                replacementSchema.copyTo(getSchema(), compiler);
            }
        }
    
    }
}