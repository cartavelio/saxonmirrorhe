using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public abstract class NumericGroupFormatter {
        protected internal Saxon.Hej.str.UnicodeString adjustedPicture;
        public Saxon.Hej.str.UnicodeString getAdjustedPicture() {
            return adjustedPicture;
        }
        public abstract string format(string value);
        public abstract string getSeparator();
    
    }
}