using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public class SingletonCharacterClass : Saxon.Hej.regex.charclass.CharacterClass {
        private readonly int codepoint;
        public SingletonCharacterClass(int codepoint)  {
            this.codepoint = codepoint;
        }
        public bool test(int value) {
            return (value == codepoint);
        }
        public bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other) {
            return !(other.test(codepoint));
        }
        public int getCodepoint() {
            return codepoint;
        }
        public Saxon.Hej.z.IntSet getIntSet() {
            return new Saxon.Hej.z.IntSingletonSet(codepoint);
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}