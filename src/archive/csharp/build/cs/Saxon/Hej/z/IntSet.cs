using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public abstract class IntSet {
        abstract public Saxon.Hej.z.IntSet copy();
        abstract public Saxon.Hej.z.IntSet mutableCopy();
        public virtual bool isMutable() {
            return true;
        }
        abstract public void clear();
        abstract public int size();
        abstract public bool isEmpty();
        abstract public bool contains(int value);
        abstract public bool remove(int value);
        abstract public bool add(int value);
        abstract public Saxon.Hej.z.IntIterator iterator();
        public virtual bool containsAll(Saxon.Hej.z.IntSet other) {
            if (((other == Saxon.Hej.z.IntUniversalSet.getInstance()) || ((other is Saxon.Hej.z.IntComplementSet)))) {
                return false;
            }
            Saxon.Hej.z.IntIterator it = other.iterator();
            while (it.hasNext()) {
                if (!(contains(it.next()))) {
                    return false;
                }
            }
            return true;
        }
        public virtual Saxon.Hej.z.IntSet union(Saxon.Hej.z.IntSet other) {
            if ((other == Saxon.Hej.z.IntUniversalSet.getInstance())) {
                return other;
            }
            if (this.isEmpty()) {
                return other.copy();
            }
            if (other.isEmpty()) {
                return this.copy();
            }
            if ((other is Saxon.Hej.z.IntComplementSet)) {
                return other.union(this);
            }
            Saxon.Hej.z.IntHashSet n = new Saxon.Hej.z.IntHashSet((this.size()+other.size()));
            Saxon.Hej.z.IntIterator it = iterator();
            while (it.hasNext()) {
                n.add(it.next());
            }
            it = other.iterator();
            while (it.hasNext()) {
                n.add(it.next());
            }
            return n;
        }
        public virtual Saxon.Hej.z.IntSet intersect(Saxon.Hej.z.IntSet other) {
            if ((this.isEmpty() || other.isEmpty())) {
                return Saxon.Hej.z.IntEmptySet.getInstance();
            }
            Saxon.Hej.z.IntHashSet n = new Saxon.Hej.z.IntHashSet(size());
            Saxon.Hej.z.IntIterator it = iterator();
            while (it.hasNext()) {
                int v = it.next();
                if (other.contains(v)) {
                    n.add(v);
                }
            }
            return n;
        }
        public virtual Saxon.Hej.z.IntSet except(Saxon.Hej.z.IntSet other) {
            Saxon.Hej.z.IntHashSet n = new Saxon.Hej.z.IntHashSet(size());
            Saxon.Hej.z.IntIterator it = iterator();
            while (it.hasNext()) {
                int v = it.next();
                if (!(other.contains(v))) {
                    n.add(v);
                }
            }
            return n;
        }
    
    }
}