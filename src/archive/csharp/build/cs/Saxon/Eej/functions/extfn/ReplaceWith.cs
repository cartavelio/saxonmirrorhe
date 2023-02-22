using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class ReplaceWith : Saxon.Hej.functions.RegexFunction {
        protected internal override bool allowRegexMatchingEmptyString() {
            return false;
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue arg0 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.value.StringValue input = ((arg0 == null) ? Saxon.Hej.value.StringValue.EMPTY_STRING : arg0);
            if ((arguments.Length == 3)) {
                Saxon.Hej.om.Sequence[] extra = new Saxon.Hej.om.Sequence[4];
                Saxon.Ejava.lang.System.arraycopy(arguments, 0, extra, 0, 3);
                extra[3] = Saxon.Hej.value.StringValue.EMPTY_STRING;
                arguments = extra;
            }
            Saxon.Hej.om.Function arg2 = (Saxon.Hej.om.Function)(arguments[2].head());
            System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString> fn = @in => {
                try {
                    Saxon.Hej.om.Sequence result = arg2.call(context, new Saxon.Hej.om.Sequence[]{new Saxon.Hej.value.StringValue(@in)});
                    return result.head().getUnicodeStringValue();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            } ;
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments);
            return new Saxon.Hej.value.StringValue(re.replaceWith(input.getUnicodeStringValue(), fn));
        }
    
    }
}