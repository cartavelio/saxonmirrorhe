using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class LaxValidator : Saxon.Eej.ee.validate.ContentValidator {
        public LaxValidator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return null;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            int fp = elemName.obtainFingerprint(getNamePool());
            Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getConfiguration().getElementDeclaration(fp));
            makeChildValidator(decl, elemName.getStructuredQName(), location, Saxon.Hej.lib.Validation.LAX);
            checkNoChildrenWhenNil(location);
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> attList = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes.size());
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                attList.Add(processAttribute(att));
            }
            Saxon.Hej.om.AttributeMap validatedAtts = Saxon.Hej.om.SequenceTool.attributeMapFromList(attList);
            type = childValidator.getAnnotation();
            base.startElement(elemName, type, validatedAtts, namespaces, location, properties);
        }
        protected internal override Saxon.Eej.ee.validate.ContentValidator getChildValidator() {
            return childValidator;
        }
        protected internal Saxon.Hej.om.AttributeInfo processAttribute(Saxon.Hej.om.AttributeInfo att) {
            string value = att.getValue();
            Saxon.Hej.type.SimpleType annotation = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(getConfiguration().getAttributeDeclaration(att.getNodeName().getStructuredQName()));
            if ((decl != null)) {
                Saxon.Hej.type.SimpleType type = decl.getSimpleType();
                Saxon.Hej.type.ValidationFailure err = type.validateContent(Saxon.Hej.str.StringView.of(att.getValue()).tidy(), getNamespaceResolver(), getConfiguration().getConversionRules());
                if ((err != null)) {
                    err.setConstraintReference(1, "cvc-complex-type", "3");
                    err.setSchemaType(type);
                    reportValidationError(err, false, att.getLocation());
                } else {
                    if ((type is Saxon.Eej.ee.schema.UserAtomicType)) {
                        value = ((Saxon.Eej.ee.schema.UserAtomicType)(type)).getSharedInstance(value);
                    }
                    annotation = type;
                    Saxon.Eej.ee.schema.ValueConstraint constraint = decl.getFixedValueConstraint();
                    if (((constraint != null) && !(constraint.testFixedValue(getConfiguration(), Saxon.Hej.str.StringView.of(value).tidy(), type, getNamespaceResolver())))) {
                        Saxon.Hej.om.AtomicSequence @fixed = decl.getFixedValue();
                        string message = ((((("The actual value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" of attribute ")+Saxon.Hej.trans.Err.wrap(att.getNodeName().getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" does not match the required fixed value ")+(((@fixed is Saxon.Hej.value.AtomicValue) ? Saxon.Hej.trans.Err.wrap(@fixed.getUnicodeStringValue(), Saxon.Hej.trans.Err.VALUE) : "")));
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                        ve.setConstraintReference(1, "cvc-au", "0");
                        ve.setSchemaType(type);
                        reportValidationError(ve, false, att.getLocation());
                    }
                }
            }
            return new Saxon.Hej.om.AttributeInfo(att.getNodeName(), annotation, value, att.getLocation(), att.getProperties());
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkNoCharactersWhenNil(locationId);
            base.characters(chars, locationId, properties);
        }
    
    }
}