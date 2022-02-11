using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class LengthRangeFacet : Saxon.Eej.ee.schema.Facet {
        internal int requiredLength;
        public override Saxon.Hej.str.UnicodeString getValue() {
            return Saxon.Hej.str.BMPString.of((""+requiredLength));
        }
        public override long toLong() {
            return requiredLength;
        }
        public void setNumericValue(int value) {
            if ((value<0)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("length facet < 0");
            }
            requiredLength = value;
        }
        public void setValue(string value) {
            try {
                requiredLength = System.Int32.Parse(value);
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                throw new Saxon.Hej.type.SchemaException((((("Value of "+getName())+" facet ")+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" is not an integer"));
            }
            if ((requiredLength<0)) {
                throw new Saxon.Hej.type.SchemaException((((("Value of "+getName())+" facet ")+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" must be a positive integer"));
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            if ((value is Saxon.Hej.value.StringValue)) {
                return testLength(((Saxon.Hej.value.StringValue)(value)).length32());
            } else {
                if ((value is Saxon.Hej.value.HexBinaryValue)) {
                    return testLength(((Saxon.Hej.value.HexBinaryValue)(value)).getLengthInOctets());
                } else {
                    if ((value is Saxon.Hej.value.Base64BinaryValue)) {
                        return testLength(((Saxon.Hej.value.Base64BinaryValue)(value)).getLengthInOctets());
                    } else {
                        if ((value is Saxon.Hej.value.QualifiedNameValue)) {
                            return true;
                        } else {
                            throw new Saxon.Ejava.lang.AssertionError("Length facet found on unexpected type");
                        }
                    }
                }
            }
        }
        public abstract override bool testLength(int actualLength);
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Hej.expr.Callable callable = new Inner_Callable_d108e373(this);
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
        private class Inner_Callable_d108e373 : Saxon.Hej.expr.Callable {
            private readonly LengthRangeFacet outerInstance;
            public Inner_Callable_d108e373(LengthRangeFacet outerInstance) {
                this.outerInstance = outerInstance;
            }
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                string key = /*localparam*/arguments[0].head().getStringValue();
                switch (/*localvar*/key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(/*guess*/outerInstance.getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.LengthRangeFacet>(this.outerInstance);
                  case "value":
                    return new Saxon.Hej.value.Int64Value(outerInstance.requiredLength, Saxon.Hej.type.BuiltInAtomicType.NON_NEGATIVE_INTEGER, false);
                  case "fixed":
                    return Saxon.Hej.value.BooleanValue.get(/*guess*/outerInstance.isFixed());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
        }

    }
}