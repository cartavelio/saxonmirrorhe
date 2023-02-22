using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class DecimalValue : Saxon.Hej.value.NumericValue {
        public abstract override Singulink.Numerics.BigDecimal getDecimalValue();
    
    }
}