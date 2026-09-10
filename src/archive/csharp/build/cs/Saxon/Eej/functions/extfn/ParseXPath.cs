using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class ParseXPath : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration configuration = controller.getConfiguration();
            Saxon.Hej.value.AtomicValue content = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            throw new Saxon.Hej.trans.XPathException("saxon:parse-XPath() is only supported in Saxon-JS", Saxon.Hej.trans.SaxonErrorCode.SXJS0001);
        }
    
    }
}