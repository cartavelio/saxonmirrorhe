using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class IdentityConstraint : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private Saxon.Eej.ee.schema.IdentitySelector selector = null;
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.IdentityField> fields = null;
        private Saxon.Hej.om.StructuredQName constraintName;
        public abstract string getConstraintCategory();
        private bool ordered;
        protected internal IdentityConstraint()  {
            fields = new System.Collections.Generic.List<Saxon.Eej.ee.schema.IdentityField>(3);
        }
        public void addField(Saxon.Eej.ee.schema.IdentityField field) {
            if ((field != null)) {
                fields.Add(field);
            }
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.IdentityField> getFields() {
            return fields;
        }
        public string getName() {
            return constraintName.getLocalPart();
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return constraintName.getNamespaceUri();
        }
        public Saxon.Eej.ee.schema.IdentitySelector getSelector() {
            return selector;
        }
        public void setSelector(Saxon.Eej.ee.schema.IdentitySelector selector) {
            this.selector = selector;
        }
        public void setConstraintName(Saxon.Hej.om.StructuredQName nc) {
            constraintName = nc;
        }
        public Saxon.Hej.om.StructuredQName getConstraintName() {
            return constraintName;
        }
        public void setOrdered(bool ordered) {
            this.ordered = ordered;
        }
        public bool isOrdered() {
            return ordered;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            string err = null;
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(getName())))) {
                err = "The name of an IdentityConstraint must be an NCName.";
            } else {
                if ((selector == null)) {
                    err = "Selector for IdentityConstraint cannot be null.";
                } else {
                    if ((fields.Count<1)) {
                        err = "There must be at least one field in an identity constraint.";
                    }
                }
            }
            if ((err != null)) {
                compiler.error(err, this);
                return false;
            }
            return true;
        }
        public void typeCheck(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.type.SchemaType contextType) {
            Saxon.Eej.config.EnterpriseConfiguration config = compiler.getConfiguration();
            Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(config);
            env.setWarningHandler((message, code, loc) => compiler.warning(message, code, this));
            env.setSchemaAware(true);
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
            Saxon.Hej.type.ItemType contextItemType = new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, contextType, config, false);
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(contextItemType, true);
            selector.setSelectExpression(selector.getSelectExpression().typeCheck(visitor, cit));
            Saxon.Hej.type.ItemType selectorType = selector.getSelectExpression().getItemType();
            if (((selectorType is Saxon.Hej.pattern.NodeTest) && (((Saxon.Hej.pattern.NodeTest)(selectorType)).getContentType() != Saxon.Hej.type.AnyType.getInstance()))) {
                Saxon.Hej.expr.parser.ContextItemStaticInfo sit = config.makeContextItemStaticInfo(selectorType, false);
                foreach (Saxon.Eej.ee.schema.IdentityField field in fields) {
                    field.setSelectExpression(field.getSelectExpression().typeCheck(visitor, sit));
                    int card = field.getCardinality(visitor, sit);
                    if (Saxon.Hej.value.Cardinality.allowsMany(card)) {
                        compiler.warning((("The field expression ("+field.getXPath())+") may select multiple nodes, which would make the instance document invalid"), Saxon.Hej.trans.SaxonErrorCode.SXSD1004, field);
                    } else {
                        if (((this is Saxon.Eej.ee.schema.Key) && Saxon.Hej.value.Cardinality.allowsZero(card))) {
                            compiler.warning((("The field expression ("+field.getXPath())+") may select no nodes, which would make the instance document invalid"), Saxon.Hej.trans.SaxonErrorCode.SXSD1004, field);
                        }
                    }
                    Saxon.Hej.pattern.NodeTest fieldType = (Saxon.Hej.pattern.NodeTest)(field.getSelectExpression().getItemType());
                    Saxon.Hej.type.AtomicType atomizedType = ((/*1430*/Saxon.Hej.type.AtomicType)fieldType.getAtomizedItemType());
                    if ((atomizedType == null)) {
                        compiler.warning("The node selected by the field expression cannot be atomized", Saxon.Hej.trans.SaxonErrorCode.SXSD1005, field);
                    }
                    Saxon.Hej.type.SchemaType contentType = fieldType.getContentType();
                    if (!(((((contentType == Saxon.Hej.type.AnyType.getInstance()) || contentType.isSimpleType()) || (contentType.isComplexType() && ((Saxon.Hej.type.ComplexType)(contentType)).isSimpleContent()))))) {
                        compiler.warning("The node selected by the field expression must have a simple type, or a complex type with simple content", Saxon.Hej.trans.SaxonErrorCode.SXSD1005, field);
                    }
                    bool listValued = (contentType.isSimpleType() && ((Saxon.Hej.type.SimpleType)(contentType)).isListType());
                    if ((((atomizedType != null) && ((Saxon.Hej.type.ItemType)(atomizedType)).isAtomicType()) && (contentType != Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))) {
                        field.setBuiltInBaseType((Saxon.Hej.type.BuiltInAtomicType)(atomizedType.getBuiltInBaseType()), listValued);
                    }
                }
            }
        }
        public bool isSameDeclaration(Saxon.Eej.ee.schema.IdentityConstraint other) {
            return ((this == other) || ((getConstraintName().Equals(other.getConstraintName()) && hasSameLocation(other))));
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.setIsSerialized(this);
            string kind = getConstraintCategory();
            serializer.startElement(kind);
            serializer.emitAttribute("id", serializer.getId(this, false));
            serializer.emitAttribute("name", getName());
            if ((getTargetNamespace() != null)) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace().ToString());
            }
            if ((this is Saxon.Eej.ee.schema.KeyRef)) {
                Saxon.Eej.ee.schema.IdentityConstraint target = (Saxon.Eej.ee.schema.IdentityConstraint)(((Saxon.Eej.ee.schema.KeyRef)(this)).getTarget());
                serializer.emitAttribute("key", serializer.getId(target, false));
            }
            serializer.startElement("selector");
            serializer.emitNamespaceContext(selector.getNamespaceContext());
            serializer.emitAttribute("xpath", selector.getXPath());
            serializer.emitAttribute("defaultNamespace", selector.getXPathDefaultNamespace().ToString());
            serializer.endElement();
            foreach (Saxon.Eej.ee.schema.IdentityField f in fields) {
                serializer.startElement("field");
                serializer.emitNamespaceContext(f.getNamespaceContext());
                serializer.emitAttribute("xpath", f.getXPath());
                serializer.emitAttribute("defaultNamespace", f.getXPathDefaultNamespace().ToString());
                Saxon.Hej.type.BuiltInAtomicType baseType = f.getBuiltInBaseType();
                if ((baseType != null)) {
                    serializer.emitAttribute("type", (("#"+baseType.getName())+((f.isListValued() ? "*" : ""))));
                }
                serializer.endElement();
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.IdentityConstraint thisConstraint = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Identity-Constraint Definition");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.IdentityConstraint>(thisConstraint);
                  case "name":
                    return new Saxon.Hej.value.StringValue(getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                  case "target namespace":
                    return new Saxon.Hej.value.AnyURIValue((getTargetNamespace().toUnicodeString()));
                  case "identity-constraint category":
                    string kind = this.getConstraintCategory();
                    return new Saxon.Hej.value.StringValue(kind);
                  case "selector":
                    Saxon.Eej.ee.schema.IdentitySelector selector = getSelector();
                    return /*134*/Saxon.Eej.ee.schema.SchemaStructure.makeXPathExpressionPropertyRecord(selector.getXPath(), selector.getNamespaceContext(), selector.getSchemaDocumentURI());
                  case "fields":
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    foreach (Saxon.Eej.ee.schema.IdentityField field in getFields()) {
                        list.Add(/*134*/Saxon.Eej.ee.schema.SchemaStructure.makeXPathExpressionPropertyRecord(field.getXPath(), field.getNamespaceContext(), field.getSchemaDocumentURI()));
                    }
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                  case "referenced key":
                    if ((this is Saxon.Eej.ee.schema.KeyRef)) {
                        return ((Saxon.Eej.ee.schema.KeyRef)(this)).getTarget().getComponentAsFunction();
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}