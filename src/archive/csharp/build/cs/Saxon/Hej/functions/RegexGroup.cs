using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class RegexGroup : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.regex.RegexIterator iter = context.getCurrentRegexIterator();
            if ((iter == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            Saxon.Hej.value.NumericValue gp0 = (Saxon.Hej.value.NumericValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString s = iter.getRegexGroup((int)(gp0.longValue()));
            return Saxon.Hej.value.StringValue.makeUStringValue(s);
        }
    
    }
}