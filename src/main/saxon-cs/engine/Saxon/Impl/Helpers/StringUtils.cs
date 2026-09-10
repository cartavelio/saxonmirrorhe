using System;
using System.Text;
using J2N;
using net.sf.saxon.trans;

namespace Saxon.Impl.Helpers {
    public class StringUtils {

        public static readonly System.Text.Encoding LATIN_1 = System.Text.Encoding.GetEncoding("iso-8859-1");

        public const char BOM = (char) 0xfeff; 
        
        public static void stringToCharArray(string str, int begin, int end, char[] buff, int dest) {
            str.CopyTo(begin, buff, dest, end - begin);
        }

        public static bool stringEqualsIgnoreCase(string a, string b) {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static string nullIfEmpty(string a) {
            return string.IsNullOrEmpty(a) ? null : a;
        }

        public static byte[] encode(string s, string encodingName) {
            Encoding encoding;
            if (stringEqualsIgnoreCase(encodingName, "utf-16")) {
                // .NET interprets utf-16 as little-endian by default. And we have to add the BOM by hand
                s = BOM + s;
                encoding  = System.Text.Encoding.GetEncoding("UTF-16BE", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                ;
            }
            else {
                encoding = System.Text.Encoding.GetEncoding(encodingName, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            }
            try {
                return encoding.GetBytes(s);
            }
            catch (EncoderFallbackException e) {
                throw new XPathException("Invalid characters for encoding " + encodingName, e);
            }
        }

        public static string decode(byte[] value, int offset, int len, string encodingName) {
            if (stringEqualsIgnoreCase(encodingName, "utf-16")) {
                if (value.Length >= 2) {
                    if (value[0] == 0xfe && value[1] == 0xff) {
                        encodingName = "UTF-16BE";
                    } else if (value[0] == 0xff && value[1] == 0xfe) {
                        encodingName = "UTF-16LE";
                    }
                }
            }
            Encoding encoding = System.Text.Encoding.GetEncoding(encodingName, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            try {
                string decoded = new string(encoding.GetChars(value, offset, len));
                if (decoded.StartsWith(BOM)) {
                    decoded = decoded[1..];
                }

                return decoded;
            }
            catch (DecoderFallbackException e) {
                throw new XPathException("Invalid bytes for decoding " + encodingName, e);
            }
        }



    }
}