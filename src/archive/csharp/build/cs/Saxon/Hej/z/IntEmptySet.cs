using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntEmptySet : Saxon.Hej.z.IntSet {
        private static readonly Saxon.Hej.z.IntEmptySet THE_INSTANCE = new Saxon.Hej.z.IntEmptySet();
        public static Saxon.Hej.z.IntEmptySet getInstance() {
            return THE_INSTANCE;
        }
        private IntEmptySet()  {
        }
        public override Saxon.Hej.z.IntSet copy() {
            return this;
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return new Saxon.Hej.z.IntHashSet();
        }
        public override bool isMutable() {
            return false;
        }
        public override void clear() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntEmptySet is immutable");
        }
        public override int size() {
            return 0;
        }
        public override bool isEmpty() {
            return true;
        }
        public override bool contains(int value) {
            return false;
        }
        public override bool remove(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntEmptySet is immutable");
        }
        public override bool add(int value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntEmptySet is immutable");
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            return new Saxon.Hej.z.IntEmptySet.EmptyIntIterator();
        }
        private class EmptyIntIterator : Saxon.Hej.z.IntIterator {
            public bool hasNext() {
                return false;
            }
            public int next() {
                return System.Int32.MinValue;
            }
        
        }
        public override Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
            return other.copy();
        }
        public override Saxon.Hej.z.IntSet intersect(Saxon.Hej.z.IntSet other) {
            return this;
        }
        public override Saxon.Hej.z.IntSet except(Saxon.Hej.z.IntSet other) {
            return this;
        }
        public override bool containsAll(Saxon.Hej.z.IntSet other) {
            return other.isEmpty();
        }
    
    }
}