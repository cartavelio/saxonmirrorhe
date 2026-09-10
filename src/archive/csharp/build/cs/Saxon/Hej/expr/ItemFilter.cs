using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ItemFilter : Saxon.Hej.expr.ItemMappingFunction {
        public delegate bool Lambda (Saxon.Hej.om.Item item);
        private readonly Saxon.Hej.expr.ItemFilter.Lambda lambda;
        private ItemFilter(Saxon.Hej.expr.ItemFilter.Lambda lambda)  {
            this.lambda = lambda;
        }
        public static Saxon.Hej.expr.ItemFilter of(Saxon.Hej.expr.ItemFilter.Lambda lambda) {
            return new Saxon.Hej.expr.ItemFilter(lambda);
        }
        public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
            if (/*Delegate.test()*/lambda(item)) {
                return item;
            } else {
                return null;
            }
        }
    
    }
}