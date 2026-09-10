using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class SimpleContentValidator : Saxon.Eej.ee.validate.AttributeValidator {
        private Saxon.Hej.type.SchemaType schemaType;
        private Saxon.Hej.type.SimpleType simpleType;
        private Saxon.Eej.ee.schema.ValueConstraint fixedValue = null;
        private Saxon.Hej.str.UnicodeString fixedValueLexicalForm = null;
        private Saxon.Eej.ee.schema.ValueConstraint defaultValue = null;
        private bool needToValidateConstraint = false;
        private readonly Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
        public SimpleContentValidator(Saxon.Eej.ee.schema.ElementDecl declaration, Saxon.Hej.type.SchemaType type, Saxon.Hej.@event.Receiver next) : base (next) {
            setSchemaType(type);
            if ((declaration != null)) {
                defaultValue = declaration.getDefaultValueConstraint();
                fixedValue = declaration.getFixedValueConstraint();
                if ((fixedValue != null)) {
                    fixedValueLexicalForm = fixedValue.getLexicalForm();
                }
                setNillability((declaration.isNillable() ? Saxon.Eej.ee.validate.ContentValidator.Nillability.ALLOWED : Saxon.Eej.ee.validate.ContentValidator.Nillability.DISALLOWED));
                needToValidateConstraint = (type != declaration.getType());
            } else {
                needToValidateConstraint = false;
            }
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return schemaType;
        }
        public void setSchemaType(Saxon.Hej.type.SchemaType type) {
            schemaType = type;
            if (type.isSimpleType()) {
                simpleType = (Saxon.Hej.type.SimpleType)(type);
            } else {
                Saxon.Eej.ee.schema.UserComplexType ctype = (Saxon.Eej.ee.schema.UserComplexType)(type);
                simpleType = ctype.getSimpleContentType();
                setAttributeGroup(ctype.getCombinedAttributeGroup());
            }
            setAnnotation(schemaType);
            needToValidateConstraint = true;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            string message = (((("Invalid child element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+", ")+getContainingElementName())+" must have simple content");
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-type", "3.1.2");
            ve.setSchemaType(schemaType);
            reportValidationError(ve, true, location);
            base.startElement(elemName, Saxon.Hej.type.AnyType.getInstance(), attributes, namespaces, location, properties);
        }
        protected internal override Saxon.Eej.ee.validate.ContentValidator getChildValidator() {
            return new Saxon.Eej.ee.validate.AnyTypeValidator(getNextReceiver());
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkNoCharactersWhenNil(locationId);
            buffer.accept(chars);
            if ((getNillability() == Saxon.Eej.ee.validate.ContentValidator.Nillability.ALLOWED)) {
                properties |= (Saxon.Hej.@event.ReceiverOption.NILLABLE_ELEMENT);
            }
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void endElement() {
            if (isNilled()) {
                if (!(buffer.isEmpty())) {
                    string message = (("The "+getContainingElementName())+" has xsi:nil='true' so it cannot have character content");
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setSchemaType(schemaType);
                    ve.setConstraintReference(1, "cvc-elt", "3.2.1");
                    reportValidationError(ve, true, getContainingElementLocationId());
                    base.endElement();
                    return ;
                }
                if ((fixedValue != null)) {
                    string message = (("The "+getContainingElementName())+" has a fixed value so it must not be nil");
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-elt", "3.2.2");
                    ve.setSchemaType(schemaType);
                    reportValidationError(ve, true, getContainingElementLocationId());
                    base.endElement();
                    return ;
                }
            } else {
                if (((defaultValue != null) && buffer.isEmpty())) {
                    if (needToValidateConstraint) {
                        Saxon.Hej.type.ValidationFailure err = simpleType.validateContent(defaultValue.getLexicalForm(), getNamespaceResolver(), getConfiguration().getConversionRules());
                        if ((err != null)) {
                            string message = ((((((("The default value "+Saxon.Hej.trans.Err.wrap(defaultValue.getLexicalForm(), Saxon.Hej.trans.Err.VALUE))+" for ")+getContainingElementName())+" does not match the type ")+simpleType.getDescription())+": ")+err.getMessage());
                            err.setMessage(message);
                            err.setSchemaType(simpleType);
                            err.setConstraintReference(2, "cvc-datatype-valid", "1");
                            reportValidationError(err, true, getContainingElementLocationId());
                            base.endElement();
                            return ;
                        }
                    }
                    characters(defaultValue.getLexicalForm(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.DEFAULTED_VALUE);
                } else {
                    if ((fixedValue != null)) {
                        if (buffer.isEmpty()) {
                            if (needToValidateConstraint) {
                                Saxon.Hej.type.ValidationFailure err = simpleType.validateContent(fixedValueLexicalForm, getNamespaceResolver(), getConfiguration().getConversionRules());
                                if ((err != null)) {
                                    string message = ((((((("The fixed value "+Saxon.Hej.trans.Err.wrap(fixedValueLexicalForm, Saxon.Hej.trans.Err.VALUE))+" for ")+getContainingElementName())+" does not match the type ")+simpleType.getDescription())+": ")+err.getMessage());
                                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                                    ve.setSchemaType(simpleType);
                                    ve.setConstraintReference(2, "cvc-datatype-valid", "1");
                                    reportValidationError(ve, true, getContainingElementLocationId());
                                    base.endElement();
                                    return ;
                                }
                            }
                            characters(fixedValueLexicalForm, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                        } else {
                            Saxon.Hej.om.AtomicSequence actual = simpleType.getTypedValue(buffer.toUnicodeString(), getNamespaceResolver(), getConfiguration().getConversionRules());
                            if (!(fixedValue.matches(actual))) {
                                string message = ((((("The content "+Saxon.Hej.trans.Err.wrap(buffer.ToString(), Saxon.Hej.trans.Err.VALUE))+" of ")+getContainingElementName())+" does not match the fixed value ")+Saxon.Hej.trans.Err.wrap(fixedValueLexicalForm, Saxon.Hej.trans.Err.VALUE));
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                                ve.setSchemaType(simpleType);
                                ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                                reportValidationError(ve, true, getContainingElementLocationId());
                            }
                        }
                    } else {
                        Saxon.Hej.lib.ConversionRules rules = getValidationContext();
                        Saxon.Hej.type.ValidationFailure err = simpleType.validateContent(buffer.toUnicodeString(), getNamespaceResolver(), rules);
                        if ((err != null)) {
                            string message = ((((("The content "+Saxon.Hej.trans.Err.wrap(buffer.ToString(), Saxon.Hej.trans.Err.VALUE))+" of ")+getContainingElementName())+" does not match the required simple type. ")+err.getMessage());
                            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                            ve.setSchemaType(simpleType);
                            ve.setConstraintReference(2, "cvc-datatype-valid", "1");
                            reportValidationError(ve, true, getContainingElementLocationId());
                            base.endElement();
                            return ;
                        }
                    }
                }
            }
            base.endElement();
        }
    
    }
}