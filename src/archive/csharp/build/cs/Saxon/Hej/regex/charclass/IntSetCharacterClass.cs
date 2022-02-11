using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public class IntSetCharacterClass : Saxon.Hej.regex.charclass.CharacterClass {
        private readonly Saxon.Hej.z.IntSet intSet;
        public IntSetCharacterClass(Saxon.Hej.z.IntSet intSet)  {
            this.intSet = intSet;
        }
        public Saxon.Hej.z.IntSet getIntSet() {
            return intSet;
        }
        public bool test(int value) {
            return intSet.contains(value);
        }
        public bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other) {
            if ((other is Saxon.Hej.regex.charclass.IntSetCharacterClass)) {
                return intSet.intersect(((Saxon.Hej.regex.charclass.IntSetCharacterClass)(other)).intSet).isEmpty();
            } else {
                if ((other is Saxon.Hej.regex.charclass.InverseCharacterClass)) {
                    return other.isDisjoint(this);
                } else {
                    return false;
                }
            }
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}