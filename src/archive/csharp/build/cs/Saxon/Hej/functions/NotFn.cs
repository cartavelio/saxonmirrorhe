using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NotFn : Saxon.Hej.functions.SystemFunction {
        public override void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.trans.XPathException err = Saxon.Hej.expr.parser.TypeChecker.ebvError(arguments[0], visitor.getConfiguration().getTypeHierarchy());
            if ((err != null)) {
                throw err;
            }
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.BooleanValue.get(!(Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(arguments[0].iterate())));
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_d1151e209(this, this, arguments);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.type.TypeHierarchy th = visitor.getStaticContext().getConfiguration().getTypeHierarchy();
            if (((arguments[0] is Saxon.Hej.expr.Negatable) && ((Saxon.Hej.expr.Negatable)(arguments[0])).isNegatable(th))) {
                return ((Saxon.Hej.expr.Negatable)(arguments[0])).negate();
            }
            if ((arguments[0].getItemType() is Saxon.Hej.pattern.NodeTest)) {
                Saxon.Hej.functions.SystemFunction empty = Saxon.Hej.functions.SystemFunction.makeFunction("empty", getRetainedStaticContext(), 1);
                return empty.makeFunctionCall(arguments[0]).optimize(visitor, contextInfo);
            }
            return null;
        }
        public override string getCompilerName() {
            return "NotFnCompiler";
        }
        public override string getStreamerName() {
            return "NotFn";
        }
    
        private class Inner_SystemFunctionCall_d1151e209 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly NotFn outerInstance;
            public Inner_SystemFunctionCall_d1151e209(NotFn outerInstance, NotFn arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
                try {
                    return !(this.getArg(0).effectiveBooleanValue(/*localparam*/c));
                } catch (Saxon.Hej.trans.XPathException e) {
                    /*exception*/e.maybeSetLocation(this.getLocation());
                    /*exception*/e.maybeSetContext(/*localparam*/c);
                    throw;
                }
            }
        }

    }
}