using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class EmptyContentValidator : Saxon.Eej.ee.validate.AttributeValidator {
        private readonly bool allowTextNodes;
        private Saxon.Hej.s9api.Location locationId;
        private Saxon.Hej.str.UnicodeBuilder buffer;
        private readonly Saxon.Hej.type.SchemaType schemaType;
        public EmptyContentValidator(Saxon.Eej.ee.schema.UserComplexType type, Saxon.Hej.@event.Receiver next) : base (next) {
            schemaType = type;
            setAttributeGroup(type.getCombinedAttributeGroup());
            allowTextNodes = type.isMixedContent();
            setAnnotation(type);
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return schemaType;
        }
        protected internal override Saxon.Eej.ee.validate.ContentValidator getChildValidator() {
            return new Saxon.Eej.ee.validate.AnyTypeValidator(getNextReceiver());
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            this.locationId = location.saveLocation();
            string message = (((("Invalid child element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+", ")+getContainingElementName())+" has an empty content model");
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-complex-type", "2.1");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, true, location);
            buffer = null;
            base.startElement(elemName, Saxon.Hej.type.AnyType.getInstance(), attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkNoCharactersWhenNil(locationId);
            if (allowTextNodes) {
                Saxon.Eej.ee.schema.ElementDecl decl = getElementDeclaration();
                if (((decl != null) && (((decl.getFixedValue() != null) || (decl.getDefaultValueLexicalForm() != null))))) {
                    if ((buffer == null)) {
                        buffer = new Saxon.Hej.str.UnicodeBuilder();
                    }
                    buffer.accept(chars);
                }
            } else {
                string message = (("Character data is not allowed: "+getContainingElementName())+" has an empty content model");
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-complex-type", "2.1");
                ve.setSchemaType(getSchemaType());
                reportValidationError(ve, true, locationId);
            }
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void endElement() {
            if (isNilled()) {
                Saxon.Eej.ee.schema.ElementDecl decl = getElementDeclaration();
                if (((decl != null) && (decl.getFixedValue() != null))) {
                    string message = "An element with xsi:nil='true' must not have a fixed value";
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-elt", "3.2.2");
                    ve.setSchemaType(getSchemaType());
                    reportValidationError(ve, true, locationId);
                }
            }
            if (allowTextNodes) {
                Saxon.Eej.ee.schema.ElementDecl decl = getElementDeclaration();
                if ((decl != null)) {
                    Saxon.Hej.str.UnicodeString defaultValue = decl.getDefaultValueLexicalForm();
                    Saxon.Hej.str.UnicodeString fixedValue = decl.getFixedValueLexicalForm();
                    if (((defaultValue != null) && (((buffer == null) || buffer.isEmpty())))) {
                        characters(defaultValue, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    } else {
                        if ((fixedValue != null)) {
                            Saxon.Hej.str.UnicodeBuilder fsb = buffer;
                            if (((fsb == null) || fsb.isEmpty())) {
                                characters(fixedValue, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                            } else {
                                if (!(fsb.toUnicodeString().Equals(fixedValue))) {
                                    string message = ((((("The content "+Saxon.Hej.trans.Err.wrap(buffer.ToString(), Saxon.Hej.trans.Err.VALUE))+" of ")+getContainingElementName())+" does not match the fixed value ")+Saxon.Hej.trans.Err.wrap(fixedValue, Saxon.Hej.trans.Err.VALUE));
                                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                                    ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                                    ve.setSchemaType(getSchemaType());
                                    reportValidationError(ve, true, getContainingElementLocationId());
                                }
                            }
                        }
                    }
                }
            }
            base.endElement();
        }
    
    }
}