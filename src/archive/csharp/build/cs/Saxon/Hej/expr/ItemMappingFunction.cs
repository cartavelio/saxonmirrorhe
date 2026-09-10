using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface ItemMappingFunction {
        Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item);
    
    }
}