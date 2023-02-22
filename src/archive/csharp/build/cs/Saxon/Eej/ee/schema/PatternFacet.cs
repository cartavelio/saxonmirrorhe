using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class PatternFacet : Saxon.Eej.ee.schema.Facet {
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.str.UnicodeString requiredValue;
        private Saxon.Hej.regex.RegularExpression regex;
        private string flags = "";
        private bool noMerge;
        public PatternFacet(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public override string getName() {
            return "pattern";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            return requiredValue;
        }
        public Saxon.Hej.regex.RegularExpression getRegularExpression() {
            return regex;
        }
        public void setValue(string value) {
            requiredValue = Saxon.Hej.str.StringView.of(value).tidy();
        }
        public void setFlags(string flags) {
            this.flags = flags;
        }
        public string getFlags() {
            return flags;
        }
        public void compile(System.Collections.Generic.IList<string> warnings) {
            try {
                string lang = ((config.getXsdVersion() == Saxon.Hej.Configuration.XSD11) ? "XSD11" : "XSD10");
                regex = config.compileRegularExpression(requiredValue, flags, lang, warnings);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.type.SchemaException(e.Message);
            }
        }
        public void setDoNotMerge(bool dont) {
            noMerge = dont;
        }
        public bool isDoNotMerge() {
            return noMerge;
        }
        public void addBranch(Saxon.Hej.str.UnicodeString pat) {
            try {
                setValue((((('('+getValue().ToString())+")|(")+pat)+')'));
                compile(null);
            } catch (Saxon.Hej.type.SchemaException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return regex.matches(value.getCanonicalLexicalRepresentation());
        }
        public override Saxon.Hej.om.FunctionItem getFacetAsFunctionItem() {
            Saxon.Eej.ee.schema.PatternFacet facet = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return new Saxon.Hej.value.StringValue(getName());
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.PatternFacet>(facet);
                  case "value":
                    return new Saxon.Hej.value.StringValue(requiredValue);
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}