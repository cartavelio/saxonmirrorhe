using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class PreprocessFacet : Saxon.Eej.ee.schema.Facet {
        internal Saxon.Eej.ee.schema.Assertion action;
        internal Saxon.Eej.ee.schema.Assertion reverseAction;
        public PreprocessFacet()  {
        }
        public PreprocessFacet(Saxon.Eej.ee.schema.Assertion action, Saxon.Eej.ee.schema.Assertion reverseAction)  {
            this.action = action;
            this.reverseAction = reverseAction;
        }
        public override bool isConstraining() {
            return false;
        }
        public override string getName() {
            return "preprocess";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            return null;
        }
        public Saxon.Eej.ee.schema.Assertion getAction() {
            return action;
        }
        public Saxon.Eej.ee.schema.Assertion getReverseAction() {
            return reverseAction;
        }
        public void setAction(Saxon.Eej.ee.schema.Assertion action) {
            this.action = action;
        }
        public void setReverseAction(Saxon.Eej.ee.schema.Assertion action) {
            this.reverseAction = action;
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            Saxon.Hej.sxpath.XPathExpression exp = action.getCondition();
            try {
                Saxon.Hej.sxpath.XPathDynamicContext context = exp.createDynamicContext();
                if ((action.getValueVariable() != null)) {
                    context.setVariable(action.getValueVariable(), new Saxon.Hej.value.StringValue(input));
                }
                return ((Saxon.Hej.om.GroundedValue)(exp.evaluateSingle(context))).getUnicodeStringValue();
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.type.ValidationException(err);
            }
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            if ((reverseAction == null)) {
                return input;
            }
            Saxon.Hej.sxpath.XPathExpression exp = reverseAction.getCondition();
            try {
                Saxon.Hej.sxpath.XPathDynamicContext context = exp.createDynamicContext();
                if ((action.getValueVariable() != null)) {
                    context.setVariable(reverseAction.getValueVariable(), new Saxon.Hej.value.StringValue(input));
                }
                return ((Saxon.Hej.om.GroundedValue)(exp.evaluateSingle(context))).getUnicodeStringValue();
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.type.ValidationException(err);
            }
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return true;
        }
        public override void serializeFacet(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement(getName());
            if (isFixed()) {
                serializer.emitAttribute("fixed", "true");
            }
            action.serialize(serializer);
            if ((reverseAction != null)) {
                reverseAction.serialize(serializer);
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.PreprocessFacet thisFacet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.PreprocessFacet>(thisFacet);
                  case "action":
                    return action.getComponentAsFunction();
                  case "reverse action":
                    return reverseAction.getComponentAsFunction();
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