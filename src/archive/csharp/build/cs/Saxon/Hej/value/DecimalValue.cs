using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class DecimalValue : Saxon.Hej.value.NumericValue {
        public DecimalValue(Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
        }
        public abstract override Singulink.Numerics.BigDecimal getDecimalValue();
    
    }
}