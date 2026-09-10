using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntSetPredicate : Saxon.Hej.z.IntPredicateProxy {
        private readonly Saxon.Hej.z.IntSet set;
        public IntSetPredicate(Saxon.Hej.z.IntSet set)  {
            if ((set == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            this.set = set;
        }
        public bool test(int value) {
            return set.contains(value);
        }
        public Saxon.Hej.z.IntSet getIntSet() {
            return set;
        }
        public override string ToString() {
            return (("in {"+set)+"}");
        }
        public readonly static Saxon.Hej.z.IntPredicateProxy ALWAYS_TRUE = new Saxon.Hej.z.IntPredicateLambda(i => true);
        public readonly static Saxon.Hej.z.IntPredicateProxy ALWAYS_FALSE = new Saxon.Hej.z.IntPredicateLambda(i => false);
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}