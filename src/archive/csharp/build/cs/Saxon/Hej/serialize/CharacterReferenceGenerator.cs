using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public interface CharacterReferenceGenerator {
        void outputCharacterReference(int charval, Saxon.Hej.str.UnicodeWriter writer);
    
    }
}