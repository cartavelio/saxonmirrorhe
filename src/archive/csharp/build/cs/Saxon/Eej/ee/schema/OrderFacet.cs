using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class OrderFacet : Saxon.Eej.ee.schema.Facet {
        private bool ascending;
        public OrderFacet()  {
        }
        public override string getName() {
            return "order";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            string @base = (ascending ? "ascending" : "descending");
            return Saxon.Hej.str.BMPString.of(@base);
        }
        public void setValue(string value) {
            switch (value) {
              case "ascending":
                this.ascending = true;
                break ;
              case "descending":
                this.ascending = false;
                break ;
              default:
                throw new Saxon.Hej.type.SchemaException("Value of saxon:order facet must be ascending or descending");
            }
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Eej.ee.schema.UserListType)) {
                Saxon.Eej.ee.schema.UserListType ubase = (Saxon.Eej.ee.schema.UserListType)(@base);
                Saxon.Eej.ee.schema.OrderFacet baseFacet = (Saxon.Eej.ee.schema.OrderFacet)(ubase.getFacet(typeof(Saxon.Eej.ee.schema.OrderFacet)));
                if ((baseFacet != null)) {
                    if ((baseFacet.ascending != ascending)) {
                        throw new Saxon.Hej.type.SchemaException("The saxon:order facet cannot be varied in a restricted type");
                    }
                }
            } else {
                throw new Saxon.Hej.type.SchemaException("The saxon:order facet is allowed only on list types");
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return true;
        }
        public override bool testListValue(Saxon.Hej.om.AtomicSequence value) {
            Saxon.Hej.value.AtomicValue latest = null;
            if (ascending) {
                foreach (Saxon.Hej.value.AtomicValue atom in value) {
                    if (((latest != null) && (Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(latest, atom)>0))) {
                        return false;
                    }
                    latest = atom;
                }
            } else {
                foreach (Saxon.Hej.value.AtomicValue atom in value) {
                    if (((latest != null) && (Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().compareItems(latest, atom)<0))) {
                        return false;
                    }
                    latest = atom;
                }
            }
            return true;
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.OrderFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.OrderFacet>(facet);
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