using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class TailIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private readonly int start;
        private TailIterator(Saxon.Hej.om.SequenceIterator @base, int start)  {
            this.@base = @base;
            this.start = start;
        }
        public static Saxon.Hej.om.SequenceIterator make(Saxon.Hej.om.SequenceIterator @base, int start) {
            if ((start<=1)) {
                return @base;
            } else {
                if ((@base is Saxon.Hej.tree.iter.ArrayIterator)) {
                    return ((Saxon.Hej.tree.iter.ArrayIterator)(@base)).makeSliceIterator(start, System.Int32.MaxValue);
                } else {
                    if (((@base is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(@base)).isActuallyGrounded())) {
                        Saxon.Hej.om.GroundedValue value = Saxon.Hej.om.SequenceTool.toGroundedValue(@base);
                        if ((start>value.getLength())) {
                            return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                        } else {
                            return new Saxon.Hej.expr.ValueTailIterator(value, (start-1));
                        }
                    } else {
                        for (int i = 0; (i<(start-1)); i++) {
                            Saxon.Hej.om.Item b = @base.next();
                            if ((b == null)) {
                                return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                            }
                        }
                        return new Saxon.Hej.expr.TailIterator(@base, start);
                    }
                }
            }
        }
        public Saxon.Hej.om.Item next() {
            return @base.next();
        }
        public bool supportsHasNext() {
            return ((@base is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext());
        }
        public bool hasNext() {
            return ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).hasNext();
        }
        public bool supportsGetLength() {
            return Saxon.Hej.om.SequenceTool.supportsGetLength(@base);
        }
        public int getLength() {
            int bl = ((Saxon.Hej.om.SequenceTool.getLength(@base)-start)+1);
            return System.Math.Max(bl, 0);
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
    
    }
}