using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public class InverseCharacterClass : Saxon.Hej.regex.charclass.CharacterClass {
        private readonly Saxon.Hej.regex.charclass.CharacterClass complement;
        public InverseCharacterClass(Saxon.Hej.regex.charclass.CharacterClass complement)  {
            this.complement = complement;
        }
        public Saxon.Hej.regex.charclass.CharacterClass getComplement() {
            return complement;
        }
        public bool test(int value) {
            return !(complement.test(value));
        }
        public bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other) {
            return (other == complement);
        }
        public Saxon.Hej.z.IntSet getIntSet() {
            Saxon.Hej.z.IntSet comp = complement.getIntSet();
            return ((comp == null) ? null : new Saxon.Hej.z.IntComplementSet(complement.getIntSet()));
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}