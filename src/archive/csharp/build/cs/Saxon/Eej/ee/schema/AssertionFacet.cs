using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class AssertionFacet : Saxon.Eej.ee.schema.Facet {
        internal Saxon.Eej.ee.schema.Assertion assertion;
        public AssertionFacet(Saxon.Eej.ee.schema.Assertion assertion)  {
            this.assertion = assertion;
        }
        public override string getName() {
            return "assertion";
        }
        public override string getConstraintName() {
            return "cvc-assertions-valid";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            return Saxon.Hej.str.StringView.tidy(assertion.getConditionText());
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Validation context required");
        }
        public bool testAtomicValue(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.ConversionRules validationContext) {
            return assertion.testSimple(value, validationContext);
        }
        public override bool testListValue(Saxon.Hej.om.AtomicSequence value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Validation context required");
        }
        public bool testListValue(Saxon.Hej.om.AtomicSequence value, Saxon.Hej.lib.ConversionRules rules) {
            return assertion.testSimple(value, rules);
        }
        public override void serializeFacet(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("assert");
            if (isFixed()) {
                serializer.emitAttribute("fixed", "true");
            }
            assertion.serialize(serializer);
            serializer.endElement();
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.AssertionFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.AssertionFacet>(facet);
                  case "value":
                    return assertion.getComponentAsFunction();
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}