using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public interface AnyExternalObject : Saxon.Hej.om.Item {
        System.Object getWrappedObject();
        Saxon.Hej.type.ItemType getItemType(Saxon.Hej.type.TypeHierarchy th);
    
    }
}