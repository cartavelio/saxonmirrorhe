using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ExplicitTimezoneFacet : Saxon.Eej.ee.schema.Facet {
        private Saxon.Eej.ee.schema.Optionality facetValue;
        public ExplicitTimezoneFacet()  {
        }
        public override string getName() {
            return "explicitTimezone";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            switch (facetValue.innerEnumValue) {
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.REQUIRED:
                return Saxon.Hej.str.BMPString.of("required");
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.OPTIONAL:
                return Saxon.Hej.str.BMPString.of("optional");
              case Saxon.Eej.ee.schema.Optionality.InnerEnum.PROHIBITED:
              default:
                return Saxon.Hej.str.BMPString.of("prohibited");
            }
        }
        public void setValue(string value) {
            switch (value) {
              case "required":
                this.facetValue = Saxon.Eej.ee.schema.Optionality.REQUIRED;
                break ;
              case "optional":
                this.facetValue = Saxon.Eej.ee.schema.Optionality.OPTIONAL;
                break ;
              case "prohibited":
                this.facetValue = Saxon.Eej.ee.schema.Optionality.PROHIBITED;
                break ;
              default:
                throw new Saxon.Hej.type.SchemaException("Value of explicitTimezone facet must be required, optional, or prohibited");
            }
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Eej.ee.schema.UserAtomicType)) {
                Saxon.Eej.ee.schema.UserAtomicType ubase = (Saxon.Eej.ee.schema.UserAtomicType)(@base);
                Saxon.Eej.ee.schema.ExplicitTimezoneFacet baseFacet = (Saxon.Eej.ee.schema.ExplicitTimezoneFacet)(ubase.getFacet(typeof(Saxon.Eej.ee.schema.ExplicitTimezoneFacet)));
                if ((baseFacet != null)) {
                    if ((baseFacet.isFixed() && (facetValue != baseFacet.facetValue))) {
                        throw new Saxon.Hej.type.SchemaException("The explicitTimezone facet is fixed in the base type and cannot be varied");
                    }
                    if (((baseFacet.facetValue == Saxon.Eej.ee.schema.Optionality.PROHIBITED) && (facetValue != Saxon.Eej.ee.schema.Optionality.PROHIBITED))) {
                        throw new Saxon.Hej.type.SchemaException("The explicitTimezone is prohibited in the base type so it must be prohibited in the derived type");
                    }
                    if (((baseFacet.facetValue == Saxon.Eej.ee.schema.Optionality.REQUIRED) && (facetValue != Saxon.Eej.ee.schema.Optionality.REQUIRED))) {
                        throw new Saxon.Hej.type.SchemaException("The explicitTimezone is required in the base type so it must be required in the derived type");
                    }
                }
            } else {
                if ((@base.getFingerprint() == Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP)) {
                    if ((facetValue != Saxon.Eej.ee.schema.Optionality.REQUIRED)) {
                        throw new Saxon.Hej.type.SchemaException("The explicitTimezone is required in the base type so it must be required in the derived type");
                    }
                }
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            if ((facetValue != Saxon.Eej.ee.schema.Optionality.OPTIONAL)) {
                if ((value is Saxon.Hej.value.CalendarValue)) {
                    return (((facetValue == Saxon.Eej.ee.schema.Optionality.REQUIRED)) == ((Saxon.Hej.value.CalendarValue)(value)).hasTimezone());
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.ExplicitTimezoneFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.ExplicitTimezoneFacet>(facet);
                  case "value":
                    return new Saxon.Hej.value.StringValue(getValue());
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