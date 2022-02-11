using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public sealed class LocalOrderComparer : System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> {
        private static readonly Saxon.Hej.expr.sort.LocalOrderComparer instance = new Saxon.Hej.expr.sort.LocalOrderComparer();
        public static Saxon.Hej.expr.sort.LocalOrderComparer getInstance() {
            return instance;
        }
        public int compare(Saxon.Hej.om.NodeInfo a, Saxon.Hej.om.NodeInfo b) {
            return a.compareOrder(b);
        }
        // Extra method to make the class a C# Comparer
        public override int Compare(Saxon.Hej.om.NodeInfo a, Saxon.Hej.om.NodeInfo b) {
            return compare(a, b);
        }
    
    }
}