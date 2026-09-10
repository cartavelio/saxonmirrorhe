using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class LookaheadIteratorImpl : Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private Saxon.Hej.om.Item _next;
        private LookaheadIteratorImpl(Saxon.Hej.om.SequenceIterator @base)  {
            this.@base = @base;
            _next = @base.next();
        }
        public static Saxon.Hej.tree.iter.LookaheadIterator makeLookaheadIterator(Saxon.Hej.om.SequenceIterator @base) {
            if (((@base is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext())) {
                return (Saxon.Hej.tree.iter.LookaheadIterator)(@base);
            } else {
                return new Saxon.Hej.tree.iter.LookaheadIteratorImpl(@base);
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (_next != null);
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item current = _next;
            if ((_next != null)) {
                _next = @base.next();
            }
            return current;
        }
        public void close() {
            @base.close();
        }
    
    }
}