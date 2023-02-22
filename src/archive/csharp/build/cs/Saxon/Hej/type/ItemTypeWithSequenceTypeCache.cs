using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface ItemTypeWithSequenceTypeCache : Saxon.Hej.type.ItemType {
        Saxon.Hej.value.SequenceType one();
        Saxon.Hej.value.SequenceType zeroOrOne();
        Saxon.Hej.value.SequenceType oneOrMore();
        Saxon.Hej.value.SequenceType zeroOrMore();
    
    }
}