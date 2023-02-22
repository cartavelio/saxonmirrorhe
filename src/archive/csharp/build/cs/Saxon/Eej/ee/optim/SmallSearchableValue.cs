using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    internal class SmallSearchableValue : Saxon.Eej.ee.optim.SearchableValue {
        internal SmallSearchableValue(Saxon.Hej.om.Item[] array, int end)  {
            this.value = array;
            this.end = end;
        }
        private delegate bool EqualityComparer (Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b);
        public override Saxon.Hej.om.SequenceIterator findItems(Saxon.Hej.expr.Expression use, Saxon.Hej.om.SequenceIterator values, bool convertUntypedToOther, bool firstOnly, Saxon.Hej.expr.sort.AtomicComparer comparer, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> selected = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            Saxon.Hej.expr.XPathContext c2 = context.newContext();
            Saxon.Hej.tree.iter.ManualIterator manual = new Saxon.Hej.tree.iter.ManualIterator();
            c2.setCurrentIterator(manual);
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> soughtValues = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
            Saxon.Hej.om.SequenceTool.supply(values, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => soughtValues.Add((Saxon.Hej.value.AtomicValue)(item)))));
            if (soughtValues.Count==0) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            }
            Saxon.Eej.ee.optim.SmallSearchableValue.EqualityComparer compare;
            if (convertUntypedToOther) {
                compare = (a, b) => Saxon.Hej.expr.GeneralComparison.compare(a, Saxon.Hej.expr.parser.Token.FEQ, b, comparer, true, c2, use.getRetainedStaticContext());
            } else {
                compare = (a, b) => Saxon.Hej.expr.ValueComparison.compare(a, Saxon.Hej.expr.parser.Token.FEQ, b, comparer, true);
            }
            for (int i = 0; (i<end); i++) {
                manual.setContextItem(value[i]);
                Saxon.Hej.om.SequenceIterator keys = use.iterate(c2);
                Saxon.Hej.om.Item key;
                while (((key = keys.next()) != null)) {
                    Saxon.Hej.om.AtomicSequence atomized = key.atomize();
                    foreach (Saxon.Hej.value.AtomicValue sought in soughtValues) {
                        foreach (Saxon.Hej.value.AtomicValue val in atomized) {
                            if (/*Delegate.compare()*/compare(val, sought)) {
                                if (firstOnly) {
                                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(value[i]);
                                } else {
                                    selected.Add(value[i]);
                                }
                            }
                        }
                    }
                }
            }
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(selected);
        }
    
    }
}