using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class ValueRangeFacet : Saxon.Eej.ee.schema.Facet {
        internal bool isConverted = false;
        internal Saxon.Hej.value.AtomicValue requiredValue;
        public override Saxon.Hej.str.UnicodeString getValue() {
            return requiredValue.getUnicodeStringValue();
        }
        public void setValue(string value) {
            requiredValue = Saxon.Hej.value.StringValue.makeStringValue(value);
        }
        public void setRequiredValue(Saxon.Hej.value.AtomicValue value) {
            requiredValue = value;
            isConverted = true;
        }
        public Saxon.Hej.value.AtomicValue getRequiredValue() {
            return requiredValue;
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!((@base.isAtomicType()))) {
                throw new Saxon.Hej.type.SchemaException(((("The "+getName())+" facet is not available on the non-atomic type ")+@base.getDescription()));
            }
            int primitive;
            if ((@base is Saxon.Eej.ee.schema.UserAtomicType)) {
                primitive = ((Saxon.Eej.ee.schema.UserAtomicType)(@base)).getPrimitiveType();
            } else {
                if ((@base is Saxon.Hej.type.BuiltInAtomicType)) {
                    primitive = ((Saxon.Hej.type.BuiltInAtomicType)(@base)).getPrimitiveType();
                } else {
                    if ((@base is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                        if ((((@base.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) || ((((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)))) {
                            return ;
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException();
                        }
                    } else {
                        throw new Saxon.Ejava.lang.IllegalStateException(("Unknown atomic type representation: "+@base.GetType()));
                    }
                }
            }
            bool OK = (((((((((((((((primitive == Saxon.Hej.om.StandardNames.XS_INTEGER) || (primitive == Saxon.Hej.om.StandardNames.XS_DOUBLE)) || (primitive == Saxon.Hej.om.StandardNames.XS_FLOAT)) || (primitive == Saxon.Hej.om.StandardNames.XS_DECIMAL)) || (primitive == Saxon.Hej.om.StandardNames.XS_DURATION)) || (primitive == Saxon.Hej.om.StandardNames.XS_DATE)) || (primitive == Saxon.Hej.om.StandardNames.XS_DATE_TIME)) || (primitive == Saxon.Hej.om.StandardNames.XS_TIME)) || (primitive == Saxon.Hej.om.StandardNames.XS_G_YEAR)) || (primitive == Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH)) || (primitive == Saxon.Hej.om.StandardNames.XS_G_MONTH)) || (primitive == Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY)) || (primitive == Saxon.Hej.om.StandardNames.XS_G_DAY)) || (primitive == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION)) || (primitive == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION));
            if (!(OK)) {
                throw new Saxon.Hej.type.SchemaException(((("The "+getName())+" facet is not allowed on the unordered type ")+@base.getDescription()));
            }
            Saxon.Eej.config.EnterpriseConfiguration config = type.getConfiguration();
            try {
                convertToTargetType((Saxon.Hej.type.AtomicType)(type.getBaseType()), config);
            } catch (Saxon.Hej.type.SchemaException e1) {
                bool ok;
                try {
                    convertToTargetType((Saxon.Hej.type.AtomicType)(type.getBuiltInBaseType()), config);
                    ok = true;
                } catch (Saxon.Hej.type.SchemaException e2) {
                    ok = false;
                }
                if (!(ok)) {
                    throw;
                }
            }
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                Saxon.Eej.ee.schema.UserSimpleType ubase = (Saxon.Eej.ee.schema.UserSimpleType)(@base);
                Saxon.Eej.ee.schema.Facet baseFacet = ubase.getFacet(this.GetType());
                if (((baseFacet != null) && baseFacet.isFixed())) {
                    if (!(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(requiredValue, ((Saxon.Eej.ee.schema.ValueRangeFacet)(baseFacet)).requiredValue))) {
                        throw new Saxon.Hej.type.SchemaException((("The "+getName())+" facet is fixed in the base type and cannot be varied"));
                    }
                }
                Saxon.Hej.type.ValidationFailure err = ubase.checkAgainstFacets(requiredValue, null, ubase.getExtendedFacetList(), null);
                if ((err != null)) {
                    if (getName().EndsWith("Exclusive")) {
                        if ((baseFacet != null)) {
                            ((Saxon.Eej.ee.schema.ValueRangeFacet)(baseFacet)).convertToTargetType(@base, config);
                            if (Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(requiredValue, ((Saxon.Eej.ee.schema.ValueRangeFacet)(baseFacet)).requiredValue)) {
                                err = null;
                            }
                        }
                    }
                    if ((err != null)) {
                        throw new Saxon.Hej.type.SchemaException((("In a type derived by restriction, the "+getName())+" facet is not a valid value of the base type"));
                    }
                }
            } else {
                Saxon.Hej.type.BuiltInAtomicType bbase = (Saxon.Hej.type.BuiltInAtomicType)(@base);
                bool ok = true;
                if ((requiredValue is Saxon.Hej.value.Int64Value)) {
                    long v = ((Saxon.Hej.value.Int64Value)(requiredValue)).longValue();
                    ok = Saxon.Hej.value.IntegerValue.checkRange(v, bbase);
                } else {
                    if ((requiredValue is Saxon.Hej.value.BigIntegerValue)) {
                        System.Numerics.BigInteger v = ((Saxon.Hej.value.BigIntegerValue)(requiredValue)).asBigInteger();
                        ok = Saxon.Hej.value.IntegerValue.checkBigRange(v, bbase);
                    }
                }
                if (!(ok)) {
                    throw new Saxon.Hej.type.SchemaException((("The value of the "+getName())+" facet is out of range for the base type"));
                }
            }
        }
        protected internal void convertToTargetType(Saxon.Hej.type.SimpleType @base, Saxon.Hej.Configuration config) {
            if (!(isConverted)) {
                Saxon.Hej.type.StringConverter converter = ((Saxon.Hej.type.AtomicType)(@base)).getStringConverter(config.getConversionRules());
                Saxon.Hej.type.ConversionResult v = converter.convertString(requiredValue.getUnicodeStringValue());
                if ((v is Saxon.Hej.type.ValidationFailure)) {
                    string s = Saxon.Hej.trans.Err.wrap(requiredValue.getUnicodeStringValue(), Saxon.Hej.trans.Err.VALUE);
                    throw new Saxon.Hej.type.SchemaException(((((((("The value "+s)+" of the ")+getName())+" facet is not valid for the type ")+@base.getDescription())+". ")+((Saxon.Hej.type.ValidationFailure)(v)).getMessage()));
                }
                requiredValue = (Saxon.Hej.value.AtomicValue)(v);
                isConverted = true;
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            if (!(isConverted)) {
                throw new Saxon.Ejava.lang.AssertionError("Value used in facet has not been converted to required type");
            }
            if (((value == null) || (requiredValue == null))) {
                return false;
            }
            int c = Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(value, requiredValue);
            return ((c != Saxon.Hej.om.SequenceTool.INDETERMINATE_ORDERING) && testComparisonOK(c));
        }
        protected internal abstract bool testComparisonOK(int comparisonResult);
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.ValueRangeFacet thisFacet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.ValueRangeFacet>(thisFacet);
                  case "value":
                    return requiredValue;
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