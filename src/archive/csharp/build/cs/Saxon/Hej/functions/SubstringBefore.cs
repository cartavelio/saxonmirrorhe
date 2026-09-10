using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class SubstringBefore : Saxon.Hej.functions.CollatingFunctionFixed {
        public override bool isSubstringMatchingFunction() {
            return true;
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UnicodeString s0 = getUniStringArg(arguments[0]);
            Saxon.Hej.str.UnicodeString s1 = getUniStringArg(arguments[1]);
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            return new Saxon.Hej.value.StringValue(((Saxon.Hej.lib.SubstringMatcher)(collator)).substringBefore(s0, s1));
        }
        public override string getCompilerName() {
            return "SubstringBeforeCompiler";
        }
    
    }
}