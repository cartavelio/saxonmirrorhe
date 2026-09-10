using System;
using System.Text;
using Saxon.Hej.serialize.charcode;

namespace Saxon.Helpers {
    public class DotNetCharacterSet : CharacterSet {
        private Encoding encoding;
        private Encoder encoder;

        public DotNetCharacterSet(Encoding encoding) {
            this.encoding = encoding;
            encoder = encoding.GetEncoder();
            encoder.Fallback = new EncoderReplacementFallback("?");
        }
        
        public static Saxon.Hej.serialize.charcode.CharacterSet GetCharacterSet(string encoding) {
            try {
                Encoding enc = Encoding.GetEncoding(encoding);
                return new DotNetCharacterSet(enc);
            }
            catch (ArgumentException e) {
                throw new Saxon.Hej.trans.XPathException(("Unknown encoding requested: "+encoding), "SESU0007");
            }
            
        }
        
        public bool inCharset(int ch) {
            if (ch >= 32 && ch <= 127) {
                return true;
            }
            char[] buff;
            if (ch <= 65535) {
                buff = new char[1] { (char)ch };
            }
            else {
                buff = new char[2] { UTF16CharacterSet.highSurrogate(ch), UTF16CharacterSet.lowSurrogate(ch) };
            }

            byte[] result = new byte[16];
            encoder.GetBytes(buff, 0, buff.Length, result, 0, true);
            return result[0] != 63;
        }

        public string getCanonicalName() {
            return encoding.HeaderName;
        }
    }
}