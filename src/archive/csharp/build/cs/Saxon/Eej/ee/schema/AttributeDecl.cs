using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public sealed class AttributeDecl : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.ComponentWithValueConstraint, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent, Saxon.Hej.type.SchemaDeclaration {
        private Saxon.Eej.ee.schema.UserComplexType containingComplexType = null;
        private Saxon.Eej.ee.schema.AttributeGroupDecl containingAttributeGroup = null;
        private Saxon.Eej.ee.schema.ValueConstraint valueConstraint;
        private readonly bool global;
        private Saxon.Hej.om.StructuredQName attributeName;
        private int fingerprint;
        private Saxon.Eej.ee.schema.TypeReference simpleTypeRef = null;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        private bool inheritable;
        public AttributeDecl(Saxon.Eej.config.EnterpriseConfiguration config, bool global)  {
            this.global = global;
            setConfiguration(config);
        }
        public bool isGlobal() {
            return global;
        }
        public void setContainingComplexType(Saxon.Eej.ee.schema.UserComplexType type) {
            containingComplexType = type;
        }
        public Saxon.Eej.ee.schema.UserComplexType getContainingComplexType() {
            return containingComplexType;
        }
        public void setContainingAttributeGroup(Saxon.Eej.ee.schema.AttributeGroupDecl type) {
            containingAttributeGroup = type;
        }
        public Saxon.Eej.ee.schema.AttributeGroupDecl getContainingAttributeGroup() {
            return containingAttributeGroup;
        }
        public string getName() {
            return attributeName.getLocalPart();
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return attributeName.getNamespaceUri();
        }
        public string getDisplayName() {
            return attributeName.getDisplayName();
        }
        public void setAttributeName(Saxon.Hej.om.StructuredQName attributeName, int fingerprint) {
            this.attributeName = attributeName;
            this.fingerprint = fingerprint;
        }
        public void setAttributeName(Saxon.Hej.om.StructuredQName attributeName) {
            this.attributeName = attributeName;
            this.fingerprint = getConfiguration().getNamePool().allocateFingerprint(attributeName.getNamespaceUri(), attributeName.getLocalPart());
        }
        public Saxon.Hej.om.StructuredQName getComponentName() {
            return attributeName;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceResolver;
        }
        public void setTypeReference(Saxon.Eej.ee.schema.TypeReference type) {
            simpleTypeRef = type;
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public Saxon.Hej.type.SimpleType getSimpleType() {
            return (Saxon.Hej.type.SimpleType)(simpleTypeRef.getTarget());
        }
        public Saxon.Hej.type.SchemaType getType() {
            return getSimpleType();
        }
        public Saxon.Eej.ee.schema.ValueConstraint getValueConstraint() {
            return valueConstraint;
        }
        public void setSimpleType(Saxon.Hej.type.SimpleType simpleType) {
            if ((simpleTypeRef != null)) {
                simpleTypeRef.setTarget(simpleType);
            } else {
                simpleTypeRef = new Saxon.Eej.ee.schema.TypeReference(simpleType.getFingerprint(), getConfiguration(), null);
                simpleTypeRef.setTarget(simpleType);
            }
        }
        public void setValueConstraint(Saxon.Eej.ee.schema.ValueConstraint vc) {
            this.valueConstraint = vc;
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            namespaceResolver = resolver;
        }
        public Saxon.Hej.pattern.NodeTest makeSchemaNodeTest() {
            return new Saxon.Eej.ee.validate.SchemaAttributeTest(this);
        }
        public void setInheritable(bool inherit) {
            this.inheritable = inherit;
        }
        public bool isInheritable() {
            return inheritable;
        }
        public bool hasTypeAlternatives() {
            return false;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                Saxon.Hej.type.SchemaType decl = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(simpleTypeRef, compiler, false));
                if (((decl is Saxon.Eej.ee.schema.UserSchemaComponent) && (decl.getValidationStatus() != /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                    bool b = ((Saxon.Eej.ee.schema.UserSchemaComponent)(decl)).fixup(compiler);
                    if (!(b)) {
                        decl = null;
                    }
                }
                if (((decl == null) || (decl.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            }
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            bool result = true;
            Saxon.Hej.type.SchemaType type = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(simpleTypeRef, compiler, false));
            if ((type == null)) {
                return true;
            }
            if (type.isComplexType()) {
                string err = (("The complex type "+type.getDescription())+" cannot be used as the type of an attribute");
                compiler.error(err, this);
                return false;
            }
            Saxon.Hej.type.SimpleType simpleType = (Saxon.Hej.type.SimpleType)(type);
            if ((simpleType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                simpleType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(simpleType)).getWorkingType();
                simpleTypeRef.setTarget(simpleType);
            }
            Saxon.Hej.type.SimpleType stype = simpleType;
            if ((stype == null)) {
                string err = (("The type of attribute "+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" has not been specified");
                compiler.error(err, this);
                return result = false;
            }
            if ((stype is Saxon.Eej.ee.schema.UserSimpleType)) {
                result = ((Saxon.Eej.ee.schema.UserSimpleType)(stype)).validate(compiler);
            }
            if ((getFingerprint() == Saxon.Hej.om.StandardNames.XML_ID)) {
                if ((stype.getFingerprint() != Saxon.Hej.om.StandardNames.XS_ID)) {
                    compiler.error("If the xml:id attribute is used, it must be of type xs:ID", this);
                    return false;
                }
            }
            if ((((result && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) && (getValueConstraint() != null)) && (stype.getBuiltInBaseType().getFingerprint() == Saxon.Hej.om.StandardNames.XS_ID))) {
                compiler.error("An attribute of type xs:ID must have no fixed or default value", this);
                return false;
            }
            if ((valueConstraint != null)) {
                result = valueConstraint.makeTypedValue(compiler, stype, getNamespaceResolver(), this);
            }
            return result;
        }
        public bool isSameDeclaration(Saxon.Eej.ee.schema.AttributeDecl other) {
            return (((this == other) || (((getFingerprint() == other.getFingerprint()) && hasSameLocation(other)))) || getTargetNamespace().Equals(Saxon.Hej.om.NamespaceUri.XML));
        }
        public bool isNillable() {
            return false;
        }
        public bool isAbstract() {
            return false;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("attribute");
            serializer.emitAttribute("id", id);
            if ((getName() != null)) {
                serializer.emitAttribute("name", getName());
                if (((getTargetNamespace() != null) && !(getTargetNamespace().isEmpty()))) {
                    serializer.emitAttribute("targetNamespace", getTargetNamespace().ToString());
                }
            }
            Saxon.Hej.type.SimpleType type = ((/*1430*/Saxon.Hej.type.SimpleType)getType());
            serializer.emitAttribute("type", serializer.getTypeLink(type));
            serializer.emitAttribute("global", (isGlobal() ? "true" : "false"));
            serializer.emitAttribute("inheritable", (isInheritable() ? "true" : "false"));
            if ((!(isGlobal()) && (getContainingComplexType() != null))) {
                serializer.emitAttribute("containingComplexType", serializer.getId(getContainingComplexType(), false));
            }
            if ((getDefaultValueLexicalForm() != null)) {
                serializer.emitAttribute("default", getDefaultValueLexicalForm().ToString());
            }
            if ((getValueConstraint() != null)) {
                getValueConstraint().serialize(serializer);
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.AttributeDecl thisAttributeDecl = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Attribute Declaration");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.AttributeDecl>(thisAttributeDecl);
                  case "name":
                    return new Saxon.Hej.value.StringValue(getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                  case "target namespace":
                    return new Saxon.Hej.value.AnyURIValue((getTargetNamespace().toUnicodeString()));
                  case "type definition":
                    return ((/*1430*/Saxon.Hej.type.SimpleType)getType()).getComponentAsFunction();
                  case "scope":
                    Saxon.Hej.expr.CallableDelegate scopeFunction = new Saxon.Hej.expr.CallableDelegate((context1, arguments1) => {
                        string key1 = arguments1[0].head().getStringValue();
                        switch (key1) {
                          case "class":
                            return Saxon.Hej.value.StringValue.bmp("Scope");
                          case "variety":
                            return Saxon.Hej.value.StringValue.bmp((isGlobal() ? "global" : "local"));
                          case "parent":
                            if (isGlobal()) {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            } else {
                                if ((getContainingComplexType() != null)) {
                                    return getContainingComplexType().getComponentAsFunction();
                                } else {
                                    if ((getContainingAttributeGroup() != null)) {
                                        return getContainingAttributeGroup().getComponentAsFunction();
                                    } else {
                                        return Saxon.Hej.value.EmptySequence.getInstance();
                                    }
                                }
                            }
                          default:
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        }
                    } );
                    return new Saxon.Hej.functions.CallableFunction(1, scopeFunction, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                  case "value constraint":
                    return getValueConstraintAsFunction(this);
                  case "inheritable":
                    return Saxon.Hej.value.BooleanValue.get(isInheritable());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
        /* Expanding default method getFixedValueConstraint from interface ComponentWithValueConstraint */
        public Saxon.Eej.ee.schema.ValueConstraint getFixedValueConstraint() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getValueConstraint();
            if (((vc != null) && (vc.getVariety() == Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED))) {
                return vc;
            } else {
                return null;
            }
        }

        /* Expanding default method getDefaultValueConstraint from interface ComponentWithValueConstraint */
        public Saxon.Eej.ee.schema.ValueConstraint getDefaultValueConstraint() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getValueConstraint();
            if (((vc != null) && (vc.getVariety() == Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT))) {
                return vc;
            } else {
                return null;
            }
        }

        /* Expanding default method getValueConstraintAsFunction from interface ComponentWithValueConstraint */
        public Saxon.Hej.om.Sequence getValueConstraintAsFunction(Saxon.Eej.ee.schema.ComponentWithValueConstraint att) {
            if ((getValueConstraint() != null)) {
                return getValueConstraint().asFunction();
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getFixedValueLexicalForm from interface ComponentWithValueConstraint */
        public Saxon.Hej.str.UnicodeString getFixedValueLexicalForm() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getFixedValueConstraint();
            return ((vc == null) ? null : vc.getLexicalForm());
        }

        /* Expanding default method getFixedValue from interface ComponentWithValueConstraint */
        public Saxon.Hej.om.AtomicSequence getFixedValue() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getFixedValueConstraint();
            return ((vc == null) ? null : vc.getValue());
        }

        /* Expanding default method getDefaultValueLexicalForm from interface ComponentWithValueConstraint */
        public Saxon.Hej.str.UnicodeString getDefaultValueLexicalForm() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getDefaultValueConstraint();
            return ((vc == null) ? null : vc.getLexicalForm());
        }

        /* Expanding default method getDefaultValue from interface ComponentWithValueConstraint */
        public Saxon.Hej.om.AtomicSequence getDefaultValue() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getDefaultValueConstraint();
            return ((vc == null) ? null : vc.getValue());
        }

    }
}