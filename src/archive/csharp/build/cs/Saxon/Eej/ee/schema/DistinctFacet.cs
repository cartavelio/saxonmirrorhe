using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class DistinctFacet : Saxon.Eej.ee.schema.Facet {
        public DistinctFacet()  {
        }
        public override string getName() {
            return "distinct";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            return Saxon.Hej.str.BMPString.of("true");
        }
        public void setValue(string value) {
            if (((value != null) && !(value.Equals("true")))) {
                throw new Saxon.Hej.type.SchemaException("The only permitted value for saxon:distinct is 'true'");
            }
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Eej.ee.schema.UserListType)) {
            } else {
                throw new Saxon.Hej.type.SchemaException("The saxon:distinct facet is allowed only on list types");
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return true;
        }
        public override bool testListValue(Saxon.Hej.om.AtomicSequence value) {
            Saxon.Hej.expr.sort.CustomSet<Saxon.Hej.om.AtomicSequence> values = new Saxon.Hej.expr.sort.CustomSet<Saxon.Hej.om.AtomicSequence>(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance());
            foreach (Saxon.Hej.value.AtomicValue atom in value) {
                if (!(values.add(atom))) {
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.DistinctFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.DistinctFacet>(facet);
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