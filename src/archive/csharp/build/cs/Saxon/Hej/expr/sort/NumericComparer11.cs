using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class NumericComparer11 : Saxon.Hej.expr.sort.NumericComparer {
        private static readonly Saxon.Hej.expr.sort.NumericComparer11 THE_INSTANCE = new Saxon.Hej.expr.sort.NumericComparer11();
        public static new Saxon.Hej.expr.sort.NumericComparer getInstance() {
            return THE_INSTANCE;
        }
        protected internal NumericComparer11()  {
            converter = Saxon.Hej.value.StringToDouble11.getInstance();
        }
        public override string save() {
            return "NC11";
        }
    
    }
}