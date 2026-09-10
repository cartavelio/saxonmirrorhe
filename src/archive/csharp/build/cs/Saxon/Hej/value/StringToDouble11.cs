using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class StringToDouble11 : Saxon.Hej.type.StringToDouble {
        private static readonly Saxon.Hej.value.StringToDouble11 THE_INSTANCE = new Saxon.Hej.value.StringToDouble11();
        public static new Saxon.Hej.value.StringToDouble11 getInstance() {
            return THE_INSTANCE;
        }
        private StringToDouble11()  {
        }
        protected internal override double signedPositiveInfinity() {
            return System.Double.PositiveInfinity;
        }
    
    }
}