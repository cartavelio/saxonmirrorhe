using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public interface TupleType : Saxon.Hej.type.FunctionItemType {
        System.Collections.Generic.IEnumerable<string> getFieldNames();
        Saxon.Hej.value.SequenceType getFieldType(string field);
        bool isOptionalField(string field);
        bool isExtensible();
    
    }
}