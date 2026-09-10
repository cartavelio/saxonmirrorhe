using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class NestedIntegerValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.expr.sort.AtomicMatchKey {
        public static Saxon.Hej.value.NestedIntegerValue ONE = new Saxon.Hej.value.NestedIntegerValue(new int[]{1});
        public static Saxon.Hej.value.NestedIntegerValue TWO = new Saxon.Hej.value.NestedIntegerValue(new int[]{2});
        internal int[] value;
        public NestedIntegerValue(string v)  {
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.STRING;
            parse(v);
        }
        public NestedIntegerValue(int[] val)  {
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.STRING;
            value = val;
        }
        public static Saxon.Hej.value.NestedIntegerValue parse(string v) {
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(v, ".");
            int[] valuei = new int[st.countTokens()];
            try {
                for (int i = 0; st.hasMoreTokens(); i++) {
                    valuei[i] = Saxon.Impl.Helpers.Utils.ParseInt(st.nextToken());
                }
            } catch (Saxon.Ejava.lang.NumberFormatException exc) {
                throw new Saxon.Hej.trans.XPathException(("Nested integer value has incorrect format: "+v));
            }
            return new Saxon.Hej.value.NestedIntegerValue(valuei);
        }
        public Saxon.Hej.value.NestedIntegerValue append(int leaf) {
            int[] v = new int[(value.Length+1)];
            Saxon.Ejava.lang.System.arraycopy(value, 0, v, 0, value.Length);
            v[value.Length] = leaf;
            return new Saxon.Hej.value.NestedIntegerValue(v);
        }
        public Saxon.Hej.value.NestedIntegerValue getStem() {
            if ((value.Length == 0)) {
                return null;
            } else {
                int[] v = new int[(value.Length-1)];
                Saxon.Ejava.lang.System.arraycopy(value, 0, v, 0, v.Length);
                return new Saxon.Hej.value.NestedIntegerValue(v);
            }
        }
        public int getDepth() {
            return value.Length;
        }
        public int getLeaf() {
            if ((value.Length == 0)) {
                return -1;
            } else {
                return value[(value.Length-1)];
            }
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override bool Equals(System.Object o) {
            return (((o is Saxon.Hej.value.NestedIntegerValue)) && Saxon.Ejava.util.Arrays.Equals(value, ((Saxon.Hej.value.NestedIntegerValue)(o)).value));
        }
        public override int GetHashCode() {
            return Saxon.Ejava.util.Arrays.hashCode(value);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.NestedIntegerValue v = new Saxon.Hej.value.NestedIntegerValue(value);
            v.typeLabel = typeLabel;
            return v;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
            for (int i = 0; (i<(value.Length-1)); i++) {
                buffer.append((value[i]+"."));
            }
            buffer.append((value[(value.Length-1)]+""));
            return buffer.toUnicodeString();
        }
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.NestedIntegerValue)) {
                Saxon.Hej.value.NestedIntegerValue v2 = (Saxon.Hej.value.NestedIntegerValue)(other);
                for (int i = 0; ((i<value.Length) && (i<v2.value.Length)); i++) {
                    if ((value[i] != v2.value[i])) {
                        if ((value[i]<v2.value[i])) {
                            return -1;
                        } else {
                            return 1;
                        }
                    }
                }
                return System.Math.Sign((value.Length-v2.value.Length));
            } else {
                throw new System.InvalidCastException(("Cannot compare nested integer value to "+other));
            }
        }
    
    }
}