using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class FoldRightFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.ItemType functionArgType = args[2].getItemType();
            if ((functionArgType is Saxon.Hej.type.AnyFunctionType)) {
                return ((Saxon.Hej.type.AnyFunctionType)(functionArgType)).getResultType().getPrimaryType();
            } else {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return evalFoldRight((Saxon.Hej.om.FunctionItem)(arguments[2].head()), arguments[1].materialize(), arguments[0].iterate(), context);
        }
        private Saxon.Hej.om.Sequence evalFoldRight(Saxon.Hej.om.FunctionItem function, Saxon.Hej.om.Sequence zero, Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator reverseBase = Saxon.Hej.functions.Reverse.getReverseIterator(@base);
            Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[2];
            Saxon.Hej.om.Item item;
            while (((item = reverseBase.next()) != null)) {
                args[0] = item;
                args[1] = zero.materialize();
                try {
                    zero = dynamicCall(function, context, args);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetContext(context);
                    throw;
                }
            }
            return zero;
        }
    
    }
}