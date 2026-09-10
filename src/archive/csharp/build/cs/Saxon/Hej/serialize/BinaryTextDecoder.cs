using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class BinaryTextDecoder : Saxon.Hej.@event.ProxyReceiver {
        internal string outputEncoding = "utf8";
        public BinaryTextDecoder(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties details) : base (next) {
            setOutputProperties(details);
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            outputEncoding = details.getProperty("encoding", "utf8");
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString value, Saxon.Hej.s9api.Location locationId, int properties) {
            string encoding;
            byte[] bytes = null;
            int dot = name.IndexOf('.');
            if (((dot>=0) && (dot != (name.Length-1)))) {
                encoding = name.Substring((dot+1));
                name = name.Substring(0, dot - 0);
            } else {
                encoding = outputEncoding;
            }
            Saxon.Hej.str.UnicodeString tValue = value.tidy();
            if (name.Equals("hex")) {
                bytes = new Saxon.Hej.value.HexBinaryValue(tValue).getBinaryValue();
            } else {
                if (name.Equals("b64")) {
                    bytes = new Saxon.Hej.value.Base64BinaryValue(tValue).getBinaryValue();
                }
            }
            if ((bytes != null)) {
                try {
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
                    System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.GetEncoding(encoding));
                    char[] array = new char[bytes.Length];
                    int used = reader.Read(array, 0, array.Length);
                    nextReceiver.characters(Saxon.Hej.str.StringView.of(new string(array, 0, used)), locationId, properties);
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(("Text output method: failed to decode binary data "+Saxon.Hej.trans.Err.wrap(value.ToString(), Saxon.Hej.trans.Err.VALUE)));
                }
            }
        }
    
    }
}