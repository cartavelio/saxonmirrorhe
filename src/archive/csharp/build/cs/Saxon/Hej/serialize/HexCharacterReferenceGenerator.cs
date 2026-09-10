using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class HexCharacterReferenceGenerator : Saxon.Hej.serialize.CharacterReferenceGenerator {
        public readonly static Saxon.Hej.serialize.HexCharacterReferenceGenerator THE_INSTANCE = new Saxon.Hej.serialize.HexCharacterReferenceGenerator();
        private HexCharacterReferenceGenerator()  {
        }
        public void outputCharacterReference(int charval, Saxon.Hej.str.UnicodeWriter writer) {
            writer.writeCodePoint('&');
            writer.writeCodePoint('#');
            writer.writeCodePoint('x');
            writer.write(Saxon.Impl.Helpers.Utils.intToHexString(charval));
            writer.writeCodePoint(';');
        }
    
    }
}