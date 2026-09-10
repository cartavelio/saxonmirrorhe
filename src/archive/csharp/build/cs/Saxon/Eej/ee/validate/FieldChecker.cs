using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class FieldChecker : Saxon.Eej.ee.validate.AbstractReceiver {
        private readonly Saxon.Eej.ee.validate.SelectorWatch parentWatch;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        private readonly Saxon.Hej.om.AtomicSequence[] currentRow;
        private readonly int column;
        private readonly bool matchElements;
        private bool active = false;
        private bool nillableElement = false;
        private Saxon.Hej.om.NodeName elementName;
        private Saxon.Hej.type.SchemaType elementType;
        private readonly Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
        private Saxon.Hej.s9api.Location textLocationId = null;
        internal int depth = 0;
        public FieldChecker(Saxon.Eej.ee.validate.SelectorWatch parentWatch, Saxon.Hej.pattern.Pattern selection, Saxon.Hej.om.AtomicSequence[] currentRow, int column)  {
            this.parentWatch = parentWatch;
            this.currentRow = currentRow;
            this.column = column;
            matchElements = selection.getUType().overlaps(Saxon.Hej.type.UType.ELEMENT);
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            namespaceResolver = resolver;
        }
        public override void startElement(Saxon.Hej.om.NodeName nameCode, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nillableElement = false;
            if (matchElements) {
                if ((depth>0)) {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("An element participating as a field in a constraint cannot have child elements");
                    ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                    throw ve.makeException();
                }
                elementType = typeCode;
                elementName = nameCode;
                active = true;
                buffer.clear();
            }
            depth++;
        }
        public void notifySelectedAttribute(Saxon.Eej.ee.stream.om.FleetingNode attribute, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.om.AtomicSequence val;
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.lib.ConversionRules rules = config.getConversionRules();
            Saxon.Hej.type.SimpleType type = (Saxon.Hej.type.SimpleType)(attribute.getSchemaType());
            Saxon.Hej.str.UnicodeString value = attribute.getUnicodeStringValue();
            if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
            } else {
                try {
                    if (type.isNamespaceSensitive()) {
                        try {
                            val = type.getTypedValue(value, namespaceResolver, rules);
                        } catch (Saxon.Hej.type.ValidationException e) {
                            val = null;
                        }
                    } else {
                        if (type.isAtomicType()) {
                            Saxon.Hej.type.AtomicType prim = (Saxon.Hej.type.AtomicType)(((Saxon.Hej.type.AtomicType)(type)).getPrimitiveItemType());
                            Saxon.Hej.type.StringConverter converter = prim.getStringConverter(getConfiguration().getConversionRules());
                            Saxon.Hej.type.ConversionResult result = converter.convertString(value);
                            if ((result is Saxon.Hej.type.ValidationFailure)) {
                                val = null;
                            } else {
                                val = result.asAtomic();
                            }
                            if ((type is Saxon.Eej.ee.schema.UserAtomicType)) {
                                Saxon.Eej.ee.schema.EnumerationFacetSet en = (Saxon.Eej.ee.schema.EnumerationFacetSet)(((Saxon.Eej.ee.schema.UserAtomicType)(type)).getFacet(typeof(Saxon.Eej.ee.schema.EnumerationFacetSet)));
                                if ((en != null)) {
                                    val = en.getSharedInstance(val);
                                }
                            }
                        } else {
                            try {
                                val = type.getTypedValue(value, namespaceResolver, rules);
                            } catch (Saxon.Hej.type.ValidationException e) {
                                val = null;
                            }
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.type.ValidationException(e);
                }
                if ((val != null)) {
                    parentWatch.addFieldValue(currentRow, column, val, false, locationId);
                }
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((active && matchElements) && (depth == 1))) {
                buffer.accept(chars);
            }
            nillableElement = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NILLABLE_ELEMENT);
            textLocationId = locationId;
        }
        public override void endElement() {
            if (((active && matchElements) && (depth == 1))) {
                Saxon.Hej.om.AtomicSequence val;
                Saxon.Hej.type.SchemaType eltype = elementType;
                Saxon.Hej.str.UnicodeString fieldVal = buffer.toUnicodeString();
                if (((eltype == null) || (eltype is Saxon.Hej.type.AnySimpleType))) {
                    val = Saxon.Hej.value.StringValue.makeUntypedAtomic(fieldVal);
                } else {
                    Saxon.Hej.type.SchemaType contentType = eltype;
                    if ((eltype is Saxon.Hej.type.ComplexType)) {
                        if (((Saxon.Hej.type.ComplexType)(eltype)).isSimpleContent()) {
                            contentType = ((Saxon.Hej.type.ComplexType)(eltype)).getSimpleContentType();
                        } else {
                            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((((("The value of a field participating in an identity constraint must have a simple type,"+" or a complex type with simple content. Element ")+elementName.getDisplayName())+" has type ")+eltype.getDescription()));
                            err.setConstraintReference(1, "cvc-identity-constraint", "3");
                            throw err.makeException();
                        }
                    }
                    System.Diagnostics.Debug.Assert((contentType != null));
                    if ((contentType.getFingerprint() == Saxon.Hej.om.StandardNames.XS_STRING)) {
                        val = Saxon.Hej.value.StringValue.makeStringValue(buffer.ToString());
                    } else {
                        if (((Saxon.Hej.type.SimpleType)(contentType)).isNamespaceSensitive()) {
                            try {
                                val = ((Saxon.Hej.type.SimpleType)(contentType)).getTypedValue(fieldVal, namespaceResolver, getConfiguration().getConversionRules());
                            } catch (Saxon.Hej.type.ValidationException e) {
                                val = null;
                            }
                        } else {
                            if (contentType.isAtomicType()) {
                                Saxon.Hej.type.AtomicType prim = (Saxon.Hej.type.AtomicType)(((Saxon.Hej.type.AtomicType)(contentType)).getPrimitiveItemType());
                                Saxon.Hej.type.StringConverter converter = prim.getStringConverter(getConfiguration().getConversionRules());
                                Saxon.Hej.type.ConversionResult result = converter.convertString(fieldVal);
                                if ((result is Saxon.Hej.type.ValidationFailure)) {
                                    val = null;
                                } else {
                                    val = result.asAtomic();
                                }
                                if ((contentType is Saxon.Eej.ee.schema.UserAtomicType)) {
                                    Saxon.Eej.ee.schema.EnumerationFacetSet en = (Saxon.Eej.ee.schema.EnumerationFacetSet)(((Saxon.Eej.ee.schema.UserAtomicType)(contentType)).getFacet(typeof(Saxon.Eej.ee.schema.EnumerationFacetSet)));
                                    if ((en != null)) {
                                        val = en.getSharedInstance(val);
                                    }
                                }
                            } else {
                                try {
                                    val = ((Saxon.Hej.type.SimpleType)(contentType)).getTypedValue(fieldVal, namespaceResolver, getConfiguration().getConversionRules());
                                } catch (Saxon.Hej.type.ValidationException e) {
                                    val = null;
                                }
                            }
                        }
                    }
                }
                if ((val != null)) {
                    parentWatch.addFieldValue(currentRow, column, val, nillableElement, textLocationId);
                }
            }
            depth--;
        }
        public override void close() {
            active = false;
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
    
    }
}