using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SimpleTypeIdentityComparison : Saxon.Hej.expr.sort.SimpleTypeComparison {
        private static readonly Saxon.Hej.expr.sort.SimpleTypeIdentityComparison INSTANCE = new Saxon.Hej.expr.sort.SimpleTypeIdentityComparison();
        public static new Saxon.Hej.expr.sort.SimpleTypeIdentityComparison getInstance() {
            return INSTANCE;
        }
        public override bool equal(Saxon.Hej.om.AtomicSequence a, Saxon.Hej.om.AtomicSequence b) {
            return base.equalOrIdentical(a, b);
        }
    
    }
}