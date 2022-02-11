using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface ListType : Saxon.Hej.type.SimpleType, Saxon.Hej.type.CastingTarget {
        Saxon.Hej.type.SimpleType getItemType();
    
    }
}