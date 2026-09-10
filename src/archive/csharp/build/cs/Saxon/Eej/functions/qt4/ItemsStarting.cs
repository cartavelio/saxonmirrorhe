using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public abstract class ItemsStarting : Saxon.Hej.functions.SystemFunction {
        protected internal abstract bool includeMatchingItem();
        public class ItemsFrom : Saxon.Eej.functions.qt4.ItemsStarting {
            protected internal override bool includeMatchingItem() {
                return true;
            }
            public override string getStreamerName() {
                return "saxon:ItemsFrom";
            }
        
        }
        public class ItemsAfter : Saxon.Eej.functions.qt4.ItemsStarting {
            protected internal override bool includeMatchingItem() {
                return false;
            }
            public override string getStreamerName() {
                return "saxon:ItemsAfter";
            }
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence input = arguments[0];
            Saxon.Hej.om.FunctionItem startCondition = (Saxon.Hej.om.FunctionItem)(arguments[1].head());
            return new Saxon.Hej.om.LazySequence(new Saxon.Eej.functions.qt4.ItemsStarting.StartingIterator(input.iterate(), startCondition, includeMatchingItem(), context));
        }
        private class StartingIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator baseIterator;
            private readonly Saxon.Hej.om.FunctionItem start;
            private readonly bool inclusive;
            private readonly Saxon.Hej.expr.XPathContext context;
            private bool started;
            internal StartingIterator(Saxon.Hej.om.SequenceIterator @in, Saxon.Hej.om.FunctionItem start, bool inclusive, Saxon.Hej.expr.XPathContext context)  {
                this.baseIterator = @in;
                this.start = start;
                this.inclusive = inclusive;
                this.context = context;
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.Item item;
                while (((item = baseIterator.next()) != null)) {
                    if (started) {
                        return item;
                    }
                    try {
                        Saxon.Hej.value.BooleanValue matches = (Saxon.Hej.value.BooleanValue)(start.call(context, Saxon.Hej.om.SequenceTool.fromItems(item)).head());
                        if (matches.getBooleanValue()) {
                            started = true;
                            if (inclusive) {
                                return item;
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                }
                return null;
            }
            public void close() {
                baseIterator.close();
            }
        
        }
    
    }
}