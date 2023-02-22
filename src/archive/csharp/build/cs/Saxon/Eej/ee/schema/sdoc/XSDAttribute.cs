using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAttribute : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.AttributeDecl attributeDecl = null;
        private Saxon.Eej.ee.schema.AttributeUse attributeUse = null;
        private bool foundAnnotation = false;
        private bool foundSimpleType = false;
        private bool typeSpecified = false;
        private bool isReference = false;
        public XSDAttribute()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (isTopLevel()) {
                string[] allowed = {"default", "fixed", "id", "inheritable", "name", "type"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "name");
            } else {
                string[] allowed = {"default", "fixed", "form", "id", "inheritable", "name", "ref", "targetNamespace", "type", "use"};
                allowAttributes(atts, allowed);
            }
            isReference = (atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "ref") != null);
            string form = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "form");
            if ((form != null)) {
                if (isReference) {
                    mutuallyExclusiveAttributes("form", "ref");
                }
                if ((form.Equals("qualified") || form.Equals("unqualified"))) {
                } else {
                    invalidAttributeValue("form", form, "Must be 'qualified' or 'unqualified'");
                    form = "qualified";
                }
            } else {
                if (isTopLevel()) {
                    form = "qualified";
                } else {
                    form = getXSDSchema().getAttributeFormDefault();
                    if ((form != null)) {
                    } else {
                        form = "unqualified";
                    }
                }
            }
            string localTargetNamespace = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
            if ((localTargetNamespace != null)) {
                if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                    error("The targetNamespace cannot be defined locally unless XSD 1.1 is enabled");
                }
                if ((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name") == null)) {
                    error("If there is no @name then there must be no @targetNamespace");
                }
                checkMutuallyExclusiveAttributes("targetNamespace", "form");
            }
            string attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name");
            Saxon.Hej.om.StructuredQName nc = null;
            if ((attValue != null)) {
                attributeDecl = new Saxon.Eej.ee.schema.AttributeDecl((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), isTopLevel());
                attributeDecl.setLocator(this);
                if (attValue.Equals("xmlns")) {
                    error("The attribute name 'xmlns' is reserved");
                }
                if ((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "ref") != null)) {
                    mutuallyExclusiveAttributes("name", "ref");
                    nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                } else {
                    try {
                        if (((localTargetNamespace != null) || "qualified".Equals(form))) {
                            if (getXSDSchema().getTargetNamespace().Equals(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE)) {
                                error("Attributes cannot be declared in the 'xsi:' namespace");
                            }
                            nc = getComponentName(attValue, REQUIRE_TARGET_NS);
                        } else {
                            nc = getComponentName(attValue, REQUIRE_NULL_NS);
                        }
                        attributeDecl.setAttributeName(nc);
                    } catch (Saxon.Hej.type.SchemaException err) {
                        error(err.Message);
                        nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                    }
                }
                attributeDecl.setAttributeName(nc);
                if (!(isTopLevel())) {
                    attributeUse = new Saxon.Eej.ee.schema.AttributeUse((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()));
                    attributeUse.setTargetComponentName(attributeDecl.getComponentName());
                    attributeUse.setTarget(attributeDecl);
                    attributeUse.setLocator(this);
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "ref");
            if ((attValue != null)) {
                attributeUse = new Saxon.Eej.ee.schema.AttributeUse((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()));
                attributeUse.setLocator(this);
                try {
                    nc = getComponentName(attValue, USE_DEFAULT_NS);
                    if (nc.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE)) {
                        getConfiguration().addSchemaForBuiltInNamespace(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE);
                    }
                } catch (Saxon.Hej.type.SchemaException err) {
                    nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                    error(err.Message);
                }
                attributeUse.setTargetComponentName(nc);
            }
            if ((nc == null)) {
                nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "default");
            if (((attValue != null) && !(nc.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE)))) {
                Saxon.Eej.ee.schema.ValueConstraint vc = new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT, Saxon.Hej.str.StringView.tidy(attValue));
                if ((attributeUse != null)) {
                    attributeUse.setValueConstraint(vc);
                } else {
                    if ((attributeDecl != null)) {
                        attributeDecl.setValueConstraint(vc);
                    }
                }
            }
            processId();
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "fixed");
            if ((attValue != null)) {
                if (((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "default") != null) && !(nc.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE)))) {
                    mutuallyExclusiveAttributes("fixed", "default");
                }
                Saxon.Eej.ee.schema.ValueConstraint vc = new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED, Saxon.Hej.str.StringView.tidy(attValue));
                if ((attributeUse != null)) {
                    attributeUse.setValueConstraint(vc);
                } else {
                    if ((attributeDecl != null)) {
                        attributeDecl.setValueConstraint(vc);
                    }
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "type");
            int typeFingerprint = -1;
            if ((attValue == null)) {
                if ((attributeDecl != null)) {
                    attributeDecl.setSimpleType(Saxon.Hej.type.AnySimpleType.getInstance());
                }
            } else {
                typeSpecified = true;
                if (isReference) {
                    mutuallyExclusiveAttributes("type", "ref");
                    return ;
                }
                try {
                    typeFingerprint = getFingerprint(attValue, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                checkPermittedType(typeFingerprint, attValue, "an attribute type");
                if ((attributeDecl != null)) {
                    attributeDecl.setTypeReference(new Saxon.Eej.ee.schema.TypeReference(typeFingerprint, getSchema().getConfiguration(), this));
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "use");
            if ((attValue != null)) {
                bool hasDefault = (atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "default") != null);
                if ((hasDefault && !(attValue.Equals("optional")))) {
                    error("When @default is present, the @use attribute must have the value \"optional\"");
                    attValue = "optional";
                }
                bool hasFixed = (atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "fixed") != null);
                if (((hasFixed && attValue.Equals("prohibited")) && (getXSDSchema().getSchemaCompiler().getLanguageVersion() != Saxon.Hej.Configuration.XSD10))) {
                    error("When @fixed is present, the @use attribute must not have the value \"prohibited\"");
                    attValue = "optional";
                }
                Saxon.Eej.ee.schema.Optionality useFlag;
                switch (attValue) {
                  case "required":
                    useFlag = Saxon.Eej.ee.schema.Optionality.REQUIRED;
                    break ;
                  case "optional":
                    useFlag = Saxon.Eej.ee.schema.Optionality.OPTIONAL;
                    break ;
                  case "prohibited":
                    useFlag = Saxon.Eej.ee.schema.Optionality.PROHIBITED;
                    break ;
                  default:
                    invalidAttributeValue("use", attValue, "Must be required|optional|prohibited");
                    useFlag = Saxon.Eej.ee.schema.Optionality.OPTIONAL;
                    break ;
                }
                if ((attributeUse != null)) {
                    attributeUse.setUse(useFlag);
                }
            }
            string inheritAtt = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "inheritable");
            if ((inheritAtt != null)) {
                if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                    error("The inheritable attribute cannot be used unless XSD 1.1 is enabled");
                }
                bool inherit = parseBooleanAttribute("inheritable", inheritAtt);
                if ((attributeUse != null)) {
                    attributeUse.setInheritable((inherit ? 1 : 0));
                }
                if ((attributeDecl != null)) {
                    attributeDecl.setInheritable(inherit);
                }
            }
            if (((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "fixed") != null) || (atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "default") != null))) {
                bool needResolver = true;
                if (typeSpecified) {
                    if ((typeFingerprint<1024)) {
                        Saxon.Hej.type.SchemaType type = Saxon.Hej.type.BuiltInType.getSchemaType(typeFingerprint);
                        if (((type is Saxon.Hej.type.SimpleType) && !(((Saxon.Hej.type.SimpleType)(type)).isNamespaceSensitive()))) {
                            needResolver = false;
                        }
                    }
                }
                if (needResolver) {
                    Saxon.Hej.om.NamespaceResolver resolver = makeNamespaceContext();
                    if ((attributeDecl != null)) {
                        attributeDecl.setNamespaceResolver(resolver);
                    }
                    if ((attributeUse != null)) {
                        attributeUse.setNamespaceResolver(resolver);
                    }
                }
            }
        }
        public Saxon.Eej.ee.schema.AttributeDecl getAttributeDeclaration() {
            return attributeDecl;
        }
        public Saxon.Eej.ee.schema.AttributeUse getAttributeUse() {
            return attributeUse;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            string localTargetNamespace = getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
            if (((localTargetNamespace != null) && !(Saxon.Hej.om.NamespaceUri.of(localTargetNamespace).Equals(getXSDSchema().getTargetNamespace())))) {
                checkLocalTargetNamespace("attribute");
            }
            if ((attributeDecl != null)) {
                Saxon.Hej.tree.iter.AxisIterator ancIter = iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR);
                Saxon.Hej.om.NodeInfo anc;
                while (((anc = ((/*1430*/Saxon.Hej.om.NodeInfo)ancIter.next())) != null)) {
                    if ((anc is Saxon.Eej.ee.schema.sdoc.XSDComplexType)) {
                        attributeDecl.setContainingComplexType(((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(anc)).getComplexType());
                        break ;
                    }
                }
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if ((fp == Saxon.Hej.om.StandardNames.XS_ANNOTATION)) {
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    if (foundSimpleType) {
                        mustBeFirstElement("annotation");
                    }
                    foundAnnotation = true;
                } else {
                    if ((fp == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE)) {
                        if (typeSpecified) {
                            mutuallyExclusiveElementAndAttribute("simpleType", "type");
                        }
                        if (isReference) {
                            mutuallyExclusiveElementAndAttribute("simpleType", "ref");
                        }
                        if (foundSimpleType) {
                            duplicateElement("simpleType");
                        }
                        foundSimpleType = true;
                    } else {
                        illegalElement(child);
                    }
                }
            }
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if (((fp == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE) && (attributeDecl != null))) {
                    Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition simpleTypeDef = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                    Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(simpleTypeDef.getFingerprint(), getSchema().getConfiguration(), this);
                    attributeDecl.setTypeReference(@ref);
                    simpleTypeDef.setContainingDeclaration(attributeDecl.getComponentName(), false);
                    simpleTypeDef.setContextComponent(attributeDecl);
                }
            }
        }
    
    }
}