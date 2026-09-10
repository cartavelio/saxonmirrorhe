using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class FilterFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(evalFilter((Saxon.Hej.om.FunctionItem)(arguments[1].head()), arguments[0].iterate(), context));
        }
        private Saxon.Hej.om.SequenceIterator evalFilter(Saxon.Hej.om.FunctionItem function, Saxon.Hej.om.SequenceIterator basis, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.expr.ItemMappingIterator.filter(basis, item => ((Saxon.Hej.value.BooleanValue)(dynamicCall(function, context, new Saxon.Hej.om.Sequence[]{item}).head())).getBooleanValue());
        }
        public override string getStreamerName() {
            return "FilterFn";
        }
    
    }
}