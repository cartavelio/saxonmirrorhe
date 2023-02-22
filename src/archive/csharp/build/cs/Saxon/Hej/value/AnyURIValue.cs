using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class AnyURIValue : Saxon.Hej.value.StringValue {
        public static readonly Saxon.Hej.value.AnyURIValue EMPTY_URI = new Saxon.Hej.value.AnyURIValue("");
        public AnyURIValue(Saxon.Hej.str.UnicodeString value) : base (((value == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : Saxon.Hej.value.Whitespace.collapseWhitespace(value)), Saxon.Hej.type.BuiltInAtomicType.ANY_URI) {
        }
        public AnyURIValue(string value) : this (Saxon.Hej.str.StringView.tidy(value)) {
        }
        public AnyURIValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.type.AtomicType type) : base (((value == null) ? "" : Saxon.Hej.value.Whitespace.collapseWhitespace(value).ToString()), type) {
        }
        public override Saxon.Hej.value.AnyURIValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.AnyURIValue(this.getUnicodeStringValue(), typeLabel);
        }
        public Saxon.Hej.value.StringValue convertToString() {
            return new Saxon.Hej.value.StringValue(getContent(), Saxon.Hej.type.BuiltInAtomicType.STRING);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_URI;
        }
        public static string decode(string s) {
            return System.Uri.UnescapeDataString(s);
        }
        private static byte hex(char high, char low) {
            return (byte)(((((hexToDec(high)<<4))|hexToDec(low))));
        }
        private static int hexToDec(char c) {
            if (((c>='0') && (c<='9'))) {
                return (c-'0');
            } else {
                if (((c>='a') && (c<='f'))) {
                    return ((c-'a')+10);
                } else {
                    if (((c>='A') && (c<='F'))) {
                        return ((c-'A')+10);
                    } else {
                        return 0;
                    }
                }
            }
        }
    
    }
}