using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class PedigreeFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item @in = arguments[0].head();
            if ((@in is Saxon.Eej.xsltextn.pedigree.PedigreeValue)) {
                Saxon.Hej.ma.map.DictionaryMap result = new Saxon.Hej.ma.map.DictionaryMap();
                Saxon.Eej.xsltextn.pedigree.Pedigree pedigree = ((Saxon.Eej.xsltextn.pedigree.PedigreeValue)(@in)).getPedigree();
                if ((pedigree.getContainer() != null)) {
                    result.initialPut("container", (Saxon.Hej.om.Item)(pedigree.getContainer()));
                }
                if ((pedigree.getIndex() != -1)) {
                    result.initialPut("index", Saxon.Hej.value.Int64Value.makeIntegerValue((pedigree.getIndex()+1)));
                }
                if ((pedigree.getKey() != null)) {
                    result.initialPut("key", pedigree.getKey());
                }
                return result;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }
    
    }
}