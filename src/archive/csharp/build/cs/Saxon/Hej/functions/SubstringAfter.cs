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
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.SubstringAfter.SubstringAfterFnElaborator();
        }
        public class SubstringAfterFnElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SubstringAfter fn = (Saxon.Hej.functions.SubstringAfter)(fnc.getTargetFunction());
                Saxon.Hej.lib.SubstringMatcher stringCollator = (Saxon.Hej.lib.SubstringMatcher)(fn.getStringCollator());
                System.Diagnostics.Debug.Assert((stringCollator != null));
                Saxon.Hej.expr.elab.UnicodeStringEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForUnicodeString(true);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForUnicodeString(true);
                if ((stringCollator is Saxon.Hej.expr.sort.CodepointCollator)) {
                    return context => {
                        Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                        Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                        if (s1.isEmpty()) {
                            return s0;
                        }
                        if (s0.isEmpty()) {
                            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                        }
                        long i = s0.indexOf(s1, 0);
                        if ((i<0)) {
                            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                        }
                        return s0.substring((i+s1.length()));
                    } ;
                } else {
                    return context => {
                        Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                        Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                        if (s1.isEmpty()) {
                            return s0;
                        }
                        if (s0.isEmpty()) {
                            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                        }
                        return stringCollator.substringAfter(s0, s1);
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SubstringAfter fn = (Saxon.Hej.functions.SubstringAfter)(fnc.getTargetFunction());
                Saxon.Hej.lib.SubstringMatcher stringCollator = (Saxon.Hej.lib.SubstringMatcher)(fn.getStringCollator());
                System.Diagnostics.Debug.Assert((stringCollator != null));
                if ((stringCollator is Saxon.Hej.expr.sort.CodepointCollator)) {
                    Saxon.Hej.expr.elab.StringEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForString(true);
                    Saxon.Hej.expr.elab.StringEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForString(true);
                    return context => {
                        string s0 = /*Delegate.eval()*/arg0Eval(context);
                        string s1 = /*Delegate.eval()*/arg1Eval(context);
                        if (s1.Length==0) {
                            return s0;
                        }
                        if (s0.Length==0) {
                            return "";
                        }
                        int i = s0.IndexOf(s1, 0);
                        if ((i<0)) {
                            return "";
                        }
                        return s0.Substring((i+s1.Length));
                    } ;
                } else {
                    return base.elaborateForString(zeroLengthWhenAbsent);
                }
            }
        
        }
    
    }
}