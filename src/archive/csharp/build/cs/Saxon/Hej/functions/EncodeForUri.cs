using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class EncodeForUri : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString s = arg.getUnicodeStringValue();
            return escape(s, "-_.~");
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public static Saxon.Hej.value.StringValue escape(Saxon.Hej.str.UnicodeString s, string allowedPunctuation) {
            s = s.tidy();
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            Saxon.Hej.z.IntIterator iter = s.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if ((((((c>='a') && (c<='z'))) || (((c>='A') && (c<='Z')))) || (((c>='0') && (c<='9'))))) {
                    sb.append(c);
                } else {
                    if (((c<=0x20) || (c>=0x7f))) {
                        escapeChar(c, sb);
                    } else {
                        if ((allowedPunctuation.IndexOf((char)(c))>=0)) {
                            sb.append(c);
                        } else {
                            escapeChar(c, sb);
                        }
                    }
                }
            }
            return new Saxon.Hej.value.StringValue(sb.toUnicodeString());
        }
        private const string hex = "0123456789ABCDEF";
        public static void escapeChar(int cp, Saxon.Hej.str.UnicodeBuilder sb) {
            byte[] array = Saxon.Hej.serialize.charcode.UTF8CharacterSet.encode(new Saxon.Hej.z.IntSingletonIterator(cp));
            foreach (byte value in array) {
                int v = ((int)(value)&unchecked((int)0xff));
                sb.append('%').append(hex[(v/16)]).append(hex[(v%16)]);
            }
        }
        public static void checkPercentEncoding(string uri) {
            string hexDigits = "0123456789abcdefABCDEF";
            for (int i = 0; (i<uri.Length); ) {
                char c = uri[i];
                byte[] bytes;
                int expectedOctets;
                if ((c == '%')) {
                    if (((i+2)>=uri.Length)) {
                        throw new Saxon.Hej.trans.XPathException(("% sign in URI must be followed by two hex digits"+Saxon.Hej.trans.Err.wrap(uri)));
                    }
                    int h1 = hexDigits.IndexOf(uri[(i+1)]);
                    if ((h1>15)) {
                        h1 -= 6;
                    }
                    int h2 = hexDigits.IndexOf(uri[(i+2)]);
                    if ((h2>15)) {
                        h2 -= 6;
                    }
                    if (((h1>=0) && (h2>=0))) {
                        int b = ((h1<<4)|h2);
                        expectedOctets = UTF8RepresentationLength[h1];
                        if ((expectedOctets == -1)) {
                            throw new Saxon.Hej.trans.XPathException((("First %-encoded octet in URI is not valid as the start of a UTF-8 "+"character: first two bits must not be '10'")+Saxon.Hej.trans.Err.wrap(uri)));
                        }
                        bytes = new byte[expectedOctets];
                        bytes[0] = (byte)(b);
                        i += 3;
                        for (int q = 1; (q<expectedOctets); q++) {
                            if ((((i+2)>uri.Length) || (uri[i] != '%'))) {
                                throw new Saxon.Hej.trans.XPathException(("Incomplete %-encoded UTF-8 octet sequence in URI "+Saxon.Hej.trans.Err.wrap(uri)));
                            }
                            h1 = hexDigits.IndexOf(uri[(i+1)]);
                            if ((h1>15)) {
                                h1 -= 6;
                            }
                            h2 = hexDigits.IndexOf(uri[(i+2)]);
                            if ((h2>15)) {
                                h2 -= 6;
                            }
                            if (((h1<0) || (h2<0))) {
                                throw new Saxon.Hej.trans.XPathException(("Invalid %-encoded UTF-8 octet sequence in URI"+Saxon.Hej.trans.Err.wrap(uri)));
                            }
                            if ((UTF8RepresentationLength[h1] != -1)) {
                                throw new Saxon.Hej.trans.XPathException((("In a URI, a %-encoded UTF-8 octet after the first "+"must have '10' as the first two bits")+Saxon.Hej.trans.Err.wrap(uri)));
                            }
                            b = ((h1<<4)|h2);
                            bytes[q] = (byte)(b);
                            i += 3;
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException(("% sign in URI must be followed by two hex digits"+Saxon.Hej.trans.Err.wrap(uri)));
                    }
                } else {
                    i++;
                }
            }
        }
        private static readonly int[] UTF8RepresentationLength = {1, 1, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, 2, 2, 3, 4};
    
    }
}