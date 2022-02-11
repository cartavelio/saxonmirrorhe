using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public class EmptyCharacterClass : Saxon.Hej.regex.charclass.CharacterClass {
        private readonly static Saxon.Hej.regex.charclass.EmptyCharacterClass THE_INSTANCE = new Saxon.Hej.regex.charclass.EmptyCharacterClass();
        private readonly static Saxon.Hej.regex.charclass.InverseCharacterClass COMPLEMENT = new Saxon.Hej.regex.charclass.InverseCharacterClass(THE_INSTANCE);
        public static Saxon.Hej.regex.charclass.EmptyCharacterClass getInstance() {
            return THE_INSTANCE;
        }
        public static Saxon.Hej.regex.charclass.CharacterClass getComplement() {
            return COMPLEMENT;
        }
        private EmptyCharacterClass()  {
        }
        public bool test(int value) {
            return false;
        }
        public bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other) {
            return true;
        }
        public Saxon.Hej.z.IntSet getIntSet() {
            return Saxon.Hej.z.IntEmptySet.getInstance();
        }
    
        /* Expanding default method union from interface IntPredicateProxy */
        public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
            return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
        }

    }
}