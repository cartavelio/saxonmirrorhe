using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NormalizeSpace_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.value.StringValue(normalizeSpace(arg.getUnicodeStringValue()));
        }
        public static Saxon.Hej.str.UnicodeString normalizeSpace(Saxon.Hej.str.UnicodeString sv) {
            if ((sv == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return Saxon.Hej.value.Whitespace.collapseWhitespace(sv);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_5(this, this, arguments);
        }
        public override string getCompilerName() {
            return "NormalizeSpaceCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.NormalizeSpace_1.NormalizeSpaceFnElaborator();
        }
        public class NormalizeSpaceFnElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override bool returnZeroLengthWhenAbsent() {
                return true;
            }
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.UnicodeStringEvaluator argEval = fnc.getArg(0).makeElaborator().elaborateForUnicodeString(true);
                return context => Saxon.Hej.value.Whitespace.collapseWhitespace(/*Delegate.eval()*/argEval(context));
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.StringEvaluator argEval = arg.makeElaborator().elaborateForString(zeroLengthWhenAbsent);
                return context => {
                    string @in = /*Delegate.eval()*/argEval(context);
                    if (((@in == null) || @in.Length==0)) {
                        return handleNullString(zeroLengthWhenAbsent);
                    }
                    return Saxon.Hej.value.Whitespace.collapseWhitespace(@in);
                } ;
            }
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator argEval = arg.makeElaborator().elaborateForUnicodeString(false);
                return context => {
                    Saxon.Hej.str.UnicodeString @in = /*Delegate.eval()*/argEval(context);
                    if (((@in == null) || @in.isEmpty())) {
                        return false;
                    }
                    return !(Saxon.Hej.value.Whitespace.isAllWhite(@in));
                } ;
            }
        
        }
    
        private class Inner_SystemFunctionCall_5 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly NormalizeSpace_1 outerInstance;
            public Inner_SystemFunctionCall_5(NormalizeSpace_1 outerInstance, NormalizeSpace_1 arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.value.AtomicValue sv = (Saxon.Hej.value.AtomicValue)(this.getArg(0).evaluateItem(/*localparam*/c));
                if ((/*localvar*/sv == null)) {
                    return false;
                }
                return !(Saxon.Hej.value.Whitespace.isAllWhite(/*localvar*/sv.getUnicodeStringValue()));
            }
        }

    }
}