using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class InScopePrefixes : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo element = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            string[] prefixes = element.getAllNamespaces().getPrefixArray();
            System.Collections.Generic.IList<Saxon.Hej.om.Item> result = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            foreach (string s in prefixes) {
                result.Add(new Saxon.Hej.value.StringValue(s));
            }
            result.Add(Saxon.Hej.value.StringValue.bmp("xml"));
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
        }
    
    }
}