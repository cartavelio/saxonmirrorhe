using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntUniversalSet : Saxon.Hej.z.IntSet {
        private static readonly Saxon.Hej.z.IntUniversalSet THE_INSTANCE = new Saxon.Hej.z.IntUniversalSet();
        public static Saxon.Hej.z.IntUniversalSet getInstance() {
            return THE_INSTANCE;
        }
        private IntUniversalSet()  {
        }
        public override Saxon.Hej.z.IntSet copy() {
            return this;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return new Saxon.Hej.z.IntComplementSet(new Saxon.Hej.z.IntHashSet());
        }
        public override bool isMutable() {
            return false;
        }
        public override void clear() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntUniversalSet is immutable");
        }
        public override int size() {
            return System.Int32.MaxValue;
        }
        public override bool isEmpty() {
            return false;
        }
        public override bool contains(int value) {
            return true;
        }
        public override bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntUniversalSet is immutable");
        }
        public override bool add(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntUniversalSet is immutable");
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot enumerate an infinite set");
        }
        public override Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
            return this;
        }
        public override Saxon.Hej.z.IntSet intersect(Saxon.Hej.z.IntSet other) {
            return other.copy();
        }
        public override Saxon.Hej.z.IntSet except(Saxon.Hej.z.IntSet other) {
            if ((other is Saxon.Hej.z.IntUniversalSet)) {
                return Saxon.Hej.z.IntEmptySet.getInstance();
            } else {
                return new Saxon.Hej.z.IntComplementSet(other.copy());
            }
        }
        public override bool containsAll(Saxon.Hej.z.IntSet other) {
            return true;
        }
    
    }
}