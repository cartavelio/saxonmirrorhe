using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLSort : Saxon.Hej.style.XSLSortOrMergeKey {
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            base.validate(decl);
            stable = typeCheck("stable", stable);
            sortKeyDefinition.setStable(stable);
        }
        protected internal override Saxon.Hej.expr.Expression getStable() {
            return stable;
        }
    
    }
}