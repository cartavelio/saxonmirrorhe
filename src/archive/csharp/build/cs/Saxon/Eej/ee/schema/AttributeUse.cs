using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public sealed class AttributeUse : Saxon.Eej.ee.schema.SimpleComponentReference, Saxon.Eej.ee.schema.ComponentWithValueConstraint, Saxon.Eej.ee.schema.UserSchemaComponent {
        private Saxon.Eej.ee.schema.Optionality useFlag = Saxon.Eej.ee.schema.Optionality.OPTIONAL;
        private Saxon.Eej.ee.schema.ValueConstraint valueConstraint;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        private Saxon.Eej.ee.schema.AttributeGroupDecl containingAttributeGroupDecl;
        private int inheritable = -1;
        public AttributeUse(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
            symbolSpace = Saxon.Eej.ee.schema.SymbolSpace.ATTRIBUTE;
        }
        public override void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            base.tryToResolve(compiler, fatal);
            if (isResolved()) {
                Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(target);
                if (!(decl.isGlobal())) {
                    decl.setContainingAttributeGroup(getContainingAttributeGroupDecl());
                }
            } else {
                string name = getTargetComponentName().getEQName();
                if ((compiler.isAllowDanglingReferences() && !(fatal))) {
                    compiler.warning((("Unknown attribute declaration "+name)+" (substituting dummy declaration)"), Saxon.Hej.trans.SaxonErrorCode.SXSD1001, this);
                    Saxon.Eej.ee.schema.AttributeDecl decl = new Saxon.Eej.ee.schema.AttributeDecl(compiler.getConfiguration(), false);
                    decl.setAttributeName(getTargetComponentName());
                    decl.setSimpleType(Saxon.Hej.type.BuiltInAtomicType.STRING);
                    target = decl;
                } else {
                    throw new Saxon.Hej.type.SchemaException(("Unknown attribute declaration "+name), this);
                }
            }
        }
        public Saxon.Eej.ee.schema.AttributeDecl getAttributeDeclaration() {
            return (Saxon.Eej.ee.schema.AttributeDecl)(getTarget());
        }
        public Saxon.Eej.ee.schema.AttributeGroupDecl getContainingAttributeGroupDecl() {
            return containingAttributeGroupDecl;
        }
        public void setContainingAttributeGroupDecl(Saxon.Eej.ee.schema.AttributeGroupDecl containingAttributeGroupDecl) {
            this.containingAttributeGroupDecl = containingAttributeGroupDecl;
        }
        public string getDisplayName() {
            return getTargetComponentName().getDisplayName();
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceResolver;
        }
        public string getUse() {
            switch (useFlag.innerEnumValue) {
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.PROHIBITED:
                return "prohibited";
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.REQUIRED:
                return "required";
              default:
                return "optional";
            }
        }
        public int getCardinality() {
            switch (useFlag.innerEnumValue) {
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.PROHIBITED:
                return Saxon.Hej.expr.StaticProperty.EMPTY;
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.REQUIRED:
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
              default:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
        }
        public Saxon.Eej.ee.schema.ValueConstraint getValueConstraint() {
            return valueConstraint;
        }
        public Saxon.Hej.str.UnicodeString getUnderlyingDefaultValue() {
            if ((getDefaultValueLexicalForm() == null)) {
                Saxon.Eej.ee.schema.AttributeDecl declaration = (Saxon.Eej.ee.schema.AttributeDecl)(getTarget());
                if ((declaration != null)) {
                    return declaration.getDefaultValueLexicalForm();
                } else {
                    return null;
                }
            } else {
                return getDefaultValueLexicalForm();
            }
        }
        public Saxon.Hej.om.AtomicSequence getUnderlyingFixedValue() {
            if ((getFixedValue() == null)) {
                Saxon.Eej.ee.schema.AttributeDecl declaration = (Saxon.Eej.ee.schema.AttributeDecl)(getTarget());
                if ((declaration != null)) {
                    return declaration.getFixedValue();
                } else {
                    return null;
                }
            } else {
                return getFixedValue();
            }
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            namespaceResolver = resolver;
        }
        public bool isOptional() {
            return (useFlag == Saxon.Eej.ee.schema.Optionality.OPTIONAL);
        }
        public bool isProhibited() {
            return (useFlag == Saxon.Eej.ee.schema.Optionality.PROHIBITED);
        }
        public bool isRequired() {
            return (useFlag == Saxon.Eej.ee.schema.Optionality.REQUIRED);
        }
        public void setUse(Saxon.Eej.ee.schema.Optionality value) {
            useFlag = value;
        }
        public void setValueConstraint(Saxon.Eej.ee.schema.ValueConstraint vc) {
            this.valueConstraint = vc;
        }
        public void setInheritable(int inherit) {
            this.inheritable = inherit;
        }
        public bool isInheritable() {
            switch (inheritable) {
              case 0:
                return false;
              case 1:
                return true;
              case -1:
              default:
                return ((Saxon.Eej.ee.schema.AttributeDecl)(getTarget())).isInheritable();
            }
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            /*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(this, compiler, false);
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result;
            tryToResolve(compiler, false);
            if (!(isResolved())) {
                return false;
            }
            Saxon.Eej.ee.schema.AttributeDecl declaration = (Saxon.Eej.ee.schema.AttributeDecl)(getTarget());
            result = declaration.validate(compiler);
            if (!(result)) {
                return false;
            }
            Saxon.Hej.type.SimpleType stype = ((/*1430*/Saxon.Hej.type.SimpleType)declaration.getType());
            Saxon.Eej.config.EnterpriseConfiguration config = compiler.getConfiguration();
            if ((((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10) && (((getFixedValue() != null) || (getDefaultValueLexicalForm() != null)))) && (stype.getBuiltInBaseType().getFingerprint() == Saxon.Hej.om.StandardNames.XS_ID))) {
                compiler.error("An attribute of type xs:ID must have no fixed or default value", this);
                return false;
            }
            if ((valueConstraint != null)) {
                bool ok = valueConstraint.makeTypedValue(compiler, stype, namespaceResolver, this);
                if (!(ok)) {
                    return false;
                }
            }
            Saxon.Hej.om.AtomicSequence fixedVal = declaration.getFixedValue();
            if ((fixedVal != null)) {
                if ((getDefaultValueLexicalForm() != null)) {
                    string err = (("An attribute reference "+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" cannot have a default value if the attribute declaration has a fixed value");
                    compiler.error(err, this);
                    return false;
                }
                Saxon.Hej.om.AtomicSequence thisFixedValue = getFixedValue();
                if (((thisFixedValue != null) && !(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(thisFixedValue, fixedVal)))) {
                    string err = (("The attribute reference "+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" and the corresponding declaration have different fixed values");
                    compiler.error(err, this);
                    return false;
                }
            }
            return true;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            if (!(isProhibited())) {
                serializer.startElement("attributeUse");
                serializer.emitAttribute("required", (isRequired() ? "true" : "false"));
                serializer.emitAttribute("inheritable", (isInheritable() ? "true" : "false"));
                serializer.emitAttribute("ref", serializer.getId(getAttributeDeclaration(), false));
                if ((getDefaultValueLexicalForm() != null)) {
                    serializer.emitAttribute("default", getDefaultValueLexicalForm().ToString());
                }
                if ((getValueConstraint() != null)) {
                    getValueConstraint().serialize(serializer);
                }
                serializer.endElement();
            }
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.AttributeUse thisAttributeUse = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Attribute Use");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.AttributeUse>(thisAttributeUse);
                  case "required":
                    return Saxon.Hej.value.BooleanValue.get(isRequired());
                  case "attribute declaration":
                    return getAttributeDeclaration().getComponentAsFunction();
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