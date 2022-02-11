using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class LowerCase : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.StringValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.StringValue.makeUStringValue(Saxon.Hej.str.ToLower.toLower(arg.getUnicodeStringValue()));
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override string getCompilerName() {
            return "ForceCaseCompiler";
        }
    
    }
}