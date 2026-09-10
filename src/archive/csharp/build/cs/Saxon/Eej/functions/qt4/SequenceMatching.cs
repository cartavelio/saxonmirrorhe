using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public abstract class SequenceMatching : Saxon.Hej.functions.SystemFunction {
        protected internal Saxon.Hej.value.BooleanValue startsWith(Saxon.Hej.om.SequenceIterator iter0, Saxon.Hej.om.SequenceIterator iter1, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.FunctionItem comparator) {
            Saxon.Hej.om.Item it0, it1;
            if ((comparator == null)) {
                Saxon.Hej.expr.sort.AtomicComparer comparer = Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, context.getConfiguration().getCollation(getRetainedStaticContext().getDefaultCollationName()), context);
                while (true) {
                    it0 = iter0.next();
                    it1 = iter1.next();
                    if ((it1 == null)) {
                        return Saxon.Hej.value.BooleanValue.TRUE;
                    }
                    if ((it0 == null)) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                    bool match = Saxon.Hej.functions.DeepEqual.deepEqual(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(it0), Saxon.Hej.tree.iter.SingletonIterator.makeIterator(it1), comparer, context, 0);
                    if (!(match)) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                }
            } else {
                while (true) {
                    it0 = iter0.next();
                    it1 = iter1.next();
                    if ((it1 == null)) {
                        return Saxon.Hej.value.BooleanValue.TRUE;
                    }
                    if ((it0 == null)) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                    Saxon.Hej.om.Sequence result = comparator.call(context, new Saxon.Hej.om.Sequence[]{it0, it1});
                    if (!(((Saxon.Hej.value.BooleanValue)(result.head())).getBooleanValue())) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                }
            }
        }
        public class StartsWithSequence : Saxon.Eej.functions.qt4.SequenceMatching {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.SequenceIterator iter0 = arguments[0].iterate();
                Saxon.Hej.om.SequenceIterator iter1 = arguments[1].iterate();
                Saxon.Hej.om.FunctionItem comparator = ((arguments.Length == 3) ? (Saxon.Hej.om.FunctionItem)(arguments[2].head()) : null);
                return startsWith(iter0, iter1, context, comparator);
            }
        
        }
        public class EndsWithSequence : Saxon.Eej.functions.qt4.SequenceMatching {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.SequenceIterator iter0 = Saxon.Hej.functions.Reverse.getReverseIterator(arguments[0].iterate());
                Saxon.Hej.om.SequenceIterator iter1 = Saxon.Hej.functions.Reverse.getReverseIterator(arguments[1].iterate());
                Saxon.Hej.om.FunctionItem comparator = ((arguments.Length == 3) ? (Saxon.Hej.om.FunctionItem)(arguments[2].head()) : null);
                return startsWith(iter0, iter1, context, comparator);
            }
        
        }
        public class ContainsSequence : Saxon.Eej.functions.qt4.SequenceMatching {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.GroundedValue seq0 = arguments[0].materialize();
                Saxon.Hej.om.GroundedValue seq1 = arguments[1].materialize();
                Saxon.Hej.om.FunctionItem comparator = ((arguments.Length == 3) ? (Saxon.Hej.om.FunctionItem)(arguments[2].head()) : null);
                int len0 = seq0.getLength();
                int len2 = seq1.getLength();
                if ((len2 == 0)) {
                    return Saxon.Hej.value.BooleanValue.TRUE;
                }
                int latestStart = (len0-len2);
                for (int i = 0; (i<=latestStart); i++) {
                    Saxon.Hej.om.SequenceIterator iter0 = Saxon.Hej.expr.TailIterator.make(seq0.iterate(), (i+1));
                    Saxon.Hej.om.SequenceIterator iter1 = seq1.iterate();
                    Saxon.Hej.value.BooleanValue result = startsWith(iter0, iter1, context, comparator);
                    if (result.getBooleanValue()) {
                        return result;
                    }
                }
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
        
        }
    
    }
}