using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class AlphanumericCollator : Saxon.Hej.lib.StringCollator {
        private readonly Saxon.Hej.lib.StringCollator baseCollator;
        private static readonly Saxon.Hej.regex.ARegularExpression pattern = Saxon.Hej.regex.ARegularExpression.compile("\\d+", "");
        public const string PREFIX = "http://saxon.sf.net/collation/alphaNumeric?base=";
        public AlphanumericCollator(Saxon.Hej.lib.StringCollator @base)  {
            baseCollator = @base;
        }
        public string getCollationURI() {
            return (PREFIX+baseCollator.getCollationURI());
        }
        public int compareStrings(Saxon.Hej.str.UnicodeString cs1, Saxon.Hej.str.UnicodeString cs2) {
            Saxon.Hej.regex.RegexIterator iter1 = pattern.analyze(cs1);
            Saxon.Hej.regex.RegexIterator iter2 = pattern.analyze(cs2);
            while (true) {
                Saxon.Hej.value.StringValue sv1 = ((/*1430*/Saxon.Hej.value.StringValue)iter1.next());
                Saxon.Hej.value.StringValue sv2 = ((/*1430*/Saxon.Hej.value.StringValue)iter2.next());
                if ((sv1 == null)) {
                    return ((sv2 == null) ? 0 : -1);
                }
                if ((sv2 == null)) {
                    return +(1);
                }
                bool numeric1 = iter1.isMatching();
                bool numeric2 = iter2.isMatching();
                if ((numeric1 && numeric2)) {
                    System.Numerics.BigInteger n1 = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(sv1.getStringValue());
                    System.Numerics.BigInteger n2 = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(sv2.getStringValue());
                    int c = n1.CompareTo(n2);
                    if ((c != 0)) {
                        return c;
                    }
                } else {
                    Saxon.Hej.str.UnicodeString u1 = (numeric1 ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : sv1.getUnicodeStringValue());
                    Saxon.Hej.str.UnicodeString u2 = (numeric2 ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : sv2.getUnicodeStringValue());
                    int c = baseCollator.compareStrings(u1, u2);
                    if ((c != 0)) {
                        return c;
                    }
                }
            }
        }
        public bool comparesEqual(Saxon.Hej.str.UnicodeString s1, Saxon.Hej.str.UnicodeString s2) {
            return (compareStrings(s1, s2) == 0);
        }
        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.str.UnicodeString cs) {
            System.IO.MemoryStream baos = new System.IO.MemoryStream();
            Saxon.Hej.regex.RegexIterator iter = pattern.analyze(cs);
            for (Saxon.Hej.value.StringValue sv; ((sv = ((/*1430*/Saxon.Hej.value.StringValue)iter.next())) != null); ) {
                if (iter.isMatching()) {
                    System.Numerics.BigInteger n = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(sv.getStringValue());
                    byte[] bin = n.ToByteArray(isUnsigned: false, isBigEndian: true);
                    int len = bin.Length;
                    writeByte(baos, (byte)(0));
                    writeByte(baos, (byte)(len));
                    baos.Write(bin, 0, bin.Length);
                } else {
                    Saxon.Hej.value.Base64BinaryValue b64 = Saxon.Hej.functions.CollationKeyFn.getCollationKey(sv.getUnicodeStringValue(), baseCollator);
                    byte[] bin = b64.getBinaryValue();
                    baos.Write(bin, 0, bin.Length);
                }
            }
            return new Saxon.Hej.value.Base64BinaryValue(baos.ToArray());
        }
        private static void writeByte(System.IO.MemoryStream baos, byte val) {
            baos.WriteByte(val);
        }
    
        /* Expanding default method isEqualToEmpty from interface StringCollator */
        public virtual bool isEqualToEmpty(Saxon.Hej.str.UnicodeString s1) {
            return comparesEqual(s1, Saxon.Hej.str.EmptyUnicodeString.getInstance());
        }

    }
}