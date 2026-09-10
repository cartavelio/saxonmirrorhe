using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class HTMLEntityReferenceGenerator : Saxon.Hej.serialize.CharacterReferenceGenerator {
        private static readonly string[] latin1Entities = {"nbsp", "iexcl", "cent", "pound", "curren", "yen", "brvbar", "sect", "uml", "copy", "ordf", "laquo", "not", "shy", "reg", "macr", "deg", "plusmn", "sup2", "sup3", "acute", "micro", "para", "middot", "cedil", "sup1", "ordm", "raquo", "frac14", "frac12", "frac34", "iquest", "Agrave", "Aacute", "Acirc", "Atilde", "Auml", "Aring", "AElig", "Ccedil", "Egrave", "Eacute", "Ecirc", "Euml", "Igrave", "Iacute", "Icirc", "Iuml", "ETH", "Ntilde", "Ograve", "Oacute", "Ocirc", "Otilde", "Ouml", "times", "Oslash", "Ugrave", "Uacute", "Ucirc", "Uuml", "Yacute", "THORN", "szlig", "agrave", "aacute", "acirc", "atilde", "auml", "aring", "aelig", "ccedil", "egrave", "eacute", "ecirc", "euml", "igrave", "iacute", "icirc", "iuml", "eth", "ntilde", "ograve", "oacute", "ocirc", "otilde", "ouml", "divide", "oslash", "ugrave", "uacute", "ucirc", "uuml", "yacute", "thorn", "yuml"};
        public readonly static Saxon.Eej.serialize.HTMLEntityReferenceGenerator HEX = new Saxon.Eej.serialize.HTMLEntityReferenceGenerator(Saxon.Hej.serialize.HexCharacterReferenceGenerator.THE_INSTANCE);
        public readonly static Saxon.Eej.serialize.HTMLEntityReferenceGenerator DECIMAL = new Saxon.Eej.serialize.HTMLEntityReferenceGenerator(Saxon.Eej.serialize.DecimalCharacterReferenceGenerator.THE_INSTANCE);
        private readonly Saxon.Hej.serialize.CharacterReferenceGenerator fallback;
        private HTMLEntityReferenceGenerator(Saxon.Hej.serialize.CharacterReferenceGenerator fallback)  {
            this.fallback = fallback;
        }
        public void outputCharacterReference(int c, Saxon.Hej.str.UnicodeWriter writer) {
            if (((c>160) && (c<=255))) {
                writer.writeCodePoint('&');
                writer.write(latin1Entities[(c-160)]);
                writer.writeCodePoint(';');
            } else {
                fallback.outputCharacterReference(c, writer);
            }
        }
    
    }
}