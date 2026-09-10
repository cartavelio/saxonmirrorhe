using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class FoldLeftFn : Saxon.Hej.functions.FoldingFunction {
        public override Saxon.Hej.functions.Fold getFold(Saxon.Hej.expr.XPathContext context, params Saxon.Hej.om.Sequence[]  arguments) {
            Saxon.Hej.om.Sequence arg0 = arguments[0];
            return new Saxon.Hej.functions.hof.FoldLeftFn.FoldLeftFold(context, arg0.materialize(), (Saxon.Hej.om.FunctionItem)(arguments[1].head()));
        }
        public class FoldLeftFold : Saxon.Hej.functions.Fold {
            private readonly Saxon.Hej.expr.XPathContext context;
            private readonly Saxon.Hej.om.FunctionItem function;
            private Saxon.Hej.om.Sequence data;
            private int counter;
            public FoldLeftFold(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.GroundedValue zero, Saxon.Hej.om.FunctionItem function)  {
                this.context = context;
                this.function = function;
                this.data = zero;
                this.counter = 0;
            }
            public void processItem(Saxon.Hej.om.Item item) {
                Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[2];
                args[0] = data;
                args[1] = item;
                Saxon.Hej.om.Sequence result = dynamicCall(function, context, args);
                if (((counter++%32) == 0)) {
                    data = result.materialize();
                } else {
                    data = result;
                }
            }
            public bool isFinished() {
                return false;
            }
            public Saxon.Hej.om.Sequence result() {
                return data;
            }
        
        }
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.ItemType functionArgType = args[2].getItemType();
            if ((functionArgType is Saxon.Hej.type.AnyFunctionType)) {
                return ((Saxon.Hej.type.AnyFunctionType)(args[2].getItemType())).getResultType().getPrimaryType();
            } else {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
    
    }
}