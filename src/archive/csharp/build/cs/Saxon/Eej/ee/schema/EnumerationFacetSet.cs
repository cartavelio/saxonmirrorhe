using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class EnumerationFacetSet : Saxon.Eej.ee.schema.Facet {
        private readonly Saxon.Hej.expr.sort.CustomMap<Saxon.Hej.om.AtomicSequence,Saxon.Hej.om.AtomicSequence> typedValues = new Saxon.Hej.expr.sort.CustomMap<Saxon.Hej.om.AtomicSequence, Saxon.Hej.om.AtomicSequence>(Saxon.Hej.expr.sort.SimpleTypeIdentityComparison.getInstance());
        private readonly System.Collections.Generic.IList<string> stringValues = new System.Collections.Generic.List<string>(20);
        public override string getName() {
            return "enumeration";
        }
        public System.Collections.Generic.IList<string> getStringValues() {
            return stringValues;
        }
        public void addEnumerationValue(Saxon.Hej.om.AtomicSequence val, string stringValue) {
            typedValues.put(val, val);
            stringValues.Add(stringValue);
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            bool first = true;
            foreach (string s in stringValues) {
                if (first) {
                    first = false;
                } else {
                    sb.appendLatin(", ");
                }
                sb.append(s);
            }
            return sb.toUnicodeString();
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return typedValues.containsKey(value);
        }
        public override bool testListValue(Saxon.Hej.om.AtomicSequence value) {
            return typedValues.containsKey(value);
        }
        public string getSharedInstance(string value) {
            foreach (string s in stringValues) {
                if (s.Equals(value)) {
                    return s;
                }
            }
            return value;
        }
        public Saxon.Hej.om.AtomicSequence getSharedInstance(Saxon.Hej.om.AtomicSequence value) {
            return typedValues.get(value);
        }
        public override void serializeFacet(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            foreach (Saxon.Hej.om.AtomicSequence v in typedValues.values()) {
                serializer.startElement("enumeration");
                bool namespaceSensitive = false;
                foreach (Saxon.Hej.value.AtomicValue av in v) {
                    if ((av is Saxon.Hej.value.QualifiedNameValue)) {
                        string prefix = ((Saxon.Hej.value.QualifiedNameValue)(av)).getPrefix();
                        string uri = ((Saxon.Hej.value.QualifiedNameValue)(av)).getNamespaceURI();
                        serializer.declareNamespace(prefix, uri);
                        namespaceSensitive = true;
                    }
                }
                serializer.emitAttribute("value", v.getUnicodeStringValue().ToString());
                if (namespaceSensitive) {
                    serializer.emitAttribute("namespaceSensitive", "true");
                }
                serializer.endElement();
            }
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.EnumerationFacetSet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.EnumerationFacetSet>(facet);
                  case "value":
                    System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> list = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                    foreach (string s in stringValues) {
                        list.Add(new Saxon.Hej.value.StringValue(s));
                    }
                    return new Saxon.Hej.om.AtomicArray(list);
                  case "typed-value":
                    System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(typedValues.values());
                    return new Saxon.Hej.ma.arrays.SimpleArrayItem(members);
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}