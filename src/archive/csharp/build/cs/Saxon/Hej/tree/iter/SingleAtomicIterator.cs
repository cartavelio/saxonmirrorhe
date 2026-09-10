using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class SingleAtomicIterator : Saxon.Hej.tree.iter.SingletonIterator, Saxon.Hej.tree.iter.AtomicIterator, Saxon.Hej.tree.iter.ReversibleIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        public SingleAtomicIterator(Saxon.Hej.value.AtomicValue value) : base (value) {
        }
        public override Saxon.Hej.tree.iter.SingleAtomicIterator getReverseIterator() {
            return new Saxon.Hej.tree.iter.SingleAtomicIterator((Saxon.Hej.value.AtomicValue)(getValue()));
        }
        public override Saxon.Hej.value.AtomicValue next() {
            return (Saxon.Hej.value.AtomicValue)(base.next());
        }
    
    }
}