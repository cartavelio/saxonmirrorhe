using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class DecodeFromURI : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            if ((arguments[0].head() == null)) {
                return new Saxon.Hej.value.StringValue("");
            }
            string value = arguments[0].head().getStringValue();
            if (value.Length==0) {
                return new Saxon.Hej.value.StringValue("");
            }
            return new Saxon.Hej.value.StringValue(Saxon.Eej.functions.qt4.DecodeFromURI.decode(value));
        }
        private static bool isHex(char ch) {
            return ((((ch>='0') && (ch<='9'))) || (((ch>='A') && (ch<='F'))));
        }
        public static string decode(string value) {
            System.Collections.Generic.IList<System.Byte> output = new System.Collections.Generic.List<System.Byte>();
            byte[] octets = Saxon.Impl.Helpers.StringUtils.EncodingForName(System.Text.Encoding.UTF8).GetBytes(value);
            int idx = 0;
            while ((idx<octets.Length)) {
                switch ((char)(octets[idx])) {
                  case '+':
                    output.Add((byte)(0x20));
                    idx++;
                    break ;
                  case '%':
                    if (((idx+2)>=octets.Length)) {
                        output.Add((byte)(0xEF));
                        output.Add((byte)(0xBF));
                        output.Add((byte)(0xBD));
                        idx = octets.Length;
                        break ;
                    }
                    char tens = System.Char.ToUpperInvariant((char)(((octets[(idx+1)]&0xFF))));
                    char ones = System.Char.ToUpperInvariant((char)(((octets[(idx+2)]&0xFF))));
                    idx += 3;
                    if ((Saxon.Eej.functions.qt4.DecodeFromURI.isHex(tens) && Saxon.Eej.functions.qt4.DecodeFromURI.isHex(ones))) {
                        int hex = Saxon.Impl.Helpers.Utils.ParseInt(((""+tens)+ones), 16);
                        output.Add((byte)(hex));
                    } else {
                        output.Add((byte)(0xEF));
                        output.Add((byte)(0xBF));
                        output.Add((byte)(0xBD));
                    }
                    break ;
                  default:
                    output.Add(octets[idx]);
                    idx++;
                    break ;
                }
            }
            byte[] utf8 = new byte[output.Count];
            for (idx = 0; (idx<output.Count); idx++) {
                utf8[idx] = output[idx];
            }
            Saxon.Hej.str.UnicodeString result = Saxon.Hej.str.StringView.of(System.Text.Encoding.UTF8.GetString(utf8));
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            Saxon.Hej.z.IntIterator iter = result.codePoints();
            while (iter.hasNext()) {
                int cp = iter.next();
                if (Saxon.Hej.serialize.charcode.XMLCharacterData.isValid10(cp)) {
                    sb.appendCodePoint(cp);
                } else {
                    sb.appendCodePoint(0xFFFD);
                }
            }
            return sb.ToString();
        }
    
    }
}