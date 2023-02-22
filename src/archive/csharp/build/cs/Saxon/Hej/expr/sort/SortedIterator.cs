using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SortedIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        protected internal Saxon.Hej.om.SequenceIterator @base;
        protected internal Saxon.Hej.expr.sort.SortKeyEvaluator sortKeyEvaluator;
        protected internal Saxon.Hej.expr.sort.AtomicComparer[] comparators;
        protected internal Saxon.Hej.expr.sort.ObjectToBeSorted[] values;
        protected internal int count = -1;
        protected internal int position = 0;
        protected internal Saxon.Hej.expr.XPathContext context;
        private Saxon.Api.HostLanguage hostLanguage;
        protected internal SortedIterator()  {
        }
        public SortedIterator(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.sort.SortKeyEvaluator sortKeyEvaluator, Saxon.Hej.expr.sort.AtomicComparer[] comparators, bool createNewContext)  {
            if (createNewContext) {
                this.context = context.newMinorContext();
                this.@base = this.context.trackFocus(@base);
                this.context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_SORT);
            } else {
                this.@base = @base;
                this.context = context;
            }
            this.sortKeyEvaluator = sortKeyEvaluator;
            this.comparators = new Saxon.Hej.expr.sort.AtomicComparer[comparators.Length];
            for (int n = 0; (n<comparators.Length); n++) {
                this.comparators[n] = comparators[n].provideContext(context);
            }
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            hostLanguage = language;
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            if ((position<0)) {
                return false;
            }
            if ((count<0)) {
                if (((@base is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext())) {
                    return ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).hasNext();
                } else {
                    try {
                        doSort();
                        return (count>0);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        count = -1;
                        @base = new Saxon.Hej.om.FocusTrackingIterator(new Saxon.Hej.expr.ErrorIterator(err));
                        return true;
                    }
                }
            } else {
                return (position<count);
            }
        }
        public Saxon.Hej.om.Item next() {
            if ((position<0)) {
                return null;
            }
            if ((count<0)) {
                try {
                    doSort();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            if ((position<count)) {
                return (Saxon.Hej.om.Item)(values[position++].value);
            } else {
                position = -1;
                return null;
            }
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            if ((count<0)) {
                try {
                    doSort();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            return count;
        }
        protected internal virtual void buildArray() {
            int allocated = (Saxon.Hej.om.SequenceTool.supportsGetLength(@base) ? Saxon.Hej.om.SequenceTool.getLength(@base) : 100);
            values = new Saxon.Hej.expr.sort.ItemToBeSorted[allocated];
            count = 0;
            Saxon.Hej.om.Item item;
            while (((item = @base.next()) != null)) {
                if ((count == allocated)) {
                    allocated *= 2;
                    Saxon.Hej.expr.sort.ObjectToBeSorted[] nk2 = new Saxon.Hej.expr.sort.ObjectToBeSorted[allocated];
                    Saxon.Ejava.lang.System.arraycopy(values, 0, nk2, 0, count);
                    values = nk2;
                }
                Saxon.Hej.expr.sort.ItemToBeSorted itbs = new Saxon.Hej.expr.sort.ItemToBeSorted(comparators.Length);
                values[count] = itbs;
                itbs.value = item;
                for (int n = 0; (n<comparators.Length); n++) {
                    itbs.sortKeyValues[n] = sortKeyEvaluator.evaluateSortKey(n, context);
                }
                itbs.originalPosition = count++;
            }
            if ((((allocated*2)<count) || (((allocated-count))>2000))) {
                Saxon.Hej.expr.sort.ObjectToBeSorted[] nk2 = new Saxon.Hej.expr.sort.ObjectToBeSorted[count];
                Saxon.Ejava.lang.System.arraycopy(values, 0, nk2, 0, count);
                values = nk2;
            }
        }
        private class SortComparer : System.Collections.Generic.Comparer<Saxon.Hej.expr.sort.ObjectToBeSorted> {
            private Saxon.Hej.expr.sort.AtomicComparer[] comparators;
            public SortComparer(Saxon.Hej.expr.sort.AtomicComparer[] comparators)  {
                this.comparators = comparators;
            }
            public int compare(Saxon.Hej.expr.sort.ObjectToBeSorted a, Saxon.Hej.expr.sort.ObjectToBeSorted b) {
                try {
                    for (int i = 0; (i<comparators.Length); i++) {
                        int comp = comparators[i].compareAtomicValues(a.sortKeyValues[i], b.sortKeyValues[i]);
                        if ((comp != 0)) {
                            return comp;
                        }
                    }
                } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                    throw new Saxon.Ejava.lang.AssertionError(("Sorting without dynamic context: "+e.Message));
                }
                return (a.originalPosition-b.originalPosition);
            }
            // Extra method to make the class a C# Comparer
            public override int Compare(Saxon.Hej.expr.sort.ObjectToBeSorted a, Saxon.Hej.expr.sort.ObjectToBeSorted b) {
                return compare(a, b);
            }
        
        }
        private void doSort() {
            buildArray();
            if ((count<2)) {
                return ;
            }
            try {
                Saxon.Ejava.util.Arrays.sort(values, 0, count, new Saxon.Hej.expr.sort.SortedIterator.SortComparer(comparators));
            } catch (System.InvalidCastException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Non-comparable types found while sorting: "+e.Message));
                if ((hostLanguage == Saxon.Api.HostLanguage.XSLT)) {
                    err.setErrorCode("XTDE1030");
                } else {
                    err.setErrorCode("XPTY0004");
                }
                throw err;
            }
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}