using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class SubstringAfter : Saxon.Hej.functions.CollatingFunctionFixed {
        public override bool isSubstringMatchingFunction() {
            return true;
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UnicodeString s0 = getUniStringArg(arguments[0]);
            Saxon.Hej.str.UnicodeString s1 = getUniStringArg(arguments[1]);
            return new Saxon.Hej.value.StringValue(substringAfter(s0, s1, (Saxon.Hej.lib.SubstringMatcher)(getStringCollator())));
        }
        private static Saxon.Hej.str.UnicodeString substringAfter(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.str.UnicodeString arg2, Saxon.Hej.lib.SubstringMatcher collator) {
            if ((arg1 == null)) {
                arg1 = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            if ((arg2 == null)) {
                arg2 = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            if (arg2.isEmpty()) {
                return arg1;
            }
            if (arg1.isEmpty()) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return collator.substringAfter(arg1, arg2);
        }
        public override string getCompilerName() {
            return "SubstringAfterCompiler";
        }
    
    }
}