using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class UndeclaredVariable : Saxon.Hej.expr.instruct.GlobalVariable {
        public UndeclaredVariable()  {
        }
        public void transferReferences(Saxon.Hej.expr.instruct.GlobalVariable var) {
            foreach (Saxon.Hej.expr.BindingReference @ref in references) {
                var.registerReference(@ref);
            }
            references = new System.Collections.Generic.List<Saxon.Hej.expr.BindingReference>();
        }
        public override void compile(Saxon.Hej.expr.instruct.Executable exec, int slot) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Attempt to compile a place-holder for an undeclared variable");
        }
    
    }
}