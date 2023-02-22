using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public abstract class Ending : Saxon.Hej.functions.SystemFunction {
        protected internal abstract bool includeMatchingItem();
        public class ItemsUntil : Saxon.Eej.functions.extfn.Ending {
            protected internal override bool includeMatchingItem() {
                return true;
            }
            public override string getStreamerName() {
                return "saxon:ItemsUntil";
            }
        
        }
        public class ItemsBefore : Saxon.Eej.functions.extfn.Ending {
            protected internal override bool includeMatchingItem() {
                return false;
            }
            public override string getStreamerName() {
                return "saxon:ItemsBefore";
            }
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence input = arguments[0];
            Saxon.Hej.om.Function endCondition = (Saxon.Hej.om.Function)(arguments[1].head());
            return new Saxon.Hej.om.LazySequence(new Saxon.Eej.functions.extfn.Ending.EndingIterator(input.iterate(), endCondition, includeMatchingItem(), context));
        }
        private class EndingIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator baseIterator;
            private readonly Saxon.Hej.om.Function end;
            private readonly bool inclusive;
            private readonly Saxon.Hej.expr.XPathContext context;
            private bool finished;
            internal EndingIterator(Saxon.Hej.om.SequenceIterator @in, Saxon.Hej.om.Function end, bool inclusive, Saxon.Hej.expr.XPathContext context)  {
                this.baseIterator = @in;
                this.end = end;
                this.inclusive = inclusive;
                this.context = context;
            }
            public Saxon.Hej.om.Item next() {
                try {
                    if (finished) {
                        return null;
                    }
                    Saxon.Hej.om.Item item = baseIterator.next();
                    if ((item == null)) {
                        finished = true;
                        return null;
                    }
                    Saxon.Hej.value.BooleanValue matches = (Saxon.Hej.value.BooleanValue)(end.call(context, Saxon.Hej.om.SequenceTool.fromItems(item)).head());
                    if (matches.getBooleanValue()) {
                        finished = true;
                        if (inclusive) {
                            return item;
                        } else {
                            baseIterator.close();
                            return null;
                        }
                    } else {
                        return item;
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            public void close() {
                baseIterator.close();
            }
        
        }
    
    }
}