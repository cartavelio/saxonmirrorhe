using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public class IntValuePredicate : Saxon.Hej.z.IntPredicateProxy {
        private readonly int target;
        public IntValuePredicate(int target)  {
            this.target = target;
        }
        public bool test(int value) {
            return (value == target);
        }
        public int getTarget() {
            return target;
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}