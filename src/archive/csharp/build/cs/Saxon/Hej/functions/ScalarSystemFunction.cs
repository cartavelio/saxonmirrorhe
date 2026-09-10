using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class ScalarSystemFunction : Saxon.Hej.functions.SystemFunction {
        public abstract Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context);
        public virtual Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.EmptySequence.getInstance();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item val0 = arguments[0].head();
            if ((val0 == null)) {
                return resultWhenEmpty();
            }
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(evaluate(val0, context));
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.expr.SystemFunctionCall call = new Inner_SystemFunctionCall_d1149e175(this, this, arguments);
            call.setRetainedStaticContext(getRetainedStaticContext());
            return call;
        }
    
        private class Inner_SystemFunctionCall_d1149e175 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly ScalarSystemFunction outerInstance;
            public Inner_SystemFunctionCall_d1149e175(ScalarSystemFunction outerInstance, ScalarSystemFunction arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.om.Item val = /*superMethod*/getArg(0).evaluateItem(/*localparam*/context);
                if ((/*localvar*/val == null)) {
                    return (Saxon.Hej.value.AtomicValue)(outerInstance.resultWhenEmpty().head());
                } else {
                    return outerInstance.evaluate(/*localvar*/val, /*localparam*/context);
                }
            }
        }

    }
}