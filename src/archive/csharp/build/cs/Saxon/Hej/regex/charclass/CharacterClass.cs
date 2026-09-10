using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public interface CharacterClass : Saxon.Hej.z.IntPredicateProxy {
        /* implicit =====
        bool test(int value);
        ===== implicit */
        bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other);
        Saxon.Hej.z.IntSet getIntSet();
    
    }
}