using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class EscapeHtmlUri : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString s = arg.getUnicodeStringValue();
            return new Saxon.Hej.value.StringValue(Saxon.Hej.serialize.HTMLURIEscaper.escapeURL(s, false, getRetainedStaticContext().getConfiguration()));
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
    
    }
}