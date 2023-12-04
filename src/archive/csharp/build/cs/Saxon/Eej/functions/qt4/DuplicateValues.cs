using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class DuplicateValues : Saxon.Hej.functions.CollatingFunctionFixed {
        public static readonly Saxon.Hej.expr.sort.AtomicMatchKey NaN_MATCH_KEY = Saxon.Hej.functions.DistinctValues.NaN_MATCH_KEY;
        public override string getStreamerName() {
            return "DistinctValues";
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            return new Saxon.Hej.om.LazySequence(new Saxon.Eej.functions.qt4.DuplicateValues.DuplicatesIterator(arguments[0].iterate(), collator, context));
        }
        public class DuplicatesIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.lib.StringCollator collator;
            private readonly Saxon.Hej.expr.XPathContext context;
            private readonly System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey> firstValues = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(40);
            private readonly System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey> secondValues = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(40);
            public DuplicatesIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.XPathContext context)  {
                this.@base = @base;
                this.collator = collator;
                this.context = context;
            }
            public Saxon.Hej.om.Item next() {
                int implicitTimezone = context.getImplicitTimezone();
                while (true) {
                    Saxon.Hej.value.AtomicValue nextBase = (Saxon.Hej.value.AtomicValue)(@base.next());
                    if ((nextBase == null)) {
                        return null;
                    }
                    Saxon.Hej.expr.sort.AtomicMatchKey key;
                    if (nextBase.isNaN()) {
                        key = NaN_MATCH_KEY;
                    } else {
                        try {
                            key = nextBase.getXPathMatchKey(collator, implicitTimezone);
                        } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                            throw new Saxon.Hej.trans.UncheckedXPathException(e);
                        }
                    }
                    if (firstValues.Add(key)) {
                    } else {
                        if (secondValues.Add(key)) {
                            return nextBase;
                        }
                    }
                }
            }
            public void close() {
                @base.close();
            }
        
        }
    
    }
}