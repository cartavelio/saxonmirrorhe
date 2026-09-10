using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class ForEachPairFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.ItemType fnType = args[2].getItemType();
            if ((fnType is Saxon.Hej.type.SpecificFunctionType)) {
                return ((Saxon.Hej.type.SpecificFunctionType)(fnType)).getResultType().getPrimaryType();
            } else {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(evalMapPairs((Saxon.Hej.om.Function)(arguments[2].head()), arguments[0].iterate(), arguments[1].iterate(), context));
        }
        private Saxon.Hej.om.SequenceIterator evalMapPairs(Saxon.Hej.om.Function function, Saxon.Hej.om.SequenceIterator seq0, Saxon.Hej.om.SequenceIterator seq1, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.functions.hof.ForEachPairFn.PairedSequenceIterator pairs = new Saxon.Hej.functions.hof.ForEachPairFn.PairedSequenceIterator(seq0, seq1);
            return Saxon.Hej.expr.MappingIterator.map(pairs, item => {
                Saxon.Hej.om.Sequence[] pair = ((Saxon.Hej.value.ObjectValue<Saxon.Hej.om.Sequence[]>)(item)).getObject();
                return dynamicCall(function, context, pair).iterate();
            } );
        }
        private class PairedSequenceIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator seq0;
            private readonly Saxon.Hej.om.SequenceIterator seq1;
            private readonly Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[2];
            public PairedSequenceIterator(Saxon.Hej.om.SequenceIterator seq0, Saxon.Hej.om.SequenceIterator seq1)  {
                this.seq0 = seq0;
                this.seq1 = seq1;
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.Item i0 = seq0.next();
                if ((i0 == null)) {
                    close();
                    return null;
                }
                Saxon.Hej.om.Item i1 = seq1.next();
                if ((i1 == null)) {
                    close();
                    return null;
                }
                args[0] = i0;
                args[1] = i1;
                return new Saxon.Hej.value.ObjectValue<Saxon.Hej.om.Sequence[]>(args);
            }
            public void close() {
                seq0.close();
                seq1.close();
            }
        
        }
    
    }
}