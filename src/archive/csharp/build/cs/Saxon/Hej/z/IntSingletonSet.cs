using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntSingletonSet : Saxon.Hej.z.IntSet {
        private readonly int value;
        public IntSingletonSet(int value)  {
            this.value = value;
        }
        public int getMember() {
            return value;
        }
        public override void clear() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntSingletonSet is immutable");
        }
        public override Saxon.Hej.z.IntSet copy() {
            return this;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            Saxon.Hej.z.IntHashSet intHashSet = new Saxon.Hej.z.IntHashSet();
            intHashSet.add(value);
            return intHashSet;
        }
        public override bool isMutable() {
            return false;
        }
        public override int size() {
            return 1;
        }
        public override bool isEmpty() {
            return false;
        }
        public override bool contains(int value) {
            return (this.value == value);
        }
        public override bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntSingletonSet is immutable");
        }
        public override bool add(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntSingletonSet is immutable");
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return new Saxon.Hej.z.IntSingletonIterator(value);
        }
        public override Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
            Saxon.Hej.z.IntSet n = other.mutableCopy();
            n.add(value);
            return n;
        }
        public override Saxon.Hej.z.IntSet intersect(Saxon.Hej.z.IntSet other) {
            if (other.contains(value)) {
                return this;
            } else {
                return Saxon.Hej.z.IntEmptySet.getInstance();
            }
        }
        public override Saxon.Hej.z.IntSet except(Saxon.Hej.z.IntSet other) {
            if (other.contains(value)) {
                return Saxon.Hej.z.IntEmptySet.getInstance();
            } else {
                return this;
            }
        }
        public override bool containsAll(Saxon.Hej.z.IntSet other) {
            if ((other.size()>1)) {
                return false;
            }
            Saxon.Hej.z.IntIterator ii = other.iterator();
            while (ii.hasNext()) {
                if ((value != ii.next())) {
                    return false;
                }
            }
            return true;
        }
    
    }
}