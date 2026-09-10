using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn {
    public class StreamFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            return new Inner_SystemFunctionCall_d34e88(this, this, arguments);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.parser.Optimizer opt = visitor.getConfiguration().obtainOptimizer();
            Saxon.Hej.expr.Expression result = opt.optimizeSaxonStreamFunction(visitor, contextInfo, arguments[0]);
            if ((result == null)) {
                if (visitor.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK)) {
                    return arguments[0];
                } else {
                    throw new Saxon.Hej.trans.XPathException("The argument to saxon:stream() has the wrong form for streaming to be possible", "XTSE3430");
                }
            }
            if (result.isCallOn(typeof(Saxon.Eej.ee.extfn.StreamFn))) {
                return ((Saxon.Hej.expr.SystemFunctionCall)(result)).getArg(0);
            }
            return result;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            throw new Saxon.Hej.trans.XPathException("The function saxon:stream() cannot be called dynamically");
        }
    
        private class Inner_SystemFunctionCall_d34e88 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly StreamFn outerInstance;
            public Inner_SystemFunctionCall_d34e88(StreamFn outerInstance, StreamFn arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
                /*localparam*/visitor.setOptimizeForStreaming(true);
                return base.optimize(/*localparam*/visitor, /*localparam*/contextItemType);
            }
        }

    }
}