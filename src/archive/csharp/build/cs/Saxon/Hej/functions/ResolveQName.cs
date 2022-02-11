using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ResolveQName : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue lex = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((lex == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return resolveQName(lex.getStringValue(), (Saxon.Hej.om.NodeInfo)(arguments[1].head()));
            }
        }
        public static Saxon.Hej.value.QNameValue resolveQName(string lexicalQName, Saxon.Hej.om.NodeInfo element) {
            Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
            Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalQName, true, false, resolver);
            return new Saxon.Hej.value.QNameValue(qName, Saxon.Hej.type.BuiltInAtomicType.QNAME);
        }
    
    }
}