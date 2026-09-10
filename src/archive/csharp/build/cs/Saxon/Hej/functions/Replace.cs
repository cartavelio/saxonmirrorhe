using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Replace : Saxon.Hej.functions.RegexFunction {
        private bool replacementChecked = false;
        protected internal override bool allowRegexMatchingEmptyString() {
            return false;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            bool maybeQ = ((arguments.Length == 4) && ((!(((arguments[3] is Saxon.Hej.expr.StringLiteral))) || ((Saxon.Hej.expr.StringLiteral)(arguments[3])).stringify().Contains("q"))));
            if (((arguments[2] is Saxon.Hej.expr.StringLiteral) && !(maybeQ))) {
                Saxon.Hej.str.UnicodeString rep = ((Saxon.Hej.expr.StringLiteral)(arguments[2])).getString();
                if ((checkReplacement(rep) == null)) {
                    replacementChecked = true;
                }
            }
            return base.makeFunctionCall(arguments);
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue arg0 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString input = ((arg0 == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : arg0.getUnicodeStringValue());
            Saxon.Hej.str.UnicodeString replacement = arguments[2].head().getUnicodeStringValue();
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments);
            if (!(re.getFlags().Contains("q"))) {
                if (!(replacementChecked)) {
                    string msg = checkReplacement(replacement);
                    if ((msg != null)) {
                        throw new Saxon.Hej.trans.XPathException(msg, "FORX0004", context);
                    }
                }
            }
            return new Saxon.Hej.value.StringValue(re.replace(input, replacement));
        }
        public static string checkReplacement(Saxon.Hej.str.UnicodeString rep) {
            for (int i = 0; (i<rep.length()); i++) {
                int c = rep.codePointAt(i);
                if ((c == '$')) {
                    if (((i+1)<rep.length())) {
                        int index = ++i;
                        int next = rep.codePointAt(index);
                        if (((next<'0') || (next>'9'))) {
                            return "Invalid replacement string in replace(): $ sign must be followed by digit 0-9";
                        }
                    } else {
                        return "Invalid replacement string in replace(): $ sign at end of string";
                    }
                } else {
                    if ((c == '\\')) {
                        if (((i+1)<rep.length())) {
                            int index = ++i;
                            int next = rep.codePointAt(index);
                            if (((next != '\\') && (next != '$'))) {
                                return "Invalid replacement string in replace(): \\ character must be followed by \\ or $";
                            }
                        } else {
                            return "Invalid replacement string in replace(): \\ character at end of string";
                        }
                    }
                }
            }
            return null;
        }
    
    }
}