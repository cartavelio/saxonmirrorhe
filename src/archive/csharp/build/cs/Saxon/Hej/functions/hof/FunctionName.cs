using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class FunctionName : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Function f = (Saxon.Hej.om.Function)(arguments[0].head());
            System.Diagnostics.Debug.Assert((f != null));
            Saxon.Hej.om.StructuredQName name = f.getFunctionName();
            if ((name == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return new Saxon.Hej.value.QNameValue(name, Saxon.Hej.type.BuiltInAtomicType.QNAME);
                }
            }
        }
    
    }
}