using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class InScopeNamespacesFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo element = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            Saxon.Hej.om.NamespaceMap nsMap = element.getAllNamespaces();
            if ((nsMap == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.ma.map.DictionaryMap dict = new Saxon.Hej.ma.map.DictionaryMap();
            foreach (Saxon.Hej.om.NamespaceBinding binding in nsMap) {
                dict.initialPut(binding.getPrefix(), new Saxon.Hej.value.StringValue(binding.getURI()));
            }
            return dict;
        }
    
    }
}