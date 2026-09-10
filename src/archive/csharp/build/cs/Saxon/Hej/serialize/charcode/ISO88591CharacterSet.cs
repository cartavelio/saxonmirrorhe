using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize.charcode {
    public class ISO88591CharacterSet : Saxon.Hej.serialize.charcode.CharacterSet {
        private static readonly Saxon.Hej.serialize.charcode.ISO88591CharacterSet theInstance = new Saxon.Hej.serialize.charcode.ISO88591CharacterSet();
        private ISO88591CharacterSet()  {
        }
        public static Saxon.Hej.serialize.charcode.ISO88591CharacterSet getInstance() {
            return theInstance;
        }
        public bool inCharset(int c) {
            return (c<=unchecked((int)0xff));
        }
        public string getCanonicalName() {
            return "ISO-8859-1";
        }
    
    }
}