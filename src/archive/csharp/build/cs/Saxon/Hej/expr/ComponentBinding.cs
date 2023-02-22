using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ComponentBinding {
        private readonly Saxon.Hej.trans.SymbolicName symbolicName;
        private readonly Saxon.Hej.expr.Component target;
        public ComponentBinding(Saxon.Hej.trans.SymbolicName name, Saxon.Hej.expr.Component target)  {
            this.symbolicName = name;
            this.target = target;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return symbolicName;
        }
        public Saxon.Hej.expr.Component getTarget() {
            return target;
        }
    
    }
}