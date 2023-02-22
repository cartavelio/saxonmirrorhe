using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class TotalDigitsFacet : Saxon.Eej.ee.schema.DigitsFacet {
        public override string getName() {
            return "totalDigits";
        }
        public override void setValue(string value) {
            int val;
            try {
                val = Saxon.Impl.Helpers.Utils.ParseInt(value);
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                throw new Saxon.Hej.type.SchemaException((("Value of "+getName())+" facet must be an integer"));
            }
            if ((val<=0)) {
                throw new Saxon.Hej.type.SchemaException("Value of totalDigits must be greater than zero");
            }
            setNumericValue(val);
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            base.checkFacetRestriction(type, @base, compiler);
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                Saxon.Eej.ee.schema.UserSimpleType ubase = (Saxon.Eej.ee.schema.UserSimpleType)(@base);
                Saxon.Eej.ee.schema.DigitsFacet f = (Saxon.Eej.ee.schema.DigitsFacet)(ubase.getFacet(typeof(Saxon.Eej.ee.schema.TotalDigitsFacet)));
                if ((f != null)) {
                    if ((getNumericValue()>f.getNumericValue())) {
                        throw new Saxon.Hej.type.SchemaException("The totalDigits facet must not exceed the value on the base type");
                    }
                    if ((f.isFixed() && (getNumericValue() != f.getNumericValue()))) {
                        throw new Saxon.Hej.type.SchemaException("The totalDigits facet is fixed in the base type and cannot be varied");
                    }
                }
            }
        }
    
    }
}