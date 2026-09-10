using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntPredicateLambda : Saxon.Hej.z.IntPredicateProxy {
        internal Saxon.Ejava.util.function.IntPredicate lambda;
        public IntPredicateLambda(Saxon.Ejava.util.function.IntPredicate lambda)  {
            this.lambda = lambda;
        }
        public static Saxon.Hej.z.IntPredicateProxy of(Saxon.Ejava.util.function.IntPredicate lambda) {
            return new Saxon.Hej.z.IntPredicateLambda(lambda);
        }
        public bool test(int value) {
            return lambda(value);
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}