using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize.charcode {
    public class ASCIICharacterSet : Saxon.Hej.serialize.charcode.CharacterSet {
        public static readonly Saxon.Hej.serialize.charcode.ASCIICharacterSet theInstance = new Saxon.Hej.serialize.charcode.ASCIICharacterSet();
        private ASCIICharacterSet()  {
        }
        public static Saxon.Hej.serialize.charcode.ASCIICharacterSet getInstance() {
            return theInstance;
        }
        public bool inCharset(int c) {
            return (c<=0x7f);
        }
        public string getCanonicalName() {
            return "US-ASCII";
        }
    
    }
}