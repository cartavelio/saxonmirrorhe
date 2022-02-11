using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Matches : Saxon.Hej.functions.RegexFunction {
        protected internal override bool allowRegexMatchingEmptyString() {
            return true;
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments);
            Saxon.Hej.value.StringValue arg = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString @in = ((arg == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : arg.getUnicodeStringValue());
            bool result = re.containsMatch(@in);
            return Saxon.Hej.value.BooleanValue.get(result);
        }
        public override string getCompilerName() {
            return "MatchesCompiler";
        }
    
    }
}