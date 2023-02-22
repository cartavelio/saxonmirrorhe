using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class BooleanValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.XPathComparable, Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.value.ContextFreeAtomicValue {
        private readonly bool value;
        public static readonly Saxon.Hej.value.BooleanValue TRUE = new Saxon.Hej.value.BooleanValue(true);
        public static readonly Saxon.Hej.value.BooleanValue FALSE = new Saxon.Hej.value.BooleanValue(false);
        private BooleanValue(bool value) : base (Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) {
            this.value = value;
        }
        public static Saxon.Hej.value.BooleanValue get(bool value) {
            return (value ? TRUE : FALSE);
        }
        public BooleanValue(bool value, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            this.value = value;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.BooleanValue(value, typeLabel);
        }
        public static Saxon.Hej.type.ConversionResult fromString(Saxon.Hej.str.UnicodeString s) {
            long start = Saxon.Hej.value.Whitespace.trimmedStart(s);
            long end = Saxon.Hej.value.Whitespace.trimmedEnd(s);
            if ((start>=0)) {
                long len = (end-start);
                if ((len == 1)) {
                    int first = s.codePointAt(start);
                    if ((first == '0')) {
                        return FALSE;
                    } else {
                        if ((first == '1')) {
                            return TRUE;
                        }
                    }
                } else {
                    if ((len == 4)) {
                        if (((((s.codePointAt(start++) == 't') && (s.codePointAt(start++) == 'r')) && (s.codePointAt(start++) == 'u')) && (s.codePointAt(start) == 'e'))) {
                            return TRUE;
                        }
                    } else {
                        if ((len == 5)) {
                            if ((((((s.codePointAt(start++) == 'f') && (s.codePointAt(start++) == 'a')) && (s.codePointAt(start++) == 'l')) && (s.codePointAt(start++) == 's')) && (s.codePointAt(start) == 'e'))) {
                                return FALSE;
                            }
                        }
                    }
                }
            }
            Saxon.Hej.type.ValidationFailure err = new Saxon.Hej.type.ValidationFailure((("The string "+Saxon.Hej.trans.Err.wrap(s, Saxon.Hej.trans.Err.VALUE))+" cannot be cast to a boolean"));
            err.setErrorCode("FORG0001");
            return err;
        }
        public bool getBooleanValue() {
            return value;
        }
        public override bool effectiveBooleanValue() {
            return value;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return (value ? Saxon.Hej.str.StringConstants.TRUE : Saxon.Hej.str.StringConstants.FALSE);
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
        public int compareTo(Saxon.Hej.expr.sort.XPathComparable other) {
            if ((other is Saxon.Hej.value.BooleanValue)) {
                if ((value == ((Saxon.Hej.value.BooleanValue)(other)).value)) {
                    return 0;
                }
                if (value) {
                    return +(1);
                }
                return -1;
            } else {
                throw new System.InvalidCastException(("Cannot compare xs:boolean to "+other));
            }
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.BooleanValue) && (value == ((Saxon.Hej.value.BooleanValue)(other)).value));
        }
        public override int GetHashCode() {
            return (value ? 0 : 1);
        }
        public override string show() {
            return (this.getUnicodeStringValue()+"()");
        }
    
    }
}