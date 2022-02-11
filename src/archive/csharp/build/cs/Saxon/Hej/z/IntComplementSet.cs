using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntComplementSet : Saxon.Hej.z.IntSet {
        private readonly Saxon.Hej.z.IntSet exclusions;
        public IntComplementSet(Saxon.Hej.z.IntSet exclusions)  {
            this.exclusions = exclusions.copy();
        }
        public Saxon.Hej.z.IntSet getExclusions() {
            return exclusions;
        }
        public override Saxon.Hej.z.IntSet copy() {
            return new Saxon.Hej.z.IntComplementSet(exclusions.copy());
        }
        public override Saxon.Hej.z.IntSet mutableCopy() {
            return copy();
        }
        public override void clear() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("IntComplementSet cannot be emptied");
        }
        public override int size() {
            return (System.Int32.MaxValue-exclusions.size());
        }
        public override bool isEmpty() {
            return (size() == 0);
        }
        public override bool contains(int value) {
            return !(exclusions.contains(value));
        }
        public override bool remove(int value) {
            bool b = contains(value);
            if (b) {
                exclusions.add(value);
            }
            return b;
        }
        public override bool add(int value) {
            bool b = contains(value);
            if (!(b)) {
                exclusions.remove(value);
            }
            return b;
        }
        public override Saxon.Hej.z.IntIterator iterator() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot enumerate an infinite set");
        }
        public override Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
            return new Saxon.Hej.z.IntComplementSet(exclusions.except(other));
        }
        public override Saxon.Hej.z.IntSet intersect(Saxon.Hej.z.IntSet other) {
            if (other.isEmpty()) {
                return Saxon.Hej.z.IntEmptySet.getInstance();
            } else {
                if ((other == Saxon.Hej.z.IntUniversalSet.getInstance())) {
                    return copy();
                } else {
                    if ((other is Saxon.Hej.z.IntComplementSet)) {
                        return new Saxon.Hej.z.IntComplementSet(exclusions.union(((Saxon.Hej.z.IntComplementSet)(other)).exclusions));
                    } else {
                        return other.intersect(this);
                    }
                }
            }
        }
        public override Saxon.Hej.z.IntSet except(Saxon.Hej.z.IntSet other) {
            return new Saxon.Hej.z.IntComplementSet(exclusions.union(other));
        }
        public override bool containsAll(Saxon.Hej.z.IntSet other) {
            if ((other is Saxon.Hej.z.IntComplementSet)) {
                return ((Saxon.Hej.z.IntComplementSet)(other)).exclusions.containsAll(exclusions);
            } else {
                if ((other is Saxon.Hej.z.IntUniversalSet)) {
                    return (!(exclusions.isEmpty()));
                } else {
                    Saxon.Hej.z.IntIterator ii = other.iterator();
                    while (ii.hasNext()) {
                        if (exclusions.contains(ii.next())) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
    
    }
}