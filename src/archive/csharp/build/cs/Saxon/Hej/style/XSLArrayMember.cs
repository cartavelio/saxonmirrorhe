using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLArrayMember : Saxon.Hej.style.XSLSequence {
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((getSelectExpression() == null)) {
                setSelectExpression(compileSequenceConstructor(exec, decl, false));
            }
            Saxon.Hej.functions.SystemFunction arrayMem = Saxon.Hej.functions.registry.XPath40FunctionSet.getInstance().makeFunction("parcel", 1);
            return arrayMem.makeFunctionCall(getSelectExpression());
        }
    
    }
}