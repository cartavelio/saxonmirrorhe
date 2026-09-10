using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class ObjectToBeSorted {
        public Saxon.Hej.om.Item value;
        public Saxon.Hej.value.AtomicValue[] sortKeyValues;
        public int originalPosition;
        public ObjectToBeSorted(int numberOfSortKeys)  {
            sortKeyValues = new Saxon.Hej.value.AtomicValue[numberOfSortKeys];
        }
    
    }
}