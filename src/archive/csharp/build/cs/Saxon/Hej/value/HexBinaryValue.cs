using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class HexBinaryValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.value.ContextFreeAtomicValue {
        private readonly byte[] binaryValue;
        public HexBinaryValue(Saxon.Hej.str.UnicodeString @in)  {
            Saxon.Hej.str.UnicodeString s = Saxon.Hej.value.Whitespace.trim(@in);
            int len32 = s.length32();
            if ((((len32&1)) != 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("A hexBinary value must contain an even number of characters");
                err.setErrorCode("FORG0001");
                throw err;
            }
            binaryValue = new byte[(len32/2)];
            for (int i = 0; (i<binaryValue.Length); i++) {
                binaryValue[i] = (byte)(((((fromHex(s.codePointAt((2*i)))<<4))+fromHex(s.codePointAt(((2*i)+1))))));
            }
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY;
        }
        public HexBinaryValue(byte[] value)  {
            binaryValue = value;
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.HexBinaryValue v = new Saxon.Hej.value.HexBinaryValue(binaryValue);
            v.typeLabel = typeLabel;
            return v;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY;
        }
        public byte[] getBinaryValue() {
            return binaryValue;
        }
        private int fromHex(int c) {
            int d = ((c<255) ? "0123456789ABCDEFabcdef".IndexOf((char)(c)) : -1);
            if ((d>15)) {
                d = (d-6);
            }
            if ((d<0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Invalid hexadecimal digit '"+c)+"'"));
                err.setErrorCode("FORG0001");
                throw err;
            }
            return d;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            string digits = "0123456789ABCDEF";
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            foreach (byte aBinaryValue in binaryValue) {
                sb.append(digits[(((aBinaryValue>>4))&unchecked((int)0xf))]);
                sb.append(digits[(aBinaryValue&unchecked((int)0xf))]);
            }
            return sb.toUnicodeString();
        }
        public int getLengthInOctets() {
            return binaryValue.Length;
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public Saxon.Hej.expr.sort.XPathComparable getXPathComparable() {
            return this;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.HexBinaryValue) && Saxon.Ejava.util.Arrays.Equals(binaryValue, ((Saxon.Hej.value.HexBinaryValue)(other)).binaryValue));
        }
        public override int GetHashCode() {
            return Saxon.Hej.value.Base64BinaryValue.byteArrayHashCode(binaryValue);
        }
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable o) {
            if ((o is Saxon.Hej.value.HexBinaryValue)) {
                byte[] other = ((Saxon.Hej.value.HexBinaryValue)(o)).binaryValue;
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
                throw new System.InvalidCastException(("Cannot compare xs:hexBinary to "+o.ToString()));
            }
        }
    
    }
}