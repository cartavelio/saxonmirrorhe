using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Sort_1 : Saxon.Hej.functions.SystemFunction {
        public class ItemToBeSorted {
            public Saxon.Hej.om.Item value;
            public Saxon.Hej.om.GroundedValue sortKey;
            public int originalPosition;
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted> inputList = getItemsToBeSorted(arguments[0]);
            Saxon.Hej.lib.StringCollator collation = context.getConfiguration().getCollation(getRetainedStaticContext().getDefaultCollationName());
            return doSort(inputList, collation, context);
        }
        protected internal System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted> getItemsToBeSorted(Saxon.Hej.om.Sequence input) {
            System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted> inputList = new System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted>();
            int i = 0;
            Saxon.Hej.om.SequenceIterator iterator = input.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iterator.next()) != null)) {
                Saxon.Hej.functions.Sort_1.ItemToBeSorted member = new Saxon.Hej.functions.Sort_1.ItemToBeSorted();
                member.value = item;
                member.originalPosition = i++;
                member.sortKey = item.atomize();
                inputList.Add(member);
            }
            return inputList;
        }
        protected internal Saxon.Hej.om.Sequence doSort(System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted> inputList, Saxon.Hej.lib.StringCollator collation, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.AtomicComparer atomicComparer = Saxon.Hej.expr.sort.AtomicSortComparer.makeSortComparer(collation, Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE, context);
            try {
                Saxon.Impl.Helpers.ListUtils.Sort(inputList, (a, b) => {
    int result = Saxon.Hej.ma.arrays.ArraySort.compareSortKeys(a.sortKey, b.sortKey, atomicComparer);
    if ((result == 0)) {
        return (a.originalPosition-b.originalPosition);
    } else {
        return result;
    }
} );
            } catch (System.InvalidCastException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Non-comparable types found while sorting: "+e.Message));
                err.setErrorCode("XPTY0004");
                throw err;
            }
            System.Collections.Generic.List<Saxon.Hej.om.Item> outputList = new System.Collections.Generic.List<Saxon.Hej.om.Item>(inputList.Count);
            foreach (Saxon.Hej.functions.Sort_1.ItemToBeSorted member in inputList) {
                outputList.Add(member.value);
            }
            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(outputList);
        }
    
    }
}