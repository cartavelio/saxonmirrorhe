using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class WithPedigree : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Item call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item @in = arguments[0].head();
            Saxon.Hej.om.Item @out = makeRootWithPedigree(@in);
            return ((@out == null) ? @in : @out);
        }
        public static Saxon.Hej.om.FunctionItem makeRootWithPedigree(Saxon.Hej.om.Item root) {
            if ((root is Saxon.Hej.ma.map.MapItem)) {
                return new Saxon.Eej.xsltextn.pedigree.PedigreeMapItem((Saxon.Hej.ma.map.MapItem)(root));
            } else {
                if ((root is Saxon.Hej.ma.arrays.ArrayItem)) {
                    return new Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem((Saxon.Hej.ma.arrays.ArrayItem)(root));
                } else {
                    return null;
                }
            }
        }
    
    }
}