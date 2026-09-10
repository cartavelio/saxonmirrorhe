using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GroupToBeSorted : Saxon.Hej.expr.sort.ObjectToBeSorted {
        public Saxon.Hej.om.AtomicSequence currentGroupingKey;
        public Saxon.Hej.om.MemoSequence currentGroup;
        public GroupToBeSorted(int numberOfSortKeys) : base (numberOfSortKeys) {
        }
    
    }
}