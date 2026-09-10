using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Matches : Saxon.Hej.functions.RegexFunction {
        protected internal override bool allowRegexMatchingEmptyString() {
            return true;
        }
        public bool evalMatches(Saxon.Hej.str.UnicodeString input, Saxon.Hej.str.UnicodeString regex, Saxon.Hej.str.UnicodeString flags, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.regex.RegularExpression re;
            if ((regex == null)) {
                return false;
            }
            try {
                string lang = "XP30";
                if ((context.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD11)) {
                    lang += "/XSD11";
                }
                re = context.getConfiguration().compileRegularExpression(regex, flags.ToString(), lang, null);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetErrorCode("FORX0002");
                err.maybeSetContext(context);
                throw;
            }
            return re.containsMatch(input);
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments);
            Saxon.Hej.value.StringValue arg = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString @in = ((arg == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : arg.getUnicodeStringValue());
            bool result = re.containsMatch(@in);
            return Saxon.Hej.value.BooleanValue.get(result);
        }
        public override string getCompilerName() {
            return "MatchesCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Matches.MatchesFnElaborator();
        }
        public class MatchesFnElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.Matches fn = (Saxon.Hej.functions.Matches)(fnc.getTargetFunction());
                Saxon.Hej.expr.elab.UnicodeStringEvaluator arg0eval = fnc.getArg(0).makeElaborator().elaborateForUnicodeString(true);
                Saxon.Hej.regex.RegularExpression staticRegex = fn.getStaticRegex();
                if ((staticRegex == null)) {
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator arg1eval = fnc.getArg(1).makeElaborator().elaborateForUnicodeString(true);
                    Saxon.Hej.expr.elab.UnicodeStringEvaluator arg2eval = ((fn.getArity() == 3) ? fnc.getArg(2).makeElaborator().elaborateForUnicodeString(true) : cxt => Saxon.Hej.str.EmptyUnicodeString.getInstance());
                    return context => {
                        try {
                            return fn.evalMatches(/*Delegate.eval()*/arg0eval(context), /*Delegate.eval()*/arg1eval(context), /*Delegate.eval()*/arg2eval(context), context);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetLocation(fnc.getLocation());
                            err.maybeSetContext(context);
                            throw;
                        }
                    } ;
                } else {
                    return context => staticRegex.containsMatch(/*Delegate.eval()*/arg0eval(context));
                }
            }
        
        }
    
    }
}