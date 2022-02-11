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