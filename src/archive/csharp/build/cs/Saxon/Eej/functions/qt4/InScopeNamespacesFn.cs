using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class InScopeNamespacesFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo element = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            Saxon.Hej.om.NamespaceMap nsMap = element.getAllNamespaces();
            if ((nsMap == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Impl.DataModel.ImmMap dict = new Saxon.Impl.DataModel.ImmMap();
            dict.initialPut(new Saxon.Hej.value.StringValue("xml", Saxon.Hej.type.BuiltInAtomicType.NCNAME), new Saxon.Hej.value.StringValue(Saxon.Hej.lib.NamespaceConstant.XML));
            foreach (Saxon.Hej.om.NamespaceBinding binding in nsMap) {
                Saxon.Hej.value.StringValue prefix = new Saxon.Hej.value.StringValue(binding.getPrefix(), (binding.getPrefix().Length==0 ? Saxon.Hej.type.BuiltInAtomicType.STRING : Saxon.Hej.type.BuiltInAtomicType.NCNAME));
                dict.initialPut(prefix, new Saxon.Hej.value.StringValue(binding.getNamespaceUri().toUnicodeString()));
            }
            return dict;
        }
    
    }
}