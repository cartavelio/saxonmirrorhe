using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public abstract class ArrayIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.ReversibleIterator {
        protected internal int index;
        protected internal int start;
        protected internal int end;
        public abstract Saxon.Hej.om.SequenceIterator makeSliceIterator(int min, int max);
        public bool isActuallyGrounded() {
            return true;
        }
        public bool supportsHasNext() {
            return true;
        }
        public class Of<T>  : Saxon.Hej.tree.iter.ArrayIterator where T : Saxon.Hej.om.Item {
            protected internal T[] items;
            public Of(T[] items)  {
                this.items = items;
                start = 0;
                end = items.Length;
                index = 0;
            }
            public Of(T[] items, int start, int end)  {
                this.items = items;
                this.end = end;
                this.start = start;
                index = start;
            }
            public override Saxon.Hej.om.SequenceIterator makeSliceIterator(int min, int max) {
                T[] items = getArray();
                int currentStart = getStartPosition();
                int currentEnd = getEndPosition();
                if ((min<1)) {
                    min = 1;
                }
                int newStart = (currentStart+((min-1)));
                if ((newStart<currentStart)) {
                    newStart = currentStart;
                }
                int newEnd = ((max == System.Int32.MaxValue) ? currentEnd : (((newStart+max)-min)+1));
                if ((newEnd>currentEnd)) {
                    newEnd = currentEnd;
                }
                if ((newEnd<=newStart)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                }
                return new Saxon.Hej.tree.iter.ArrayIterator.Of<T>(items, newStart, newEnd);
            }
            public override bool hasNext() {
                return (index<end);
            }
            public override Saxon.Hej.om.Item next() {
                if ((index>=end)) {
                    index = (end+1);
                    return null;
                }
                return items[index++];
            }
            public override bool supportsGetLength() {
                return true;
            }
            public override int getLength() {
                return (end-start);
            }
            public T[] getArray() {
                return items;
            }
            public int getStartPosition() {
                return start;
            }
            public int getEndPosition() {
                return end;
            }
            public override Saxon.Hej.om.GroundedValue materialize() {
                Saxon.Hej.value.SequenceExtent.Of<T> seq;
                if (((start == 0) && (end == items.Length))) {
                    seq = new Saxon.Hej.value.SequenceExtent.Of<T>(items);
                } else {
                    System.Collections.Generic.IList<T> sublist = Saxon.Impl.Helpers.ListUtils.SubList(Saxon.Ejava.util.Arrays.asList(items), start, end);
                    seq = new Saxon.Hej.value.SequenceExtent.Of<T>(sublist);
                }
                return seq.reduce();
            }
            public override Saxon.Hej.om.GroundedValue getResidue() {
                Saxon.Hej.value.SequenceExtent seq;
                if ((((start == 0) && (index == 0)) && (end == items.Length))) {
                    seq = new Saxon.Hej.value.SequenceExtent.Of<T>(items);
                } else {
                    System.Collections.Generic.IList<T> sublist = Saxon.Impl.Helpers.ListUtils.SubList(Saxon.Ejava.util.Arrays.asList(items), (start+index), end);
                    seq = new Saxon.Hej.value.SequenceExtent.Of<T>(sublist);
                }
                return seq.reduce();
            }
            public override Saxon.Hej.om.SequenceIterator getReverseIterator() {
                return Saxon.Hej.functions.Reverse.reverseIterator(Saxon.Ejava.util.Arrays.asList(items));
            }
        
        }
        public class OfNodes<N>  : Saxon.Hej.tree.iter.ArrayIterator.Of<N>, Saxon.Hej.tree.iter.AxisIterator where N : Saxon.Hej.om.NodeInfo {
            public OfNodes(N[] nodes) : base (nodes) {
            }
            public override Saxon.Hej.om.NodeInfo next() {
                return (Saxon.Hej.om.NodeInfo)(base.next());
            }
        
        }
    
        /* Expanding default method materialize from interface GroundedIterator */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.value.SequenceExtent.from(this).reduce();
        }

        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

        /* Creating stub method hasNext to satisfy interface LookaheadIterator */
        public virtual bool hasNext() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getReverseIterator to satisfy interface ReversibleIterator */
        public virtual Saxon.Hej.om.SequenceIterator getReverseIterator() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getResidue to satisfy interface GroundedIterator */
        public virtual Saxon.Hej.om.GroundedValue getResidue() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method next to satisfy interface SequenceIterator */
        public virtual Saxon.Hej.om.Item next() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method supportsGetLength to satisfy interface LastPositionFinder */
        public virtual bool supportsGetLength() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getLength to satisfy interface LastPositionFinder */
        public virtual int getLength() {
            throw new System.NotImplementedException();
        }

    }
}