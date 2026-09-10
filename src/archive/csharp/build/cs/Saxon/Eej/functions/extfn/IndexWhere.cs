using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class IndexWhere : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.PLUS_ONE, Saxon.Hej.expr.Expression.MAX_SEQUENCE_LENGTH};
        }
        private class IndexIterator : Saxon.Hej.om.SequenceIterator {
            private int index = 0;
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.om.Function predicate;
            private readonly Saxon.Hej.expr.XPathContext context;
            public IndexIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Function predicate)  {
                this.@base = @base;
                this.context = context;
                this.predicate = predicate;
            }
            public void close() {
                @base.close();
            }
            public void discharge() {
                @base.discharge();
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.Item baseItem;
                while (((baseItem = @base.next()) != null)) {
                    index++;
                    try {
                        Saxon.Hej.om.Sequence result = dynamicCall(predicate, context, new Saxon.Hej.om.Sequence[]{baseItem});
                        Saxon.Hej.value.BooleanValue val = (Saxon.Hej.value.BooleanValue)(result.head());
                        if (((val != null) && val.getBooleanValue())) {
                            return new Saxon.Hej.value.Int64Value(index);
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                }
                return null;
            }
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator seq = arguments[0].iterate();
            Saxon.Hej.om.Function predicate = (Saxon.Hej.om.Function)(arguments[1].head());
            Saxon.Hej.om.SequenceIterator iter = new Saxon.Eej.functions.extfn.IndexWhere.IndexIterator(seq, context, predicate);
            return new Saxon.Hej.om.LazySequence(iter);
        }
    
    }
}