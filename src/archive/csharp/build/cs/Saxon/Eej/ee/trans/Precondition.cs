using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public abstract class Precondition {
        public const int PRECONDITION_UNKNOWN = 0;
        public const int PRECONDITION_FALSE = -1;
        public const int PRECONDITION_TRUE = 1;
        internal int slotNumber;
        public void setSlotNumber(int slotNumber) {
            this.slotNumber = slotNumber;
        }
        public bool isSatisfied(Saxon.Eej.ee.trans.PreconditionMatcher matcher) {
            int state = matcher.getPreconditionState(slotNumber);
            if ((state == PRECONDITION_UNKNOWN)) {
                bool b = testPrecondition(matcher);
                matcher.setPreconditionState(slotNumber, b);
            }
            return (state == PRECONDITION_TRUE);
        }
        protected internal abstract bool testPrecondition(Saxon.Eej.ee.trans.PreconditionMatcher matcher);
        public class StringHashPrecondition : Saxon.Eej.ee.trans.Precondition {
            private readonly Saxon.Eej.ee.trans.Characteristic characteristic;
            private readonly int hash;
            private readonly Saxon.Hej.lib.StringCollator collator;
            public StringHashPrecondition(Saxon.Eej.ee.trans.Characteristic characteristic, Saxon.Hej.expr.sort.AtomicMatchKey required, Saxon.Hej.lib.StringCollator collator)  {
                this.characteristic = characteristic;
                this.collator = collator;
                this.hash = required.GetHashCode();
            }
            protected internal override bool testPrecondition(Saxon.Eej.ee.trans.PreconditionMatcher matcher) {
                Saxon.Hej.z.IntSet comparand = (Saxon.Hej.z.IntSet)(matcher.obtainCharacteristic(characteristic));
                return comparand.contains(hash);
            }
            public override int GetHashCode() {
                return (characteristic.GetHashCode()^hash);
            }
            public override bool Equals(System.Object obj) {
                if ((obj is Saxon.Eej.ee.trans.Precondition.StringHashPrecondition)) {
                    Saxon.Eej.ee.trans.Precondition.StringHashPrecondition other = (Saxon.Eej.ee.trans.Precondition.StringHashPrecondition)(obj);
                    return (((collator == other.collator) && (hash == other.hash)) && characteristic.Equals(other.characteristic));
                } else {
                    return false;
                }
            }
            public override string ToString() {
                return ((("hash("+characteristic)+")=")+Saxon.Impl.Helpers.Utils.intToHexString(hash));
            }
        
        }
        public class NumericComparisonPrecondition : Saxon.Eej.ee.trans.Precondition {
            private readonly Saxon.Eej.ee.trans.Characteristic characteristic;
            private readonly long required;
            public NumericComparisonPrecondition(Saxon.Eej.ee.trans.Characteristic characteristic, long required)  {
                this.characteristic = characteristic;
                this.required = required;
            }
            protected internal override bool testPrecondition(Saxon.Eej.ee.trans.PreconditionMatcher matcher) {
                Saxon.Hej.value.NumericValue comparand = (Saxon.Hej.value.NumericValue)(matcher.obtainCharacteristic(characteristic));
                return (!(comparand.isNaN()) && (comparand.compareTo(required) == 0));
            }
            public override int GetHashCode() {
                return (characteristic.GetHashCode()^(int)(required));
            }
            public override bool Equals(System.Object obj) {
                if ((obj is Saxon.Eej.ee.trans.Precondition.NumericComparisonPrecondition)) {
                    Saxon.Eej.ee.trans.Precondition.NumericComparisonPrecondition other = (Saxon.Eej.ee.trans.Precondition.NumericComparisonPrecondition)(obj);
                    return ((required == other.required) && characteristic.Equals(other.characteristic));
                } else {
                    return false;
                }
            }
            public override string ToString() {
                return ((("hash("+characteristic)+")=")+Saxon.Impl.Helpers.Utils.longToHexString(required));
            }
        
        }
        public class BooleanPrecondition : Saxon.Eej.ee.trans.Precondition {
            private readonly Saxon.Eej.ee.trans.Characteristic characteristic;
            public BooleanPrecondition(Saxon.Eej.ee.trans.Characteristic characteristic)  {
                this.characteristic = characteristic;
            }
            protected internal override bool testPrecondition(Saxon.Eej.ee.trans.PreconditionMatcher matcher) {
                return ((Saxon.Hej.value.BooleanValue)(matcher.obtainCharacteristic(characteristic))).getBooleanValue();
            }
            public override int GetHashCode() {
                return (characteristic.GetHashCode()^7720);
            }
            public override bool Equals(System.Object obj) {
                if ((obj is Saxon.Eej.ee.trans.Precondition.BooleanPrecondition)) {
                    Saxon.Eej.ee.trans.Precondition.BooleanPrecondition other = (Saxon.Eej.ee.trans.Precondition.BooleanPrecondition)(obj);
                    return characteristic.Equals(other.characteristic);
                } else {
                    return false;
                }
            }
            public override string ToString() {
                return characteristic.ToString();
            }
        
        }
    
    }
}