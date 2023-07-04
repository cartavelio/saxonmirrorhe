using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Remove : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            if (Saxon.Hej.expr.Literal.isAtomic(arguments[1])) {
                Saxon.Hej.om.Sequence index = ((Saxon.Hej.expr.Literal)(arguments[1])).getGroundedValue();
                if ((index is Saxon.Hej.value.IntegerValue)) {
                    try {
                        long value = ((Saxon.Hej.value.IntegerValue)(index)).longValue();
                        if ((value<=0)) {
                            return arguments[0];
                        } else {
                            if ((value == 1)) {
                                return new Saxon.Hej.expr.TailExpression(arguments[0], 2);
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException err) {
                    }
                }
            }
            return base.makeFunctionCall(arguments);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.z.IntSet removePositions;
            if ((arguments[1] is Saxon.Hej.value.AtomicValue)) {
                Saxon.Hej.value.NumericValue n = (Saxon.Hej.value.NumericValue)(arguments[1].head());
                int pos = (int)(n.longValue());
                if ((pos<1)) {
                    return arguments[0];
                }
                removePositions = new Saxon.Hej.z.IntSingletonSet(pos);
            } else {
                Saxon.Hej.z.IntHashSet positions = new Saxon.Hej.z.IntHashSet();
                Saxon.Hej.value.NumericValue n;
                Saxon.Hej.om.SequenceIterator iter = arguments[1].iterate();
                while (((n = (Saxon.Hej.value.NumericValue)(iter.next())) != null)) {
                    int pos = (int)(n.longValue());
                    if ((pos>=1)) {
                        positions.add(pos);
                    }
                }
                if (positions.isEmpty()) {
                    return arguments[0];
                }
                removePositions = positions;
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Hej.functions.Remove.RemoveIterator(arguments[0].iterate(), removePositions));
        }
        public class RemoveIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder {
            internal Saxon.Hej.om.SequenceIterator @base;
            internal Saxon.Hej.z.IntSet removePositions;
            internal int basePosition = 0;
            internal Saxon.Hej.om.Item current = null;
            public RemoveIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.z.IntSet removePosition)  {
                this.@base = @base;
                this.removePositions = removePosition;
            }
            public Saxon.Hej.om.Item next() {
                current = @base.next();
                basePosition++;
                while (((current != null) && removePositions.contains(basePosition))) {
                    current = @base.next();
                    basePosition++;
                }
                return current;
            }
            public void close() {
                @base.close();
            }
            public bool supportsGetLength() {
                return Saxon.Hej.om.SequenceTool.supportsGetLength(@base);
            }
            public int getLength() {
                int x = Saxon.Hej.om.SequenceTool.getLength(@base);
                int result = x;
                Saxon.Hej.z.IntIterator iter = removePositions.iterator();
                while (iter.hasNext()) {
                    int i = iter.next();
                    if (((i>=1) && (i<=x))) {
                        result--;
                    }
                }
                return result;
            }
        
        }
        public override string getStreamerName() {
            return "Remove";
        }
    
    }
}