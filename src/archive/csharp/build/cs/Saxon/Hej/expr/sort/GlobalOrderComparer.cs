using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public sealed class GlobalOrderComparer : System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> {
        private static readonly Saxon.Hej.expr.sort.GlobalOrderComparer instance = new Saxon.Hej.expr.sort.GlobalOrderComparer();
        public static Saxon.Hej.expr.sort.GlobalOrderComparer getInstance() {
            return instance;
        }
        public int compare(Saxon.Hej.om.NodeInfo a, Saxon.Hej.om.NodeInfo b) {
            if ((a == b)) {
                return 0;
            }
            long d1 = a.getTreeInfo().getDocumentNumber();
            long d2 = b.getTreeInfo().getDocumentNumber();
            if ((d1 == d2)) {
                return a.compareOrder(b);
            }
            return (int)(System.Math.Sign((d1-d2)));
        }
        // Extra method to make the class a C# Comparer
        public override int Compare(Saxon.Hej.om.NodeInfo a, Saxon.Hej.om.NodeInfo b) {
            return compare(a, b);
        }
    
    }
}