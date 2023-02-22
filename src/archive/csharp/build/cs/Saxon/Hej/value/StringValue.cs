using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class StringValue : Saxon.Hej.value.AtomicValue {
        protected internal Saxon.Hej.str.UnicodeString content;
        public static readonly Saxon.Hej.value.StringValue EMPTY_STRING = new Saxon.Hej.value.StringValue(Saxon.Hej.str.EmptyUnicodeString.getInstance());
        public static readonly Saxon.Hej.value.StringValue SINGLE_SPACE = new Saxon.Hej.value.StringValue(Saxon.Hej.str.StringConstants.SINGLE_SPACE);
        public static readonly Saxon.Hej.value.StringValue TRUE = new Saxon.Hej.value.StringValue(Saxon.Hej.str.StringConstants.TRUE);
        public static readonly Saxon.Hej.value.StringValue FALSE = new Saxon.Hej.value.StringValue(Saxon.Hej.str.StringConstants.FALSE);
        public static readonly Saxon.Hej.value.StringValue ZERO_LENGTH_UNTYPED = new Saxon.Hej.value.UntypedAtomicValue(Saxon.Hej.str.EmptyUnicodeString.getInstance());
        protected internal StringValue()  {
            content = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public StringValue(Saxon.Hej.str.UnicodeString content) : this (content, Saxon.Hej.type.BuiltInAtomicType.STRING) {
        }
        public StringValue(Saxon.Hej.str.UnicodeString content, Saxon.Hej.type.AtomicType type)  {
            this.content = content;
            this.typeLabel = type;
        }
        public StringValue(string value) : this (value, Saxon.Hej.type.BuiltInAtomicType.STRING) {
        }
        public StringValue(string value, Saxon.Hej.type.AtomicType typeLabel)  {
            this.content = Saxon.Hej.str.StringView.of(value).tidy();
            this.typeLabel = typeLabel;
        }
        public override Saxon.Hej.value.StringValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            if ((typeLabel == this.typeLabel)) {
                return this;
            } else {
                Saxon.Hej.value.StringValue v = new Saxon.Hej.value.StringValue();
                v.typeLabel = typeLabel;
                v.content = content;
                return v;
            }
        }
        public static Saxon.Hej.value.StringValue bmp(string content) {
            return new Saxon.Hej.value.StringValue(Saxon.Hej.str.BMPString.of(content));
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return ((typeLabel == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) ? Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC : Saxon.Hej.type.BuiltInAtomicType.STRING);
        }
        public static Saxon.Hej.value.StringValue makeStringValue(string value) {
            if (((value == null) || (value.Length == 0))) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            } else {
                return new Saxon.Hej.value.StringValue(value.ToString());
            }
        }
        public static Saxon.Hej.value.StringValue makeUStringValue(Saxon.Hej.str.UnicodeString value) {
            if (((value == null) || value.isEmpty())) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            } else {
                return new Saxon.Hej.value.StringValue(value);
            }
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return content;
        }
        public Saxon.Hej.str.UnicodeString getContent() {
            return content;
        }
        public long length() {
            return content.length();
        }
        public int length32() {
            return content.length32();
        }
        public bool isEmpty() {
            return content.isEmpty();
        }
        public Saxon.Hej.tree.iter.AtomicIterator iterateCharacters() { lock(this) {
            return new Saxon.Hej.str.CodepointIterator(codePoints());
        }}
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return collator.getCollationKey(this.getUnicodeStringValue());
        }
        public Saxon.Hej.value.Base64BinaryValue getCodepointCollationKey() {
            int len = getContent().length32();
            byte[] result = new byte[(len*3)];
            for (int i = 0, j = 0; (i<len); i++) {
                int c = getContent().codePointAt(i);
                result[j++] = (byte)(((c>>16)));
                result[j++] = (byte)(((c>>8)));
                result[j++] = (byte)(c);
            }
            return new Saxon.Hej.value.Base64BinaryValue(result);
        }
        public Saxon.Hej.z.IntIterator codePoints() {
            return content.codePoints();
        }
        public override int GetHashCode() {
            int h = 0;
            int count = 0;
            Saxon.Hej.z.IntIterator iter = codePoints();
            while (iter.hasNext()) {
                h = ((31*h)+iter.next());
                if ((++count>=100)) {
                    break ;
                }
            }
            return h;
        }
        public override bool Equals(System.Object o) {
            if ((o is Saxon.Hej.value.StringValue)) {
                return content.Equals(((Saxon.Hej.value.StringValue)(o)).content);
            } else {
                return false;
            }
        }
        public override bool effectiveBooleanValue() {
            return !(isEmpty());
        }
        public override string ToString() {
            return getContent().ToString();
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return content;
        }
        public override string toShortString() {
            string s = content.ToString();
            if ((s.Length>40)) {
                s = ((s.Substring(0, 20 - 0)+" ... ")+s.Substring((s.Length-20)));
            }
            s = (("\""+s)+'\"');
            if ((typeLabel == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                s = ("u"+s);
            }
            return s;
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return new Inner_XPathComparable_d2000e966(this, collator);
        }
        public override bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return ((((v is Saxon.Hej.value.StringValue) && (((this is Saxon.Hej.value.AnyURIValue) == (v is Saxon.Hej.value.AnyURIValue)))) && ((this.isUntypedAtomic() == v.isUntypedAtomic()))) && Equals(v));
        }
        public sealed class CharacterIterator : Saxon.Hej.tree.iter.AtomicIterator {
            internal int inpos = 0;
            private readonly string value;
            public CharacterIterator(string value)  {
                this.value = value;
            }
            public Saxon.Hej.om.Item next() {
                if ((inpos<value.Length)) {
                    int c = value[inpos++];
                    int current;
                    if (((c>=55296) && (c<=56319))) {
                        try {
                            current = ((((((c-55296))*1024))+(((int)(value[inpos++])-56320)))+65536);
                        } catch (System.IndexOutOfRangeException e) {
                            throw new Saxon.Ejava.lang.AssertionError(("Invalid surrogate at end of string: "+Saxon.Hej.str.StringTool.diagnosticDisplay(value.ToString())));
                        }
                    } else {
                        current = c;
                    }
                    return new Saxon.Hej.value.Int64Value(current);
                } else {
                    return null;
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

        }
        public sealed class Builder : Saxon.Hej.str.UniStringConsumer {
            internal Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
            public Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
                buffer.accept(chars);
                return this;
            }
            public Saxon.Hej.str.UnicodeString getStringValue() {
                return buffer.toUnicodeString();
            }
        
            /* Expanding default method open from interface UniStringConsumer */
            public void open() {
            }

            /* Expanding default method close from interface UniStringConsumer */
            public void close() {
            }

        }
    
        private class Inner_XPathComparable_d2000e966 : Saxon.Hej.expr.sort.XPathComparable {
            private readonly StringValue outerInstance;
            public Inner_XPathComparable_d2000e966(StringValue outerInstance, Saxon.Hej.lib.StringCollator collator) {
                this.outerInstance = outerInstance;
                this.collator = collator;
            }
            private Saxon.Hej.lib.StringCollator collator;
            public int compareTo(Saxon.Hej.expr.sort.XPathComparable o) {
                if ((/*localparam*/o is Saxon.Hej.value.StringValue)) {
                    return /*extraParam*/collator.compareStrings(outerInstance.getContent(), ((Saxon.Hej.value.StringValue)(/*localparam*/o)).content);
                } else {
                    throw new System.InvalidCastException(("Cannot compare xs:string to "+/*localparam*/o.ToString()));
                }
            }
        }

    }
}