using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class ForEachFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.ItemType fnType = args[1].getItemType();
            if ((fnType is Saxon.Hej.type.SpecificFunctionType)) {
                return ((Saxon.Hej.type.SpecificFunctionType)(fnType)).getResultType().getPrimaryType();
            } else {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(evalMap((Saxon.Hej.om.Function)(arguments[1].head()), arguments[0].iterate(), context));
        }
        private Saxon.Hej.om.SequenceIterator evalMap(Saxon.Hej.om.Function function, Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.expr.MappingIterator.map(@base, item => dynamicCall(function, context, new Saxon.Hej.om.Sequence[]{item}).iterate());
        }
    
    }
}