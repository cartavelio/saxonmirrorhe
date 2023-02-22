using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public class PredicateCharacterClass : Saxon.Hej.regex.charclass.CharacterClass {
        private readonly Saxon.Ejava.util.function.IntPredicate predicate;
        public PredicateCharacterClass(Saxon.Ejava.util.function.IntPredicate predicate)  {
            this.predicate = predicate;
        }
        public bool test(int value) {
            return predicate(value);
        }
        public bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other) {
            return ((other is Saxon.Hej.regex.charclass.InverseCharacterClass) && other.isDisjoint(this));
        }
        public Saxon.Hej.z.IntSet getIntSet() {
            return null;
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}