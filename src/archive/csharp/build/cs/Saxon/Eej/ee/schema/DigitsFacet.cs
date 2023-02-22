using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class DigitsFacet : Saxon.Eej.ee.schema.Facet {
        private int requiredValue;
        public abstract void setValue(string value);
        public override Saxon.Hej.str.UnicodeString getValue() {
            return Saxon.Hej.str.BMPString.of((requiredValue+""));
        }
        public int getNumericValue() {
            return requiredValue;
        }
        public void setNumericValue(int value) {
            requiredValue = value;
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!((@base.isAtomicType()))) {
                throw new Saxon.Hej.type.SchemaException(((("The "+getName())+" facet is not available on the non-atomic type ")+type.getDescription()));
            }
            int primitive = ((Saxon.Hej.type.AtomicType)(@base)).getPrimitiveType();
            if (((primitive == Saxon.Hej.om.StandardNames.XS_INTEGER) || (primitive == Saxon.Hej.om.StandardNames.XS_DECIMAL))) {
            } else {
                throw new Saxon.Hej.type.SchemaException(((("The "+getName())+" facet is not allowed on a type derived from the type ")+@base.getDescription()));
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            if (!(((value is Saxon.Hej.value.DecimalValue)))) {
                return false;
            }
            string s = value.getStringValue();
            if ((s[0] == '-')) {
                s = s.Substring(1);
            }
            int before, after;
            int dot = s.IndexOf('.');
            if ((dot<0)) {
                before = s.Length;
                if ((s[0] == '0')) {
                    before--;
                }
                after = 0;
            } else {
                before = dot;
                after = ((s.Length-before)-1);
                if ((s[0] == '0')) {
                    before--;
                }
            }
            if ((this is Saxon.Eej.ee.schema.TotalDigitsFacet)) {
                return (((before+after))<=requiredValue);
            } else {
                return (after<=requiredValue);
            }
        }
        public override Saxon.Hej.om.FunctionItem getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.DigitsFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.DigitsFacet>(facet);
                  case "value":
                    Saxon.Hej.type.BuiltInAtomicType type = ((this is Saxon.Eej.ee.schema.TotalDigitsFacet) ? Saxon.Hej.type.BuiltInAtomicType.POSITIVE_INTEGER : Saxon.Hej.type.BuiltInAtomicType.NON_NEGATIVE_INTEGER);
                    return new Saxon.Hej.value.Int64Value(requiredValue, type, false);
                  case "fixed":
                    return Saxon.Hej.value.BooleanValue.get(isFixed());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}