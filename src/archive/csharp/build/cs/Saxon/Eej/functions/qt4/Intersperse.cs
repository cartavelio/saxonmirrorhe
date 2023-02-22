using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class Intersperse : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator input = arguments[0].iterate();
            Saxon.Hej.om.GroundedValue separator = arguments[1].materialize();
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> result = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
            Saxon.Hej.om.Item first = input.next();
            if ((first != null)) {
                result = result.add(first);
                for (Saxon.Hej.om.Item it; ((it = input.next()) != null); ) {
                    result = result.addAll(separator.asIterable());
                    result = result.add(it);
                }
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(result);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_9(this, this, arguments);
        }
    
        private class Inner_SystemFunctionCall_9 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly Intersperse outerInstance;
            public Inner_SystemFunctionCall_9(Intersperse outerInstance, Intersperse arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.type.ItemType getItemType() {
                return Saxon.Hej.type.Type.getCommonSuperType(/*superMethod*/getArg(0).getItemType(), /*superMethod*/getArg(1).getItemType());
            }
        }

    }
}