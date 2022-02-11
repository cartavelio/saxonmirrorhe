using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntExceptPredicate : Saxon.Hej.z.IntPredicateProxy {
        private readonly Saxon.Hej.z.IntPredicateProxy p1;
        private readonly Saxon.Hej.z.IntPredicateProxy p2;
        private IntExceptPredicate(Saxon.Hej.z.IntPredicateProxy p1, Saxon.Hej.z.IntPredicateProxy p2)  {
            this.p1 = p1;
            this.p2 = p2;
        }
        public static Saxon.Hej.z.IntPredicateProxy makeDifference(Saxon.Hej.z.IntPredicateProxy p1, Saxon.Hej.z.IntPredicateProxy p2) {
            return new Saxon.Hej.z.IntExceptPredicate(p1, p2);
        }
        public bool test(int value) {
            return (p1.test(value) && !(p2.test(value)));
        }
        public Saxon.Hej.z.IntPredicateProxy[] getOperands() {
            return new Saxon.Hej.z.IntPredicateProxy[]{p1, p2};
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}