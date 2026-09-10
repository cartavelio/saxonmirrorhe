using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ItemMapper : Saxon.Hej.expr.ItemMappingFunction {
        public delegate Saxon.Hej.om.Item Lambda (Saxon.Hej.om.Item item);
        private readonly Saxon.Hej.expr.ItemMapper.Lambda lambda;
        private ItemMapper(Saxon.Hej.expr.ItemMapper.Lambda lambda)  {
            this.lambda = lambda;
        }
        public static Saxon.Hej.expr.ItemMapper of(Saxon.Hej.expr.ItemMapper.Lambda lambda) {
            return new Saxon.Hej.expr.ItemMapper(lambda);
        }
        public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
            return /*Delegate.map()*/lambda(item);
        }
    
    }
}