using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDSchema : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.SingleNamespaceSchema schema = null;
        private Saxon.Eej.ee.schema.SchemaCompiler compiler;
        private int errorCount = 0;
        private Saxon.Hej.expr.instruct.Executable executable;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalParam> @params = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.GlobalParam>();
        private Saxon.Eej.ee.schema.sdoc.SchemaElement referrer;
        private string attributeFormDefault = null;
        private int blockDefault = 0;
        private string elementFormDefault = null;
        private int finalDefault = 0;
        private Saxon.Hej.om.NamespaceUri targetNamespace;
        private bool chameleon = false;
        private System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> importedNamespaces = null;
        private Saxon.Eej.ee.schema.Wildcard openContentWildcard;
        private string openContentMode;
        private bool openContentAppliesToEmpty = false;
        private string xpathDefaultNamespace = "##local";
        private Saxon.Hej.om.StructuredQName defaultAttributeGroup = null;
        private Saxon.Eej.ee.schema.AttributeGroupReference defaultAttributeGroupReference = null;
        private readonly System.Collections.Generic.ISet<string> saxonExtensionEnabled = new System.Collections.Generic.HashSet<string>();
        public XSDSchema()  {
        }
        public override Saxon.Eej.ee.schema.SingleNamespaceSchema getSchema() {
            return schema;
        }
        public Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        public System.Collections.Generic.ISet<string> getAllowedExtensions() {
            return saxonExtensionEnabled;
        }
        public void setSchema(Saxon.Eej.ee.schema.SingleNamespaceSchema schema) {
            this.schema = schema;
        }
        public void setSchemaCompiler(Saxon.Eej.ee.schema.SchemaCompiler manager) {
            this.compiler = manager;
        }
        public Saxon.Eej.ee.schema.SchemaCompiler getSchemaCompiler() {
            return compiler;
        }
        public void setReferrer(Saxon.Eej.ee.schema.sdoc.SchemaElement referrer) {
            this.referrer = referrer;
        }
        public bool isChameleon() {
            return chameleon;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.Configuration config = getConfiguration();
            this.executable = new Saxon.Hej.expr.instruct.Executable(config);
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"attributeFormDefault", "blockDefault", "defaultAttributes", "elementFormDefault", "finalDefault", "id", "targetNamespace", "version", "xpathDefaultNamespace"};
            allowAttributes(atts, allowed);
            string nsURIatt = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
            if (((nsURIatt != null) && nsURIatt.Length==0)) {
                error("The zero-length string is not a legal namespace URI");
                nsURIatt = null;
            }
            if ((nsURIatt != null)) {
                Saxon.Hej.om.NamespaceUri nsURI = Saxon.Hej.om.NamespaceUri.of(nsURIatt);
                if (((referrer is Saxon.Eej.ee.schema.sdoc.XSDInclude) || (referrer is Saxon.Eej.ee.schema.sdoc.XSDRedefine))) {
                    Saxon.Hej.om.NamespaceUri includingNamespace = referrer.getXSDSchema().getTargetNamespace();
                    if (!(includingNamespace.Equals(nsURI))) {
                        error("The target namespace of the included schema must be the same as the target namespace of the including schema");
                        return ;
                    }
                }
                schema.setTargetNamespace(nsURI);
                ((Saxon.Eej.config.EnterpriseConfiguration)(config)).addSurrogateSchema(nsURI);
                targetNamespace = nsURI;
            } else {
                if ((((referrer is Saxon.Eej.ee.schema.sdoc.XSDInclude) || (referrer is Saxon.Eej.ee.schema.sdoc.XSDRedefine)) || (referrer is Saxon.Eej.ee.schema.sdoc.XSDOverride))) {
                    targetNamespace = referrer.getXSDSchema().getTargetNamespace();
                    chameleon = true;
                } else {
                    targetNamespace = Saxon.Hej.om.NamespaceUri.NULL;
                    ((Saxon.Eej.config.EnterpriseConfiguration)(config)).addSurrogateSchema(Saxon.Hej.om.NamespaceUri.NULL);
                }
            }
            string extensions = atts.getValue(Saxon.Hej.om.NamespaceUri.SAXON, "extensions");
            if ((extensions != null)) {
                Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(extensions, " \t\n\r");
                while (st.hasMoreTokens()) {
                    saxonExtensionEnabled.Add(st.nextToken());
                }
            }
            processId();
            atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "version");
            string attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "attributeFormDefault");
            if ((attValue != null)) {
                if ((attValue.Equals("qualified") || attValue.Equals("unqualified"))) {
                    setAttributeFormDefault(attValue);
                } else {
                    invalidAttributeValue("attributeFormDefault", attValue, "Must be 'qualified' or 'unqualified'");
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "defaultAttributes");
            if ((attValue != null)) {
                try {
                    defaultAttributeGroup = getComponentName(attValue, USE_DEFAULT_NS_WITHOUT_IMPORT);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "elementFormDefault");
            if ((attValue != null)) {
                if ((attValue.Equals("qualified") || attValue.Equals("unqualified"))) {
                    setElementFormDefault(attValue);
                } else {
                    invalidAttributeValue("elementFormDefault", attValue, "Must be 'qualified' or 'unqualified'");
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "blockDefault");
            if ((attValue != null)) {
                int v = parseFinalOrBlock("blockDefault", attValue, ((Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION));
                setBlockDefault(v);
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "finalDefault");
            if ((attValue != null)) {
                int v = parseFinalOrBlock("finalDefault", attValue, (((Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVATION_LIST)|Saxon.Hej.type.Derivation.DERIVATION_UNION));
                setFinalDefault(v);
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "xpathDefaultNamespace");
            if ((attValue != null)) {
                xpathDefaultNamespace = attValue;
            }
        }
        public bool isImportedNamespace(Saxon.Hej.om.NamespaceUri uri) {
            if ((importedNamespaces == null)) {
                if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.IMPLICIT_SCHEMA_IMPORTS)) {
                    return true;
                }
                importedNamespaces = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(10);
                foreach (Saxon.Hej.om.NodeInfo child in children()) {
                    if ((child is Saxon.Eej.ee.schema.sdoc.XSDImport)) {
                        Saxon.Hej.om.NamespaceUri targetNamespace = ((Saxon.Eej.ee.schema.sdoc.XSDImport)(child)).getTargetNamespace();
                        importedNamespaces.Add(targetNamespace);
                    }
                }
            }
            return importedNamespaces.Contains(uri);
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                if (!(xpathDefaultNamespace.Equals("##local"))) {
                    error("The xpathDefaultNamespace attribute requires XSD 1.1 to be enabled");
                }
                if ((defaultAttributeGroup != null)) {
                    error("The defaultAttributes attribute requires XSD 1.1 to be enabled");
                }
            }
            checkContentModel(stateTerms, stateTargets, false);
            if ((defaultAttributeGroup != null)) {
                string attValue = getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "defaultAttributes");
                try {
                    defaultAttributeGroup = getComponentName(attValue, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                }
            }
        }
        protected internal override void validateChildren(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children(Saxon.Hej.pattern.NodeSelector.of(c => (c is Saxon.Eej.ee.schema.sdoc.XSDDefaultOpenContent)))) {
                ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).validateSubtree(compiler);
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if (((child is Saxon.Eej.ee.schema.sdoc.SchemaElement) && !(((child is Saxon.Eej.ee.schema.sdoc.XSDDefaultOpenContent))))) {
                    ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).validateSubtree(compiler);
                }
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_OVERRIDE, Saxon.Hej.om.StandardNames.XS_IMPORT, Saxon.Hej.om.StandardNames.XS_DEFAULT_OPEN_CONTENT, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_INCLUDE, Saxon.Hej.om.StandardNames.XS_notation, Saxon.Hej.om.StandardNames.XS_ELEMENT, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_REDEFINE};
        private readonly static int[] state0targets = {2, 2, 3, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_ELEMENT, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_notation, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state1targets = {1, 1, 1, 1, 1, 1, 1, 4};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_OVERRIDE, Saxon.Hej.om.StandardNames.XS_IMPORT, Saxon.Hej.om.StandardNames.XS_DEFAULT_OPEN_CONTENT, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_INCLUDE, Saxon.Hej.om.StandardNames.XS_notation, Saxon.Hej.om.StandardNames.XS_ELEMENT, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_REDEFINE};
        private readonly static int[] state2targets = {2, 2, 3, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_ELEMENT, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_notation, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state3targets = {1, 1, 1, 1, 1, 1, 1, 5};
        private readonly static int[] state4terms = {Saxon.Hej.om.StandardNames.XS_ELEMENT, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_notation, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state4targets = {1, 1, 1, 1, 1, 1, 1, 4};
        private readonly static int[] state5terms = {Saxon.Hej.om.StandardNames.XS_ELEMENT, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_notation, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state5targets = {1, 1, 1, 1, 1, 1, 1, 5};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms, state4terms, state5terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets, state4targets, state5targets};
        protected internal override bool isFinalState(int state) {
            return ((((((state == 0) || (state == 1)) || (state == 2)) || (state == 3)) || (state == 4)) || (state == 5));
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int childfp = child.getFingerprint();
                switch (childfp) {
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                    Saxon.Eej.ee.schema.AttributeDecl att = ((Saxon.Eej.ee.schema.sdoc.XSDAttribute)(child)).getAttributeDeclaration();
                    if ((att == null)) {
                        break ;
                    }
                    if ((schema.getAttributeDecl(att.getComponentName()) != null)) {
                        duplicate("attribute declaration", att.getDisplayName());
                    }
                    schema.addAttributeDecl(att);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    Saxon.Eej.ee.schema.AttributeGroupDecl ag = ((Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup)(child)).getAttributeGroupDecl();
                    if ((ag == null)) {
                        break ;
                    }
                    if ((schema.getAttributeGroup(ag.getAttributeGroupName()) != null)) {
                        duplicate("attribute group", ag.getDisplayName());
                    } else {
                        schema.addAttributeGroup(ag);
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                    Saxon.Eej.ee.schema.UserComplexType complexType = ((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(child)).getComplexType();
                    int fpt = complexType.getFingerprint();
                    if (((schema.getSchemaType(complexType.getTypeName()) != null) && (fpt>1023))) {
                        duplicate("complex type", complexType.getName());
                    } else {
                        schema.addType(complexType);
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_DEFAULT_OPEN_CONTENT:
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition typeDef;
                    try {
                        typeDef = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                    } catch (Saxon.Hej.type.SchemaException e) {
                        error(e.Message);
                        return ;
                    }
                    int fingerprint = typeDef.getFingerprint();
                    if (((schema.getSchemaType(typeDef.getTypeName()) != null) && (fingerprint>1023))) {
                        duplicate("simple type", typeDef.getDisplayName());
                    } else {
                        schema.addType(typeDef);
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                    Saxon.Eej.ee.schema.ElementDecl element = ((Saxon.Eej.ee.schema.sdoc.XSDElement)(child)).getElementDeclaration();
                    string elname = element.getName();
                    if ((schema.getElementDecl(element.getComponentName()) != null)) {
                        duplicate("element declaration", elname);
                    }
                    schema.addElementDecl(element);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    Saxon.Eej.ee.schema.ModelGroupDefinition group = ((Saxon.Eej.ee.schema.sdoc.XSDGroup)(child)).getGroupDeclaration();
                    string groupName = group.getName();
                    if ((groupName == null)) {
                    } else {
                        if ((schema.getGroup(group.getModelGroupName()) != null)) {
                            duplicate("group", groupName);
                        } else {
                            schema.addGroup(group);
                        }
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_notation:
                    Saxon.Eej.ee.schema.Notation n = ((Saxon.Eej.ee.schema.sdoc.XSDNotation)(child)).getNotation();
                    if ((schema.getNotation(n.getNotationName()) != null)) {
                        duplicate("notation", n.getNotationName().getDisplayName());
                    } else {
                        schema.addNotation(n);
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_INCLUDE:
                    Saxon.Eej.ee.schema.PreparedSchema includedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDInclude)(child)).getIncludedSchema();
                    if ((includedSchema == null)) {
                    } else {
                        if ((compiler.getErrorCount()>0)) {
                            ((Saxon.Eej.ee.schema.sdoc.XSDInclude)(child)).error("Errors were found in the included schema document");
                        } else {
                            includedSchema.copyTo(schema, compiler);
                            if ((compiler.getErrorCount()>0)) {
                                ((Saxon.Eej.ee.schema.sdoc.XSDInclude)(child)).error("The included schema document resulted in conflicting components");
                            }
                        }
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_IMPORT:
                    Saxon.Eej.ee.schema.PreparedSchema importedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDImport)(child)).getImportedSchema();
                    if ((importedSchema != null)) {
                        importedSchema.copyTo(schema, compiler);
                        if ((compiler.getErrorCount()>0)) {
                            ((Saxon.Eej.ee.schema.sdoc.XSDImport)(child)).error("The imported schema document resulted in conflicting components");
                        }
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_REDEFINE:
                    Saxon.Eej.ee.schema.PreparedSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(child)).getRedefinedSchema();
                    if ((redefinedSchema != null)) {
                        redefinedSchema.copyTo(schema, compiler);
                        if ((compiler.getErrorCount()>0)) {
                            ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(child)).error("The redefined schema document resulted in conflicting components");
                        }
                    }
                    break ;
                }
            }
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            if ((targetNamespace == null)) {
                string tn = getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
                targetNamespace = ((tn == null) ? Saxon.Hej.om.NamespaceUri.NULL : Saxon.Hej.om.NamespaceUri.of(tn));
            }
            return targetNamespace;
        }
        public string getAttributeFormDefault() {
            return attributeFormDefault;
        }
        public int getBlockDefault() {
            return blockDefault;
        }
        public string getElementFormDefault() {
            return elementFormDefault;
        }
        public int getFinalDefault() {
            return finalDefault;
        }
        public Saxon.Eej.ee.schema.Wildcard getDefaultOpenContentWildcard() {
            return openContentWildcard;
        }
        public string getDefaultOpenContentMode() {
            return openContentMode;
        }
        public bool defaultOpenContentAppliesToEmpty() {
            return openContentAppliesToEmpty;
        }
        public void setAttributeFormDefault(string attributeFormDefault) {
            this.attributeFormDefault = attributeFormDefault;
        }
        public void setBlockDefault(int value) {
            blockDefault = value;
        }
        public void setElementFormDefault(string elementFormDefault) {
            this.elementFormDefault = elementFormDefault;
        }
        public void setFinalDefault(int value) {
            finalDefault = value;
        }
        public void setDefaultOpenContent(Saxon.Eej.ee.schema.Wildcard wildcard, string mode, bool appliesToEmpty) {
            openContentWildcard = wildcard;
            openContentMode = mode;
            openContentAppliesToEmpty = appliesToEmpty;
        }
        public string getXPathDefaultNamespace() {
            return xpathDefaultNamespace;
        }
        public Saxon.Eej.ee.schema.AttributeGroupReference getDefaultAttributeGroupReference() {
            if ((defaultAttributeGroupReference != null)) {
                return defaultAttributeGroupReference;
            }
            if ((defaultAttributeGroup == null)) {
                return null;
            }
            defaultAttributeGroupReference = new Saxon.Eej.ee.schema.AttributeGroupReference((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), defaultAttributeGroup);
            return defaultAttributeGroupReference;
        }
        public int getErrorCount() {
            return errorCount;
        }
        public void incrementErrorCount() {
            errorCount++;
        }
        public void addGlobalParam(Saxon.Hej.expr.instruct.GlobalParam param) {
            Saxon.Hej.expr.instruct.GlobalParam prev = Saxon.Impl.Helpers.MapUtils.Put(@params, param.getVariableQName(), param);
            if ((prev != null)) {
                error(("Duplicate declaration of parameter "+param.getVariableQName().getEQName()));
            }
        }
        public Saxon.Hej.expr.instruct.GlobalParam getGlobalParam(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(@params, name);
        }
        public System.Collections.Generic.ICollection<Saxon.Hej.expr.instruct.GlobalParam> getGlobalParams() {
            return @params.Values;
        }
    
    }
}