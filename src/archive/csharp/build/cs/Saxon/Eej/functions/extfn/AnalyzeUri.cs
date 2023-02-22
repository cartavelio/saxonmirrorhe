using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class AnalyzeUri : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return /*134*/Saxon.Eej.functions.qt4.Parts.uriComponents(arguments[0].head().getStringValue());
        }
    
    }
}