using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class UntypedAtomicValue : Saxon.Hej.value.StringValue {
        public UntypedAtomicValue(Saxon.Hej.str.UnicodeString value) : base (value, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) {
        }
        public override Saxon.Hej.value.UntypedAtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            if (!(typeLabel.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            return this;
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        public override string toShortString() {
            return ("u"+base.toShortString());
        }
    
    }
}