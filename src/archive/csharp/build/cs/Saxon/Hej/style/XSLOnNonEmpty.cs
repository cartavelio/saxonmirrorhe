using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLOnNonEmpty : Saxon.Hej.style.XSLSequence {
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression e = base.compile(exec, decl);
            return new Saxon.Hej.expr.instruct.OnNonEmptyExpr(e);
        }
    
    }
}