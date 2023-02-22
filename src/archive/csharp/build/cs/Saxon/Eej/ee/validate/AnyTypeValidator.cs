using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class AnyTypeValidator : Saxon.Eej.ee.validate.LaxValidator {
        internal Saxon.Hej.str.UnicodeString fixedValue = null;
        internal Saxon.Hej.str.UnicodeString defaultValue = null;
        internal bool foundChildren = false;
        internal Saxon.Hej.str.UnicodeBuilder buffer;
        public AnyTypeValidator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public AnyTypeValidator(Saxon.Eej.ee.schema.ElementDecl elementDecl, Saxon.Hej.@event.Receiver next) : base (next) {
            if ((elementDecl != null)) {
                Saxon.Hej.om.AtomicSequence @fixed = elementDecl.getFixedValue();
                if ((@fixed != null)) {
                    fixedValue = ((Saxon.Hej.value.AtomicValue)(@fixed)).getUnicodeStringValue();
                }
                defaultValue = elementDecl.getDefaultValueLexicalForm();
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (locallyInvalid) {
                childValidator = new Saxon.Eej.ee.validate.SkipValidator(nextReceiver);
                nextReceiver.startElement(elemName, Saxon.Hej.type.AnyType.getInstance(), attributes, namespaces, location, properties);
                return ;
            }
            if ((fixedValue != null)) {
                string message = (((("Element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" cannot appear here, because the containing ")+getContainingElementName())+" element has fixed content");
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "5.2.2.1");
                ve.setSchemaType(getSchemaType());
                reportValidationError(ve, true, location);
            }
            foundChildren = true;
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((fixedValue != null) || (defaultValue != null))) {
                if ((buffer == null)) {
                    buffer = new Saxon.Hej.str.UnicodeBuilder();
                }
                buffer.accept(chars);
            }
            base.characters(chars, locationId, properties);
        }
        public override void endElement() {
            if ((fixedValue != null)) {
                if (((buffer == null) || buffer.isEmpty())) {
                    characters(fixedValue, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                } else {
                    if (!(buffer.toUnicodeString().Equals(fixedValue))) {
                        string message = ((((("The content of "+getContainingElementName())+" differs from the fixed value defined in the schema. Fixed value is ")+Saxon.Hej.trans.Err.wrap(fixedValue, Saxon.Hej.trans.Err.VALUE))+", actual value is ")+Saxon.Hej.trans.Err.wrap(buffer.ToString(), Saxon.Hej.trans.Err.VALUE));
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                        ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                        ve.setSchemaType(getSchemaType());
                        reportValidationError(ve, true, getContainingElementLocationId());
                    }
                }
            }
            if (((defaultValue != null) && !(foundChildren))) {
                if (((buffer == null) || buffer.isEmpty())) {
                    characters(defaultValue, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
            }
            base.endElement();
        }
    
    }
}