using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class EQName : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item first = arguments[0].head();
            if ((first == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string arg = first.getStringValue();
            Saxon.Hej.om.StructuredQName qname = Saxon.Hej.om.StructuredQName.fromLexicalQName(arg, false, true, getRetainedStaticContext());
            return new Saxon.Hej.value.QNameValue(qname, Saxon.Hej.type.BuiltInAtomicType.QNAME);
        }
    
    }
}