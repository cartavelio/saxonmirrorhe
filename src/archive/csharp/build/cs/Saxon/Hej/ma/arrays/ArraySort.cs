using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    public class ArraySort : Saxon.Hej.functions.SystemFunction {
        private class MemberToBeSorted {
            public Saxon.Hej.om.GroundedValue value;
            public Saxon.Hej.om.GroundedValue sortKey;
            internal int originalPosition;
        
        }
        public override Saxon.Hej.ma.arrays.ArrayItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.ma.arrays.ArrayItem array = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[0].head());
            System.Collections.Generic.List<Saxon.Hej.ma.arrays.ArraySort.MemberToBeSorted> inputList = new System.Collections.Generic.List<Saxon.Hej.ma.arrays.ArraySort.MemberToBeSorted>(array.arrayLength());
            int i = 0;
            Saxon.Hej.lib.StringCollator collation;
            if ((arguments.Length == 1)) {
                collation = context.getConfiguration().getCollation(getRetainedStaticContext().getDefaultCollationName());
            } else {
                Saxon.Hej.value.StringValue collName = (Saxon.Hej.value.StringValue)(arguments[1].head());
                if ((collName == null)) {
                    collation = context.getConfiguration().getCollation(getRetainedStaticContext().getDefaultCollationName());
                } else {
                    collation = context.getConfiguration().getCollation(collName.getStringValue(), getStaticBaseUriString());
                }
            }
            Saxon.Hej.om.Function key = null;
            if ((arguments.Length == 3)) {
                key = (Saxon.Hej.om.Function)(arguments[2].head());
            }
            foreach (Saxon.Hej.om.GroundedValue seq in array.members()) {
                Saxon.Hej.ma.arrays.ArraySort.MemberToBeSorted member = new Saxon.Hej.ma.arrays.ArraySort.MemberToBeSorted();
                member.value = seq;
                member.originalPosition = i++;
                if ((key != null)) {
                    member.sortKey = dynamicCall(key, context, new Saxon.Hej.om.Sequence[]{seq}).materialize();
                } else {
                    member.sortKey = atomize(seq);
                }
                inputList.Add(member);
            }
            Saxon.Hej.expr.sort.AtomicComparer atomicComparer = Saxon.Hej.expr.sort.AtomicSortComparer.makeSortComparer(collation, Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE, context);
            try {
                inputList.Sort((a, b) => {
    int result = compareSortKeys(a.sortKey, b.sortKey, atomicComparer);
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
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> outputList = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(array.arrayLength());
            foreach (Saxon.Hej.ma.arrays.ArraySort.MemberToBeSorted member in inputList) {
                outputList.Add(member.value);
            }
            return new Saxon.Hej.ma.arrays.SimpleArrayItem(outputList);
        }
        public static int compareSortKeys(Saxon.Hej.om.GroundedValue a, Saxon.Hej.om.GroundedValue b, Saxon.Hej.expr.sort.AtomicComparer comparer) {
            Saxon.Hej.om.SequenceIterator iteratora = a.iterate();
            Saxon.Hej.om.SequenceIterator iteratorb = b.iterate();
            while (true) {
                Saxon.Hej.value.AtomicValue firsta = (Saxon.Hej.value.AtomicValue)(iteratora.next());
                Saxon.Hej.value.AtomicValue firstb = (Saxon.Hej.value.AtomicValue)(iteratorb.next());
                if ((firsta == null)) {
                    if ((firstb == null)) {
                        return 0;
                    } else {
                        return -1;
                    }
                } else {
                    if ((firstb == null)) {
                        return +(1);
                    } else {
                        try {
                            int first = comparer.compareAtomicValues(firsta, firstb);
                            if ((first == 0)) {
                                continue ;
                            } else {
                                return first;
                            }
                        } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                            throw new Saxon.Ejava.lang.AssertionError(e);
                        }
                    }
                }
            }
        }
        private static Saxon.Hej.om.GroundedValue atomize(Saxon.Hej.om.Sequence input) {
            Saxon.Hej.om.SequenceIterator iterator = input.iterate();
            Saxon.Hej.om.SequenceIterator mapper = Saxon.Hej.expr.Atomizer.getAtomizingIterator(iterator, false);
            return Saxon.Hej.om.SequenceTool.toGroundedValue(mapper);
        }
    
    }
}