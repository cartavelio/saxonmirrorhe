using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Contains : Saxon.Hej.functions.CollatingFunctionFixed {
        public override bool isSubstringMatchingFunction() {
            return true;
        }
        private static bool contains(Saxon.Hej.value.StringValue arg0, Saxon.Hej.value.StringValue arg1, Saxon.Hej.lib.SubstringMatcher collator) {
            if ((((arg1 == null) || arg1.isEmpty()) || collator.isEqualToEmpty(arg1.getUnicodeStringValue()))) {
                return true;
            }
            if (((arg0 == null) || arg0.isEmpty())) {
                return false;
            }
            return collator.contains(arg0.getUnicodeStringValue(), arg1.getUnicodeStringValue());
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue s0 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.value.StringValue s1 = (Saxon.Hej.value.StringValue)(arguments[1].head());
            return Saxon.Hej.value.BooleanValue.get(contains(s0, s1, (Saxon.Hej.lib.SubstringMatcher)(getStringCollator())));
        }
        public override string getCompilerName() {
            return "ContainsCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Contains.ContainsFnElaborator();
        }
        public class ContainsFnElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.CollatingFunctionFixed fn = (Saxon.Hej.functions.CollatingFunctionFixed)(fnc.getTargetFunction());
                Saxon.Hej.lib.SubstringMatcher collation = (Saxon.Hej.lib.SubstringMatcher)(fn.getStringCollator());
                System.Diagnostics.Debug.Assert((collation != null));
                string name = fnc.getFunctionName().getLocalPart();
                if ((collation == Saxon.Hej.expr.sort.CodepointCollator.getInstance())) {
                    Saxon.Hej.expr.elab.StringEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForString(true);
                    Saxon.Hej.expr.elab.StringEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForString(true);
                    switch (name) {
                      case "contains":
                        return context => /*Delegate.eval()*/arg0Eval(context).Contains(/*Delegate.eval()*/arg1Eval(context));
                      case "starts-with":
                        return context => /*Delegate.eval()*/arg0Eval(context).StartsWith(/*Delegate.eval()*/arg1Eval(context));
                      case "ends-with":
                        return context => /*Delegate.eval()*/arg0Eval(context).EndsWith(/*Delegate.eval()*/arg1Eval(context));
                      default:
                        throw new Saxon.Ejava.lang.UnsupportedOperationException();
                    }
                } else {
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForUnicodeString(true);
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForUnicodeString(true);
                    switch (name) {
                      case "contains":
                        return context => {
                            Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                            Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                            return collation.contains(s0, s1);
                        } ;
                      case "starts-with":
                        return context => {
                            Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                            Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                            return collation.startsWith(s0, s1);
                        } ;
                      case "ends-with":
                        return context => {
                            Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/arg0Eval(context);
                            Saxon.Hej.str.UnicodeString s1 = /*Delegate.eval()*/arg1Eval(context);
                            return collation.endsWith(s0, s1);
                        } ;
                      default:
                        throw new Saxon.Ejava.lang.UnsupportedOperationException();
                    }
                }
            }
        
        }
    
    }
}