using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class AttributeCreator : Saxon.Hej.expr.instruct.SimpleNodeConstructor, Saxon.Hej.expr.instruct.ValidatingInstruction {
        internal Saxon.Hej.type.SimpleType schemaType = null;
        private int validationAction;
        private int options = Saxon.Hej.@event.ReceiverOption.NONE;
        private bool _isInstruction;
        public void setInstruction(bool inst) {
            _isInstruction = inst;
        }
        public override bool isInstruction() {
            return _isInstruction;
        }
        public void setSchemaType(Saxon.Hej.type.SimpleType type) {
            schemaType = type;
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return schemaType;
        }
        public void setValidationAction(int action) {
            validationAction = action;
        }
        public int getValidationAction() {
            return validationAction;
        }
        public void setOptions(int options) {
            this.options = options;
        }
        public virtual void setRejectDuplicates() {
            options |= Saxon.Hej.@event.ReceiverOption.REJECT_DUPLICATES;
        }
        public void setNoSpecialChars() {
            options |= Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS;
        }
        public int getOptions() {
            return options;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            if ((getValidationAction() == Saxon.Hej.lib.Validation.SKIP)) {
                p |= Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED;
            }
            return p;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
        }
        public override void processValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeName attName = evaluateNodeName(context);
            int opt = getOptions();
            Saxon.Hej.type.SimpleType ann = validate(attName, value, context);
            if (attName.Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME)) {
                value = Saxon.Hej.value.Whitespace.collapseWhitespace(value);
            }
            try {
                output.attribute(attName, ann, value.ToString(), getLocation(), opt);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw dynamicError(getLocation(), err, context);
            }
        }
        protected internal Saxon.Hej.type.SimpleType validate(Saxon.Hej.om.NodeName attName, Saxon.Hej.str.UnicodeString value, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.SimpleType ann;
            Saxon.Hej.type.SimpleType schemaType = ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType());
            int validationAction = getValidationAction();
            if ((schemaType != null)) {
                ann = schemaType;
                Saxon.Hej.type.ValidationFailure err = schemaType.validateContent(value, Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), context.getConfiguration().getConversionRules());
                if ((err != null)) {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((((("Attribute value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not match the required type ")+schemaType.getDescription())+". ")+err.getMessage()));
                    ve.setSchemaType(schemaType);
                    ve.setErrorCode("XTTE1540");
                    throw ve.makeException();
                }
            } else {
                if (((validationAction == Saxon.Hej.lib.Validation.STRICT) || (validationAction == Saxon.Hej.lib.Validation.LAX))) {
                    try {
                        Saxon.Hej.Configuration config = context.getConfiguration();
                        ann = config.validateAttribute(attName.getStructuredQName(), value, validationAction);
                    } catch (Saxon.Hej.type.ValidationException e) {
                        Saxon.Hej.trans.XPathException err = Saxon.Hej.trans.XPathException.makeXPathException(e);
                        err.maybeSetErrorCode(((validationAction == Saxon.Hej.lib.Validation.STRICT) ? "XTTE1510" : "XTTE1515"));
                        err.setXPathContext(context);
                        err.maybeSetLocation(getLocation());
                        err.setIsTypeError(true);
                        throw err;
                    }
                } else {
                    ann = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                }
            }
            return ann;
        }
        protected internal void validateOrphanAttribute(Saxon.Hej.tree.util.Orphan orphan, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.type.SimpleType schemaType = ((/*1430*/Saxon.Hej.type.SimpleType)getSchemaType());
            int validationAction = getValidationAction();
            if ((schemaType != null)) {
                Saxon.Hej.type.ValidationFailure err = schemaType.validateContent(orphan.getUnicodeStringValue(), Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), rules);
                if ((err != null)) {
                    err.setMessage(((((("Attribute value "+Saxon.Hej.trans.Err.wrap(orphan.getUnicodeStringValue(), Saxon.Hej.trans.Err.VALUE))+" does not the match the required type ")+schemaType.getDescription())+". ")+err.getMessage()));
                    err.setErrorCode("XTTE1555");
                    err.setLocator(getLocation());
                    throw err.makeException();
                }
                orphan.setTypeAnnotation(schemaType);
                if (schemaType.isNamespaceSensitive()) {
                    throw new Saxon.Hej.trans.XPathException("Cannot validate a parentless attribute whose content is namespace-sensitive", "XTTE1545");
                }
            } else {
                if (((validationAction == Saxon.Hej.lib.Validation.STRICT) || (validationAction == Saxon.Hej.lib.Validation.LAX))) {
                    try {
                        Saxon.Hej.Controller controller = context.getController();
                        System.Diagnostics.Debug.Assert((controller != null));
                        Saxon.Hej.type.SimpleType ann = controller.getConfiguration().validateAttribute(Saxon.Hej.om.NameOfNode.makeName(orphan).getStructuredQName(), orphan.getUnicodeStringValue(), validationAction);
                        orphan.setTypeAnnotation(ann);
                    } catch (Saxon.Hej.type.ValidationException e) {
                        Saxon.Hej.trans.XPathException err = Saxon.Hej.trans.XPathException.makeXPathException(e);
                        err.setErrorCodeQName(e.getErrorCodeQName());
                        err.setXPathContext(context);
                        err.setLocation(getLocation());
                        err.setIsTypeError(true);
                        throw err;
                    }
                }
            }
        }
    
    }
}