using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public abstract class ListIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.om.FocusIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.ReversibleIterator {
        public bool supportsHasNext() {
            return true;
        }
        public class Of<T>  : Saxon.Hej.tree.iter.ListIterator where T : Saxon.Hej.om.Item {
            private int index = 0;
            protected internal System.Collections.Generic.IList<T> list;
            public Of(System.Collections.Generic.IList<T> list)  {
                index = 0;
                this.list = list;
            }
            public override bool hasNext() {
                return (index<list.Count);
            }
            public override Saxon.Hej.om.Item next() {
                if ((index>=list.Count)) {
                    return null;
                }
                return list[index++];
            }
            public override bool supportsGetLength() {
                return true;
            }
            public override int getLength() {
                return list.Count;
            }
            public override Saxon.Hej.om.Item current() {
                return list[(index-1)];
            }
            public override int position() {
                return index;
            }
            public override bool isActuallyGrounded() {
                return true;
            }
            public override Saxon.Hej.om.GroundedValue materialize() {
                return new Saxon.Hej.value.SequenceExtent.Of<T>(list);
            }
            public override Saxon.Hej.om.GroundedValue getResidue() {
                System.Collections.Generic.IList<T> l2 = list;
                if ((index != 0)) {
                    l2 = Saxon.Impl.Helpers.ListUtils.SubList(l2, index, l2.Count);
                }
                return new Saxon.Hej.value.SequenceExtent.Of<T>(l2);
            }
            public override Saxon.Hej.om.SequenceIterator getReverseIterator() {
                return Saxon.Hej.functions.Reverse.reverseIterator(list);
            }
        
        }
        public class OfAtomic<A>  : Saxon.Hej.tree.iter.ListIterator.Of<A>, Saxon.Hej.tree.iter.AtomicIterator where A : Saxon.Hej.value.AtomicValue {
            public OfAtomic(System.Collections.Generic.IList<A> nodes) : base (nodes) {
            }
            public override Saxon.Hej.value.AtomicValue next() {
                return (Saxon.Hej.value.AtomicValue)(base.next());
            }
        
        }
    
        /* Expanding default method materialize from interface GroundedIterator */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.value.SequenceExtent.from(this).reduce();
        }

        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Creating stub method hasNext to satisfy interface LookaheadIterator */
        public virtual bool hasNext() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getReverseIterator to satisfy interface ReversibleIterator */
        public virtual Saxon.Hej.om.SequenceIterator getReverseIterator() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isActuallyGrounded to satisfy interface GroundedIterator */
        public virtual bool isActuallyGrounded() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getResidue to satisfy interface GroundedIterator */
        public virtual Saxon.Hej.om.GroundedValue getResidue() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method current to satisfy interface FocusIterator */
        public virtual Saxon.Hej.om.Item current() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method position to satisfy interface FocusIterator */
        public virtual int position() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getLength to satisfy interface FocusIterator */
        /* (Also inherited from LastPositionFinder) */
        public virtual int getLength() {
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

    }
}