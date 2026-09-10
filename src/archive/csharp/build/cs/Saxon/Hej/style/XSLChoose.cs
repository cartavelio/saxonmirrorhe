using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLChoose : Saxon.Hej.style.XSLChooseOrSwitch {
        protected internal override void compileConditions(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.expr.Expression[] conditions) {
            int w = 0;
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((curr is Saxon.Hej.style.XSLWhen)) {
                    conditions[w] = ((Saxon.Hej.style.XSLWhen)(curr)).getCondition();
                    w++;
                } else {
                    if ((curr is Saxon.Hej.style.XSLOtherwise)) {
                        Saxon.Hej.expr.Expression otherwise = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE);
                        otherwise.setRetainedStaticContext(makeRetainedStaticContext());
                        conditions[w] = otherwise;
                        w++;
                    }
                }
            }
        }
    
    }
}