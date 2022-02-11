using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Contains : Saxon.Hej.functions.CollatingFunctionFixed {
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if ((getStringCollator() == Saxon.Hej.expr.sort.CodepointCollator.getInstance())) {
                return new Inner_Optimized_d1009e130(this, this, arguments);
            } else {
                return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
            }
        }
        public override bool isSubstringMatchingFunction() {
            return true;
        }
        private static bool contains(Saxon.Hej.value.StringValue arg0, Saxon.Hej.value.StringValue arg1, Saxon.Hej.lib.SubstringMatcher collator) {
            if ((((arg1 == null) || arg1.isEmpty()) || collator.comparesEqual(arg1.getUnicodeStringValue(), Saxon.Hej.str.EmptyUnicodeString.getInstance()))) {
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
    
        private class Inner_Optimized_d1009e130 : Saxon.Hej.expr.SystemFunctionCall.Optimized {
            private readonly Contains outerInstance;
            public Inner_Optimized_d1009e130(Contains outerInstance, Contains arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.str.UnicodeString s0 = /*superMethod*/getArg(0).evaluateAsString(/*localparam*/context).tidy();
                Saxon.Hej.str.UnicodeString s1 = /*superMethod*/getArg(1).evaluateAsString(/*localparam*/context).tidy();
                return (/*localvar*/s0.indexOf(/*localvar*/s1, 0)>=0);
            }
            public override  /* REPLACING public */ Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
                return this;
            }
        }

    }
}