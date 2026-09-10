using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class DistinctValues : Saxon.Hej.functions.CollatingFunctionFixed {
        public static readonly Saxon.Hej.expr.sort.AtomicMatchKey NaN_MATCH_KEY = new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.SAXON, "+NaN+");
        public override string getStreamerName() {
            return "DistinctValues";
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            return new Saxon.Hej.om.LazySequence(new Saxon.Hej.functions.DistinctValues.DistinctIterator(arguments[0].iterate(), collator, context));
        }
        public class DistinctIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.lib.StringCollator collator;
            private readonly Saxon.Hej.expr.XPathContext context;
            private readonly System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey> lookup = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(40);
            private Saxon.Hej.om.Action onDuplicates = null;
            public DistinctIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.XPathContext context)  {
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
                    if (lookup.Add(key)) {
                        return nextBase;
                    } else {
                        if ((onDuplicates != null)) {
                            try {
                                /*Delegate.doAction()*/onDuplicates();
                            } catch (Saxon.Hej.trans.XPathException e) {
                                throw new Saxon.Hej.trans.UncheckedXPathException(e);
                            }
                        }
                    }
                }
            }
            public void close() {
                @base.close();
            }
            public void notifyDuplicates(Saxon.Hej.om.Action onDuplicates) {
                this.onDuplicates = onDuplicates;
            }
        
        }
    
    }
}