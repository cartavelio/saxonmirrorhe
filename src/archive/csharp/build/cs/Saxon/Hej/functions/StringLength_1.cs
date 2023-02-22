using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class StringLength_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.ZERO, Saxon.Hej.expr.Expression.MAX_STRING_LENGTH};
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.Int64Value.ZERO;
        }
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            if ((arg is Saxon.Hej.value.StringValue)) {
                return Saxon.Hej.value.Int64Value.makeIntegerValue(((Saxon.Hej.value.StringValue)(arg)).length());
            } else {
                Saxon.Hej.str.UnicodeString s;
                try {
                    s = arg.getUnicodeStringValue();
                } catch (Saxon.Ejava.lang.UnsupportedOperationException e) {
                    throw new Saxon.Hej.trans.XPathException("Cannot get the string value of a function item", "FOTY0013");
                }
                return Saxon.Hej.value.Int64Value.makeIntegerValue(s.length());
            }
        }
        public override string getCompilerName() {
            return "StringLengthCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.StringLength_1.StringLengthFnElaborator();
        }
        public class StringLengthFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator argEval = arg.makeElaborator().elaborateForUnicodeString(true);
                return context => {
                    Saxon.Hej.str.UnicodeString str = /*Delegate.eval()*/argEval(context);
                    return Saxon.Hej.value.Int64Value.makeIntegerValue(str.length());
                } ;
            }
        
        }
    
    }
}