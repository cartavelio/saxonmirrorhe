using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Data_1 : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            return Saxon.Hej.expr.Atomizer.makeAtomizer(arguments[0], null);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence arg = arguments[0];
            if ((arg is Saxon.Hej.om.Item)) {
                return ((Saxon.Hej.om.Item)(arg)).atomize();
            } else {
                Saxon.Hej.om.SequenceIterator a = Saxon.Hej.expr.Atomizer.getAtomizingIterator(arg.iterate(), false);
                return Saxon.Hej.om.SequenceTool.toLazySequence(a);
            }
        }
    
    }
}