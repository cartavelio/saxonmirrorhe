using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface PlainType : Saxon.Hej.type.ItemType, Saxon.Hej.type.HyperType {
        Saxon.Hej.om.StructuredQName getTypeName();
        System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes();
        /* implicit =====
        bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th);
        ===== implicit */
        /* implicit =====
        Saxon.Hej.type.ItemType getPrimitiveItemType();
        ===== implicit */
    
    }
}