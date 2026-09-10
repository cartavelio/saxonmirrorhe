using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class AxisAtomizingIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.AtomizedValueIterator @base;
        private Saxon.Hej.om.AtomicSequence results = null;
        private int atomicPosition = 0;
        public AxisAtomizingIterator(Saxon.Hej.om.AtomizedValueIterator @base)  {
            this.@base = @base;
        }
        public Saxon.Hej.om.Item next() {
            while (true) {
                if ((results != null)) {
                    if ((atomicPosition<results.getLength())) {
                        return ((/*1430*/Saxon.Hej.value.AtomicValue)results.itemAt(atomicPosition++));
                    } else {
                        results = null;
                        continue ;
                    }
                }
                try {
                    Saxon.Hej.om.AtomicSequence atomized = @base.nextAtomizedValue();
                    if ((atomized == null)) {
                        results = null;
                        return null;
                    }
                    if ((atomized is Saxon.Hej.value.AtomicValue)) {
                        results = null;
                        return (Saxon.Hej.value.AtomicValue)(atomized);
                    } else {
                        results = atomized;
                        atomicPosition = 0;
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
        }
        public void close() {
            @base.close();
        }
    
    }
}