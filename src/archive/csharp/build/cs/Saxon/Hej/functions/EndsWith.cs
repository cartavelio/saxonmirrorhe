using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class EndsWith : Saxon.Hej.functions.CollatingFunctionFixed {
        public override bool isSubstringMatchingFunction() {
            return true;
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if ((getStringCollator() == Saxon.Hej.expr.sort.CodepointCollator.getInstance())) {
                return new Inner_Optimized_3(this, this, arguments);
            } else {
                return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
            }
        }
        public static bool endsWith(Saxon.Hej.str.UnicodeString arg0, Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.lib.SubstringMatcher collator) {
            if ((((arg1 == null) || arg1.isEmpty()) || collator.isEqualToEmpty(arg1))) {
                return true;
            }
            if (((arg0 == null) || arg0.isEmpty())) {
                return false;
            }
            return collator.endsWith(arg0, arg1);
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UnicodeString s0 = getUniStringArg(arguments[0]);
            Saxon.Hej.str.UnicodeString s1 = getUniStringArg(arguments[1]);
            return Saxon.Hej.value.BooleanValue.get(endsWith(s0, s1, (Saxon.Hej.lib.SubstringMatcher)(getStringCollator())));
        }
        public override string getCompilerName() {
            return "StartsWithCompiler";
        }
    
        private class Inner_Optimized_3 : Saxon.Hej.expr.SystemFunctionCall.Optimized {
            private readonly EndsWith outerInstance;
            public Inner_Optimized_3(EndsWith outerInstance, EndsWith arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
                string s0 = /*superMethod*/getArg(0).evaluateAsString(/*localparam*/context).ToString();
                string s1 = /*superMethod*/getArg(1).evaluateAsString(/*localparam*/context).ToString();
                return /*localvar*/s0.EndsWith(/*localvar*/s1);
            }
        }

    }
}