using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class SubsequenceIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private int basePosition = 0;
        private readonly int min;
        private readonly int max;
        private Saxon.Hej.om.Item nextItem = null;
        private SubsequenceIterator(Saxon.Hej.om.SequenceIterator @base, int min, int max)  {
            this.@base = @base;
            this.min = min;
            if ((min<1)) {
                min = 1;
            }
            this.max = max;
            if ((max<min)) {
                nextItem = null;
                return ;
            }
            int i = 1;
            while ((i++<=min)) {
                nextItem = @base.next();
                basePosition++;
                if ((nextItem == null)) {
                    break ;
                }
            }
        }
        public static Saxon.Hej.om.SequenceIterator make(Saxon.Hej.om.SequenceIterator @base, int min, int max) {
            if ((@base is Saxon.Hej.tree.iter.ArrayIterator)) {
                return ((Saxon.Hej.tree.iter.ArrayIterator)(@base)).makeSliceIterator(min, max);
            } else {
                if ((max == System.Int32.MaxValue)) {
                    return Saxon.Hej.expr.TailIterator.make(@base, min);
                } else {
                    if ((((@base is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(@base)).isActuallyGrounded()) && (min>4))) {
                        Saxon.Hej.om.GroundedValue value = Saxon.Hej.om.SequenceTool.toGroundedValue(@base);
                        value = value.subsequence((min-1), ((max-min)+1));
                        return value.iterate();
                    } else {
                        return new Saxon.Hej.expr.SubsequenceIterator(@base, min, max);
                    }
                }
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (nextItem != null);
        }
        public Saxon.Hej.om.Item next() {
            if ((nextItem == null)) {
                return null;
            }
            Saxon.Hej.om.Item current = nextItem;
            if ((basePosition<max)) {
                nextItem = @base.next();
                basePosition++;
            } else {
                nextItem = null;
                @base.close();
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
            int lastBase = Saxon.Hej.om.SequenceTool.getLength(@base);
            int z = System.Math.Min(lastBase, max);
            return System.Math.Max(((z-min)+1), 0);
        }
    
    }
}