using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class SymbolicName {
        private readonly int kind;
        private readonly Saxon.Hej.om.StructuredQName name;
        public SymbolicName(int kind, Saxon.Hej.om.StructuredQName name)  {
            this.kind = kind;
            this.name = name;
        }
        public override int GetHashCode() {
            return ((kind<<16)^name.GetHashCode());
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Hej.trans.SymbolicName) && (((Saxon.Hej.trans.SymbolicName)(obj)).kind == this.kind)) && ((Saxon.Hej.trans.SymbolicName)(obj)).name.Equals(this.name));
        }
        public int getComponentKind() {
            return kind;
        }
        public Saxon.Hej.om.StructuredQName getComponentName() {
            return name;
        }
        public override string ToString() {
            return ((Saxon.Hej.om.StandardNames.getLocalName(kind)+" ")+name.getDisplayName());
        }
        public virtual string getShortName() {
            return name.getDisplayName();
        }
        public class F : Saxon.Hej.trans.SymbolicName {
            internal int arity;
            public F(Saxon.Hej.om.StructuredQName name, int arity) : base (Saxon.Hej.om.StandardNames.XSL_FUNCTION, name) {
                this.arity = arity;
            }
            public int getArity() {
                return arity;
            }
            public override int GetHashCode() {
                return (base.GetHashCode()^arity);
            }
            public override bool Equals(System.Object obj) {
                return (((obj is Saxon.Hej.trans.SymbolicName.F) && base.Equals(obj)) && (((Saxon.Hej.trans.SymbolicName.F)(obj)).arity == this.arity));
            }
            public override string ToString() {
                return ((base.ToString()+"#")+arity);
            }
            public override string getShortName() {
                return ((base.getShortName()+"#")+arity);
            }
        
        }
    
    }
}