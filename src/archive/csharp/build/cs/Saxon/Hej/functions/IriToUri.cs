using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class IriToUri : Saxon.Hej.functions.ScalarSystemFunction {

        static IriToUri() {
            Saxon.Ejava.util.Arrays.fill(allowedASCII, 0, 32, false);
            Saxon.Ejava.util.Arrays.fill(allowedASCII, 33, 127, true);
            allowedASCII[(int)('"')] = false;
            allowedASCII[(int)('<')] = false;
            allowedASCII[(int)('>')] = false;
            allowedASCII[(int)('\\')] = false;
            allowedASCII[(int)('^')] = false;
            allowedASCII[(int)('`')] = false;
            allowedASCII[(int)('{')] = false;
            allowedASCII[(int)('|')] = false;
            allowedASCII[(int)('}')] = false;
        }
        public static bool[] allowedASCII = new bool[128];
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.value.StringValue(iriToUri(arg.getUnicodeStringValue()));
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public static Saxon.Hej.str.UnicodeString iriToUri(Saxon.Hej.str.UnicodeString s) {
            if (allAllowedAscii(s.codePoints())) {
                return s;
            }
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder((s.length32()+20));
            Saxon.Hej.z.IntIterator iter = s.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if (((c>=0x7f) || !(allowedASCII[(int)(c)]))) {
                    Saxon.Hej.functions.EncodeForUri.escapeChar(c, sb);
                } else {
                    sb.append(c);
                }
            }
            return sb.toUnicodeString();
        }
        private static bool allAllowedAscii(Saxon.Hej.z.IntIterator codePoints) {
            while (codePoints.hasNext()) {
                int c = codePoints.next();
                if (((c>=0x7f) || !(allowedASCII[(int)(c)]))) {
                    return false;
                }
            }
            return true;
        }
    
    }
}