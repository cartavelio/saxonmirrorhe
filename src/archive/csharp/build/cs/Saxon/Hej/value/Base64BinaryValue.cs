using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class Base64BinaryValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.value.ContextFreeAtomicValue {

        static Base64BinaryValue() {
            Saxon.Ejava.util.Arrays.fill(decoding, -1);
            for (int i = 0; (i<alphabet.Length); i++) {
                char c = alphabet[i];
                encoding[i] = c;
                decoding[c] = i;
            }
        }
        private readonly byte[] binaryValue;
        public Base64BinaryValue(Saxon.Hej.str.UnicodeString s)  {
            binaryValue = decode(s);
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY;
        }
        public Base64BinaryValue(byte[] value)  {
            binaryValue = value;
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.Base64BinaryValue v = new Saxon.Hej.value.Base64BinaryValue(binaryValue);
            v.typeLabel = typeLabel;
            return v;
        }
        public byte[] getBinaryValue() {
            return binaryValue;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return encode(binaryValue);
        }
        public int getLengthInOctets() {
            return binaryValue.Length;
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public Saxon.Hej.expr.sort.XPathComparable getXPathComparable() {
            return this;
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.Base64BinaryValue) && Saxon.Ejava.util.Arrays.Equals(binaryValue, ((Saxon.Hej.value.Base64BinaryValue)(other)).binaryValue));
        }
        public override int GetHashCode() {
            return byteArrayHashCode(binaryValue);
        }
        protected internal static int byteArrayHashCode(byte[] value) {
            long h = 0;
            for (int i = 0; (i<System.Math.Min(value.Length, 64)); i++) {
                h = (((h<<1))^value[i]);
            }
            return (int)(((((h>>32))^h)));
        }
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private static readonly int[] encoding = new int[64];
        private static readonly int[] decoding = new int[128];
        public static Saxon.Hej.str.UnicodeString encode(byte[] value) {
            Saxon.Hej.str.UnicodeBuilder buff = new Saxon.Hej.str.UnicodeBuilder();
            int whole = (value.Length-(value.Length%3));
            for (int i = 0; (i<whole); i += 3) {
                int val = ((((((((int)(value[i]))&unchecked((int)0xff)))<<16))+((((((int)(value[(i+1)]))&unchecked((int)0xff)))<<8)))+(((((int)(value[(i+2)]))&unchecked((int)0xff)))));
                buff.append((char)(encoding[(((val>>18))&0x3f)]));
                buff.append((char)(encoding[(((val>>12))&0x3f)]));
                buff.append((char)(encoding[(((val>>6))&0x3f)]));
                buff.append((char)(encoding[(val&0x3f)]));
            }
            int remainder = ((value.Length%3));
            switch (remainder) {
              case 0:
              default:
                break ;
              case 1:
                {
                    int val = ((((((int)(value[whole]))&unchecked((int)0xff)))<<4));
                    buff.append((char)(encoding[(((val>>6))&0x3f)]));
                    buff.append((char)(encoding[(val&0x3f)]));
                    buff.appendLatin("==");
                    break ;
                }
              case 2:
                {
                    int val = (((((((int)(value[whole]))&unchecked((int)0xff)))<<10))+((((((int)(value[(whole+1)]))&unchecked((int)0xff)))<<2)));
                    buff.append((char)(encoding[(((val>>12))&0x3f)]));
                    buff.append((char)(encoding[(((val>>6))&0x3f)]));
                    buff.append((char)(encoding[(val&0x3f)]));
                    buff.append("=");
                    break ;
                }
            }
            return buff.toUnicodeString();
        }
        public static byte[] decode(Saxon.Hej.str.UnicodeString @in) {
            @in = @in.tidy();
            int[] unit = new int[4];
            byte[] result = new byte[@in.length32()];
            int bytesUsed = 0;
            int i = 0;
            int u = 0;
            int pad = 0;
            int chars = 0;
            char last = (char)(0);
            while ((i<@in.length())) {
                int c = @in.codePointAt(i++);
                if (!(Saxon.Hej.value.Whitespace.isWhite(c))) {
                    chars++;
                    if ((c == '=')) {
                        pad = 1;
                        for (int k = i; (k<@in.length()); k++) {
                            int ch = @in.codePointAt(k);
                            if ((ch == '=')) {
                                pad++;
                                chars++;
                            } else {
                                if (Saxon.Hej.value.Whitespace.isWhite(ch)) {
                                } else {
                                    throw new Saxon.Hej.trans.XPathException("Base64 padding character '=' is followed by non-padding characters", "FORG0001");
                                }
                            }
                        }
                        if (((pad == 1) && ("AEIMQUYcgkosw048".IndexOf(last)<0))) {
                            throw new Saxon.Hej.trans.XPathException(("In base64, if the value ends with a single '=' character, then the preceding character must be"+" one of [AEIMQUYcgkosw048]"), "FORG0001");
                        } else {
                            if (((pad == 2) && ("AQgw".IndexOf(last)<0))) {
                                throw new Saxon.Hej.trans.XPathException(("In base64, if the value ends with '==', then the preceding character must be"+" one of [AQgw]"), "FORG0001");
                            }
                        }
                        if ((pad>2)) {
                            throw new Saxon.Hej.trans.XPathException((("Found "+pad)+" '=' characters at end of base64 value; max is 2"), "FORG0001");
                        }
                        if ((pad != ((((4-u))%4)))) {
                            throw new Saxon.Hej.trans.XPathException(((("Required "+((((4-u))%4)))+" '=' characters at end of base64 value; found ")+pad), "FORG0001");
                        }
                        for (int p = 0; (p<pad); p++) {
                            unit[u++] = 'A';
                        }
                        i = @in.length32();
                    } else {
                        last = (char)(c);
                        unit[u++] = c;
                    }
                    if ((u == 4)) {
                        int t = (((((decodeChar(unit[0])<<18))+((decodeChar(unit[1])<<12)))+((decodeChar(unit[2])<<6)))+(decodeChar(unit[3])));
                        if (((bytesUsed+3)>result.Length)) {
                            byte[] r2 = new byte[(bytesUsed*2)];
                            Saxon.Ejava.lang.System.arraycopy(result, 0, r2, 0, bytesUsed);
                            result = r2;
                        }
                        result[bytesUsed++] = (byte)(((((t>>16))&unchecked((int)0xff))));
                        result[bytesUsed++] = (byte)(((((t>>8))&unchecked((int)0xff))));
                        result[bytesUsed++] = (byte)(((t&unchecked((int)0xff))));
                        u = 0;
                    }
                }
                if ((i>=@in.length())) {
                    bytesUsed -= pad;
                    break ;
                }
            }
            if (((chars%4) != 0)) {
                throw new Saxon.Hej.trans.XPathException("Length of base64 value must be a multiple of four", "FORG0001");
            }
            byte[] r3 = new byte[bytesUsed];
            Saxon.Ejava.lang.System.arraycopy(result, 0, r3, 0, bytesUsed);
            return r3;
        }
        private static int decodeChar(int c) {
            int d = ((c<128) ? decoding[c] : -1);
            if ((d == -1)) {
                throw new Saxon.Hej.trans.XPathException((("Invalid character '"+c)+"' in base64 value"), "FORG0001");
            }
            return d;
        }
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable o) {
            if ((o is Saxon.Hej.value.Base64BinaryValue)) {
                byte[] other = ((Saxon.Hej.value.Base64BinaryValue)(o)).binaryValue;
                int len0 = binaryValue.Length;
                int len1 = other.Length;
                int shorter = System.Math.Min(len0, len1);
                for (int i = 0; (i<shorter); i++) {
                    int a = ((int)(binaryValue[i])&unchecked((int)0xff));
                    int b = ((int)(other[i])&unchecked((int)0xff));
                    if ((a != b)) {
                        return ((a<b) ? -1 : +(1));
                    }
                }
                return System.Math.Sign((len0-len1));
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:base64Binary to "+o.ToString()));
            }
        }
    
    }
}