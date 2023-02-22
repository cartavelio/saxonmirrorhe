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
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.SubstringBefore.SubstringBeforeFnElaborator();
        }
        public class SubstringBeforeFnElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SubstringBefore fn = (Saxon.Hej.functions.SubstringBefore)(fnc.getTargetFunction());
                Saxon.Hej.lib.SubstringMatcher stringCollator = (Saxon.Hej.lib.SubstringMatcher)(fn.getStringCollator());
                System.Diagnostics.Debug.Assert((stringCollator != null));
                Saxon.Hej.expr.elab.UnicodeStringEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForUnicodeString(true);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForUnicodeString(true);
                if ((stringCollator is Saxon.Hej.expr.sort.CodepointCollator)) {
                    return context => {
                        Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                        Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                        if (s1.isEmpty()) {
                            return s1;
                        }
                        if (s0.isEmpty()) {
                            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                        }
                        long j = s0.indexOf(s1, 0);
                        if ((j<0)) {
                            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                        }
                        return s0.prefix(j);
                    } ;
                } else {
                    return context => {
                        Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                        Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                        return stringCollator.substringBefore(s0, s1);
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SubstringBefore fn = (Saxon.Hej.functions.SubstringBefore)(fnc.getTargetFunction());
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
                        int j = s0.IndexOf(s1, 0);
                        if ((j<0)) {
                            return "";
                        }
                        return s0.Substring(0, j - 0);
                    } ;
                } else {
                    return base.elaborateForString(zeroLengthWhenAbsent);
                }
            }
        
        }
    
    }
}