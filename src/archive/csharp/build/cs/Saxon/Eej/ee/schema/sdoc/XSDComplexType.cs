using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDComplexType : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        internal Saxon.Ejava.util.Optional<bool> mixed = Saxon.Ejava.util.Optional<bool>.empty();
        internal bool defaultAttributesApply = true;
        private bool isRedefinition;
        private Saxon.Eej.ee.schema.UserComplexType complexType = null;
        public XSDComplexType()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Eej.ee.schema.PreparedSchema schema = getSchema();
            complexType = new Saxon.Eej.ee.schema.UserComplexType(schema.getConfiguration(), this);
            complexType.setSchemaDocumentURI(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getSystemId());
            isRedefinition = ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDRedefine));
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (isTopLevel()) {
                string[] allowed = {"abstract", "block", "defaultAttributesApply", "final", "id", "mixed", "name"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "name");
            } else {
                string[] allowed = {"defaultAttributesApply", "id", "mixed"};
                allowAttributes(atts, allowed);
            }
            string debug = atts.getValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "debug");
            if ((debug != null)) {
                debug = Saxon.Hej.value.Whitespace.trim(debug);
                if (("true".Equals(debug) || "1".Equals(debug))) {
                    complexType.setSaxonDebug(true);
                }
            }
            processId();
            string attValue = Saxon.Hej.value.Whitespace.trim(atts.getValue("", "name"));
            if ((attValue != null)) {
                Saxon.Hej.om.StructuredQName typeName;
                try {
                    typeName = getComponentName(attValue, REQUIRE_TARGET_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    typeName = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                    attValue = ("invalid-complex-type-definition-"+GetHashCode());
                    error(err.Message);
                }
                int fp = getNamePool().allocateFingerprint(typeName.getURI(), typeName.getLocalPart());
                complexType.setTypeName(typeName, fp);
                if (!(isRedefinition)) {
                    if (((schema.getSchemaType(complexType.getTypeName()) != null) && (fp>1023))) {
                        duplicate("type", attValue);
                    }
                }
            } else {
                Saxon.Hej.om.NodeName nc = allocateAnonymousTypeCode();
                complexType.setTypeName(nc.getStructuredQName(), nc.getFingerprint());
                schema.addAnonymousType(complexType);
            }
            string mixedAtt = Saxon.Hej.value.Whitespace.trim(atts.getValue("", "mixed"));
            if ((mixedAtt != null)) {
                mixed = Saxon.Ejava.util.Optional<bool>.of(parseBooleanAttribute("mixed", mixedAtt));
            }
            attValue = atts.getValue("", "abstract");
            if ((attValue != null)) {
                bool isAbstract = parseBooleanAttribute("abstract", attValue);
                complexType.setAbstract(isAbstract);
            }
            string blockVal = atts.getValue("", "block");
            if ((blockVal == null)) {
                complexType.setFinalProhibitions(getXSDSchema().getBlockDefault());
            } else {
                int f = parseFinalOrBlock("block", blockVal, (Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION));
                complexType.setBlock(f);
            }
            string finalVal = atts.getValue("", "final");
            if ((finalVal == null)) {
                complexType.setFinalProhibitions(getXSDSchema().getFinalDefault());
            } else {
                int f = parseFinalOrBlock("final", finalVal, (Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION));
                complexType.setFinalProhibitions(f);
            }
            string applyAtt = Saxon.Hej.value.Whitespace.trim(atts.getValue("", "defaultAttributesApply"));
            if ((applyAtt != null)) {
                if ((applyAtt.Equals("true") || applyAtt.Equals("1"))) {
                    defaultAttributesApply = true;
                } else {
                    if ((applyAtt.Equals("false") || applyAtt.Equals("0"))) {
                        defaultAttributesApply = false;
                    } else {
                        invalidAttributeValue("defaultAttributesApply", applyAtt, "Must be true or false");
                    }
                }
            }
        }
        public Saxon.Eej.ee.schema.UserComplexType getComplexType() {
            return complexType;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
            bool foundSimpleContent = false;
            bool foundComplexContent = false;
            bool thisMixed = (mixed.isPresent() && mixed.get());
            complexType.setVariety((thisMixed ? Saxon.Hej.type.ComplexVariety.MIXED : Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY));
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if ((fp == Saxon.Hej.om.StandardNames.XS_SIMPLE_CONTENT)) {
                    foundSimpleContent = true;
                    complexType.setVariety(Saxon.Hej.type.ComplexVariety.SIMPLE);
                    if ((thisMixed && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD11))) {
                        error("The <complexType> must not specify mixed='true' when <simpleContent> is present");
                    }
                } else {
                    if ((fp == Saxon.Hej.om.StandardNames.XS_COMPLEX_CONTENT)) {
                        foundComplexContent = true;
                        Saxon.Ejava.util.Optional<bool> ccMixed = ((Saxon.Eej.ee.schema.sdoc.XSDComplexContent)(child)).getMixedAttribute();
                        if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD11)) {
                            if (((mixed.isPresent() && ccMixed.isPresent()) && !(mixed.Equals(ccMixed)))) {
                                error("If mixed is specified on both <complexType> and <complexContent>, they must agree");
                            }
                        }
                        bool eitherMixed = (thisMixed || ((ccMixed.isPresent() && ccMixed.get())));
                        complexType.setVariety((eitherMixed ? Saxon.Hej.type.ComplexVariety.MIXED : Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY));
                    } else {
                        if ((fp == Saxon.Hej.om.StandardNames.XS_ASSERT)) {
                            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                                error("Assertions require XSD 1.1 to be enabled");
                            }
                        } else {
                            if ((fp == Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT)) {
                                if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                                    error("Open content requires XSD 1.1 to be enabled");
                                }
                            }
                        }
                    }
                }
            }
            if ((isRedefinition && !(((foundSimpleContent || foundComplexContent))))) {
                error("A redefined complex type must be derived by extension or restriction");
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_CHOICE, Saxon.Hej.om.StandardNames.XS_SEQUENCE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT, Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_SIMPLE_CONTENT, Saxon.Hej.om.StandardNames.XS_ALL, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_COMPLEX_CONTENT};
        private readonly static int[] state0targets = {3, 3, 7, 5, 6, 1, 4, 2, 3, 1, 3, 2};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state1targets = {1, 1, 4, 7};
        private readonly static int[] state2terms = {};
        private readonly static int[] state2targets = {};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state3targets = {1, 1, 4, 7};
        private readonly static int[] state4terms = {Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state4targets = {7};
        private readonly static int[] state5terms = {Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_CHOICE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ALL, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_SEQUENCE, Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state5targets = {1, 3, 3, 1, 3, 4, 3, 7};
        private readonly static int[] state6terms = {Saxon.Hej.om.StandardNames.XS_CHOICE, Saxon.Hej.om.StandardNames.XS_SEQUENCE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_SIMPLE_CONTENT, Saxon.Hej.om.StandardNames.XS_ALL, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_COMPLEX_CONTENT};
        private readonly static int[] state6targets = {3, 3, 7, 5, 1, 4, 2, 3, 1, 3, 2};
        private readonly static int[] state7terms = {Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state7targets = {7};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms, state4terms, state5terms, state6terms, state7terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets, state4targets, state5targets, state6targets, state7targets};
        protected internal override bool isFinalState(int state) {
            return true;
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (isRedefinition) {
                Saxon.Eej.ee.schema.PreparedSchema externalSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchema();
                Saxon.Eej.ee.schema.PreparedSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getRedefinedSchema();
                if ((complexType.getBaseTypeFingerprint() != complexType.getFingerprint())) {
                    error("A redefined complex type must be based on the type with the same name");
                }
                if ((externalSchema == null)) {
                    return ;
                }
                Saxon.Eej.ee.schema.UserDefinedType oldType = (Saxon.Eej.ee.schema.UserDefinedType)(externalSchema.getSchemaType(complexType.getTypeName()));
                if ((oldType == null)) {
                    error((("Cannot redefine complex type "+Saxon.Hej.trans.Err.wrap(complexType.getName()))+" because it is not present in the redefined schema"));
                    return ;
                } else {
                    if ((oldType is Saxon.Hej.type.SimpleType)) {
                        error((("Cannot redefine a simple type "+Saxon.Hej.trans.Err.wrap(complexType.getName()))+"as a complex type"));
                    }
                }
                if (!(((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchemaDocumentURI().Equals(oldType.getSchemaDocumentURI()))) {
                    compiler.warning((("The redefined complex type was found, but not in the schema document referenced by "+"the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD ")+"specification. However, Saxon does not currently enforce this rule."), this);
                }
                complexType.setRedefinitionLevel((oldType.getRedefinitionLevel()+1));
                string uri = getXSDSchema().getTargetNamespace();
                Saxon.Hej.om.NamePool pool = getNamePool();
                string localName = pool.getLocalName(complexType.getFingerprint());
                string newLocalName = ((localName+'_')+complexType.GetHashCode());
                string prefix = complexType.getTypeName().getPrefix();
                int newFingerprint = pool.allocateFingerprint(uri, newLocalName);
                oldType.setTypeName(new Saxon.Hej.om.StructuredQName(prefix, uri, newLocalName), newFingerprint);
                Saxon.Eej.ee.schema.TypeReference baseType = new Saxon.Eej.ee.schema.TypeReference(newFingerprint, complexType.getConfiguration(), this);
                complexType.setBaseTypeReference(baseType);
                redefinedSchema.addType(oldType);
                redefinedSchema.addType(complexType);
                if (complexType.isSimpleContent()) {
                    Saxon.Eej.ee.schema.TypeReference simpleTypeRef = complexType.getSimpleContentTypeReference();
                    Saxon.Hej.type.SimpleType simpleType = (Saxon.Hej.type.SimpleType)(simpleTypeRef.getTarget());
                    if ((simpleType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                        ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(simpleType)).setBaseTypeReference(baseType);
                        redefinedSchema.addType(simpleType);
                    }
                }
            } else {
                if ((complexType.getBaseTypeReference() == null)) {
                    Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ANY_TYPE, complexType.getConfiguration(), this);
                    @ref.setTarget(Saxon.Hej.type.AnyType.getInstance());
                    complexType.setBaseTypeReference(@ref);
                }
                if (((complexType.getBaseTypeFingerprint() == complexType.getFingerprint()) && (complexType.getFingerprint() != Saxon.Hej.om.StandardNames.XS_ANY_TYPE))) {
                    error("A complex type must not be based on itself");
                }
            }
            assembleParticles();
        }
        private void assembleParticles() {
            Saxon.Eej.ee.schema.Wildcard openContentWildcard = null;
            string openContentMode = null;
            Saxon.Eej.ee.schema.AttributeGroupReference defaultAttributes;
            Saxon.Eej.ee.schema.sdoc.XSDSchema schemaDefiningDefaults;
            if ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDOverride)) {
                Saxon.Eej.ee.schema.sdoc.SchemaDocument original = ((Saxon.Eej.ee.schema.sdoc.XSDOverride)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getOriginalSchemaDocument();
                schemaDefiningDefaults = original.getXSDSchema();
            } else {
                schemaDefiningDefaults = getXSDSchema();
            }
            defaultAttributes = schemaDefiningDefaults.getDefaultAttributeGroupReference();
            if (((defaultAttributes != null) && defaultAttributesApply)) {
                complexType.addAttributeGroupReference(defaultAttributes);
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE:
                    {
                        Saxon.Eej.ee.schema.AttributeWildcard wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAnyAttribute)(child)).getWildcard();
                        complexType.setAnyAttribute(wildcard);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                    {
                        Saxon.Eej.ee.schema.AttributeUse attrDecl = ((Saxon.Eej.ee.schema.sdoc.XSDAttribute)(child)).getAttributeUse();
                        complexType.addAttributeUse(attrDecl);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    {
                        Saxon.Eej.ee.schema.AttributeGroupReference attrGroupRef = ((Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup)(child)).getAttributeGroupReference();
                        complexType.addAttributeGroupReference(attrGroupRef);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                    {
                        Saxon.Eej.ee.schema.ElementParticle element = ((Saxon.Eej.ee.schema.sdoc.XSDElement)(child)).getElementParticle();
                        complexType.setParticle(element);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    {
                        Saxon.Eej.ee.schema.ModelGroupParticle modelGroup = ((Saxon.Eej.ee.schema.sdoc.XSDGroup)(child)).getGroupReference();
                        complexType.setParticle(modelGroup);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_CONTENT:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_CONTENT:
                    {
                        openContentWildcard = complexType.getOpenContentWildcard();
                        openContentMode = complexType.getOpenContentModeString();
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                  case Saxon.Hej.om.StandardNames.XS_CHOICE:
                  case Saxon.Hej.om.StandardNames.XS_ALL:
                    {
                        complexType.setParticle(((Saxon.Eej.ee.schema.sdoc.XSDCompositor)(child)).getModelGroupParticle());
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ASSERT:
                    {
                        complexType.addAssertion(((Saxon.Eej.ee.schema.sdoc.XSDAssert)(child)).getAssertion());
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT:
                    {
                        Saxon.Eej.ee.schema.ElementWildcard ew = ((Saxon.Eej.ee.schema.sdoc.XSDOpenContent)(child)).getElementWildcard();
                        openContentWildcard = (((ew == null) ? null : ew.getWildcard()));
                        openContentMode = ((Saxon.Eej.ee.schema.sdoc.XSDOpenContent)(child)).getMode();
                        break ;
                    }
                }
            }
            if ((openContentWildcard != null)) {
                complexType.setOpenContentWildcard(openContentWildcard, openContentMode, true);
            } else {
                if (((schemaDefiningDefaults.getDefaultOpenContentWildcard() != null) && !("none".Equals(openContentMode)))) {
                    complexType.setOpenContentWildcard(schemaDefiningDefaults.getDefaultOpenContentWildcard(), schemaDefiningDefaults.getDefaultOpenContentMode(), schemaDefiningDefaults.defaultOpenContentAppliesToEmpty());
                }
            }
        }
    
    }
}