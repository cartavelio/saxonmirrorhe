using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Replace : Saxon.Hej.functions.RegexFunction {
        private int version = 20;
        public static Saxon.Hej.functions.Replace make20() {
            Saxon.Hej.functions.Replace rep = new Saxon.Hej.functions.Replace();
            rep.version = 20;
            return rep;
        }
        public static Saxon.Hej.functions.Replace make40() {
            Saxon.Hej.functions.Replace rep = new Saxon.Hej.functions.Replace();
            rep.version = 40;
            return rep;
        }
        private bool replacementChecked = false;
        protected internal override bool allowRegexMatchingEmptyString() {
            return false;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            bool doEarlyReplacementCheck = true;
            if ((arguments.Length>=4)) {
                if ((arguments[3] is Saxon.Hej.expr.StringLiteral)) {
                    string flags = ((Saxon.Hej.expr.StringLiteral)(arguments[3])).stringify();
                    if ((flags.Contains("q") || flags.Contains(";"))) {
                        doEarlyReplacementCheck = false;
                    }
                } else {
                    doEarlyReplacementCheck = false;
                }
            }
            if (((arguments[2] is Saxon.Hej.expr.StringLiteral) && doEarlyReplacementCheck)) {
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
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments, 1, 3);
            Saxon.Hej.om.Item replacementArg = arguments[2].head();
            Saxon.Hej.str.UnicodeString replacement = ((replacementArg == null) ? null : replacementArg.getUnicodeStringValue());
            if (((replacement == null) && (version == 20))) {
                throw new Saxon.Hej.trans.XPathException("Third argument of fn:replace() must not be empty").withErrorCode("XPTY0004").asTypeError();
            }
            System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.str.UnicodeString[],Saxon.Hej.str.UnicodeString> action = null;
            if ((arguments.Length == 5)) {
                Saxon.Hej.om.FunctionItem actionFn = (Saxon.Hej.om.FunctionItem)(arguments[4].head());
                if ((actionFn != null)) {
                    action = (@in, groups) => {
                        try {
                            System.Collections.Generic.IList<Saxon.Hej.om.Item> groupItems = new System.Collections.Generic.List<Saxon.Hej.om.Item>(((Saxon.Hej.str.UnicodeString[])(groups)).Length);
                            foreach (Saxon.Hej.str.UnicodeString group in groups) {
                                groupItems.Add(new Saxon.Hej.value.StringValue(group, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC));
                            }
                            Saxon.Hej.om.Sequence result = actionFn.call(context, new Saxon.Hej.om.Sequence[]{new Saxon.Hej.value.StringValue(@in, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC), Saxon.Hej.value.SequenceExtent.makeSequenceExtent(groupItems)});
                            Saxon.Hej.om.Item resultItem = result.head();
                            if ((resultItem == null)) {
                                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                            } else {
                                return resultItem.getUnicodeStringValue();
                            }
                        } catch (Saxon.Hej.trans.XPathException e) {
                            throw new Saxon.Ejava.lang.AssertionError(e);
                        }
                    } ;
                }
            }
            if (((replacement != null) && (action != null))) {
                throw new Saxon.Hej.trans.XPathException("Cannot supply both a replacement string and a replacement action", "FORX0005");
            }
            if (((((replacement != null) && !(replacementChecked)) && !(re.getFlags().Contains("q"))) && !(re.isPlatformNative()))) {
                string msg = checkReplacement(replacement);
                if ((msg != null)) {
                    throw new Saxon.Hej.trans.XPathException(msg, "FORX0004", context);
                }
            }
            if ((replacement != null)) {
                return new Saxon.Hej.value.StringValue(re.replace(input, replacement));
            } else {
                if ((action != null)) {
                    return new Saxon.Hej.value.StringValue(re.replaceWith(input, action));
                } else {
                    return new Saxon.Hej.value.StringValue(re.replace(input, Saxon.Hej.str.EmptyUnicodeString.getInstance()));
                }
            }
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