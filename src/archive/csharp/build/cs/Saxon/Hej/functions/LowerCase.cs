using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class LowerCase : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.StringValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.StringValue.makeUStringValue(Saxon.Hej.str.ToLower.toLower(arg.getUnicodeStringValue()));
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override string getCompilerName() {
            return "ForceCaseCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.LowerCase.LowerCaseFnElaborator();
        }
        public class LowerCaseFnElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator argEval = arg.makeElaborator().elaborateForUnicodeString(false);
                return context => {
                    Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/argEval(context);
                    if ((s0 == null)) {
                        return handleNullUnicodeString(zeroLengthWhenAbsent);
                    }
                    return Saxon.Hej.str.ToLower.toLower(s0);
                } ;
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                return elaborateForStringAlternative(zeroLengthWhenAbsent);
            }
            public Saxon.Hej.expr.elab.StringEvaluator elaborateForStringAlternative(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator argEval = arg.makeElaborator().elaborateForUnicodeString(false);
                return context => {
                    Saxon.Hej.str.UnicodeString s0 = /*Delegate.eval()*/argEval(context);
                    if ((s0 == null)) {
                        return (zeroLengthWhenAbsent ? "" : null);
                    }
                    return Saxon.Hej.str.ToLower.toLower(s0).ToString();
                } ;
            }
        
        }
    
    }
}