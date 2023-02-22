using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class DecimalCharacterReferenceGenerator : Saxon.Hej.serialize.CharacterReferenceGenerator {
        public readonly static Saxon.Eej.serialize.DecimalCharacterReferenceGenerator THE_INSTANCE = new Saxon.Eej.serialize.DecimalCharacterReferenceGenerator();
        private DecimalCharacterReferenceGenerator()  {
        }
        public void outputCharacterReference(int charval, Saxon.Hej.str.UnicodeWriter writer) {
            writer.writeCodePoint('&');
            writer.writeCodePoint('#');
            writer.write(charval.ToString());
            writer.writeCodePoint(';');
        }
    
    }
}