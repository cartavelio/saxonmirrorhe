using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Tokenize_3 : Saxon.Hej.functions.RegexFunction {
        protected internal override bool allowRegexMatchingEmptyString() {
            return false;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue sv = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((sv == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.str.UnicodeString input = sv.getUnicodeStringValue();
            if (input.isEmpty()) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments);
            return Saxon.Hej.om.SequenceTool.toLazySequence(re.tokenize(input));
        }
    
    }
}