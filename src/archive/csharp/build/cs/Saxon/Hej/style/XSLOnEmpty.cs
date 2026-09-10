using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLOnEmpty : Saxon.Hej.style.XSLSequence {
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            base.validate(decl);
            Saxon.Hej.om.SequenceTool.supply(iterateAxis(Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((next => {
                if (!((((next is Saxon.Hej.style.XSLFallback) || (next is Saxon.Hej.style.XSLCatch))))) {
                    compileError("xsl:on-empty must be the last instruction in the sequence constructor");
                }
            } )));
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression e = base.compile(exec, decl);
            return new Saxon.Hej.expr.instruct.OnEmptyExpr(e);
        }
    
    }
}