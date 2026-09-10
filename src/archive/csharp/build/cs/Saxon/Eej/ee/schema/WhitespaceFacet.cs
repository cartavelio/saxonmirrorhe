using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class WhitespaceFacet : Saxon.Eej.ee.schema.Facet {
        internal int action;
        public WhitespaceFacet()  {
        }
        public override bool isConstraining() {
            return false;
        }
        public override string getName() {
            return "whiteSpace";
        }
        public void setValue(string value) {
            if (value.Equals("preserve")) {
                action = Saxon.Hej.value.Whitespace.PRESERVE;
            } else {
                if (value.Equals("replace")) {
                    action = Saxon.Hej.value.Whitespace.REPLACE;
                } else {
                    if (value.Equals("collapse")) {
                        action = Saxon.Hej.value.Whitespace.COLLAPSE;
                    } else {
                        throw new Saxon.Hej.type.SchemaException(("Invalid whiteSpace facet value: "+value));
                    }
                }
            }
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            switch (action) {
              case Saxon.Hej.value.Whitespace.PRESERVE:
                return Saxon.Hej.str.BMPString.of("preserve");
              case Saxon.Hej.value.Whitespace.REPLACE:
                return Saxon.Hej.str.BMPString.of("replace");
              case Saxon.Hej.value.Whitespace.COLLAPSE:
                return Saxon.Hej.str.BMPString.of("collapse");
            }
            return null;
        }
        public override int getWhitespaceAction() {
            return action;
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (@base.isListType()) {
                if ((action != Saxon.Hej.value.Whitespace.COLLAPSE)) {
                    throw new Saxon.Hej.type.SchemaException("The whiteSpace facet on a list type must be {collapse}");
                }
            } else {
                if (@base.isUnionType()) {
                    throw new Saxon.Hej.type.SchemaException("The whiteSpace facet is not applicable to a union type");
                } else {
                    if ((@base is Saxon.Hej.type.AnySimpleType)) {
                    } else {
                        int p = ((Saxon.Hej.type.AtomicType)(@base)).getPrimitiveType();
                        if ((type.getFingerprint() == Saxon.Hej.om.StandardNames.XS_STRING)) {
                        } else {
                            if ((p == Saxon.Hej.om.StandardNames.XS_STRING)) {
                                if ((action<@base.getWhitespaceAction())) {
                                    throw new Saxon.Hej.type.SchemaException("The whiteSpace normalization must not be weaker than that of the base type");
                                }
                            } else {
                                if ((action != Saxon.Hej.value.Whitespace.COLLAPSE)) {
                                    throw new Saxon.Hej.type.SchemaException(("The whiteSpace facet must be {collapse} "+"unless the type is derived from string"));
                                }
                            }
                        }
                    }
                }
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return true;
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.WhitespaceFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.WhitespaceFacet>(facet);
                  case "value":
                    switch (action) {
                      case Saxon.Hej.value.Whitespace.PRESERVE:
                      default:
                        return Saxon.Hej.value.StringValue.bmp("preserve");
                      case Saxon.Hej.value.Whitespace.COLLAPSE:
                        return Saxon.Hej.value.StringValue.bmp("collapse");
                      case Saxon.Hej.value.Whitespace.REPLACE:
                        return Saxon.Hej.value.StringValue.bmp("replace");
                    }
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