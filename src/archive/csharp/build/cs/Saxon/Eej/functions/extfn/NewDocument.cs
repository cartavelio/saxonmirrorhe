using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class NewDocument : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.@event.Builder builder = context.getController().makeBuilder();
            builder.setBaseURI(getStaticBaseUriString());
            Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(builder);
            cco.setSystemId(getStaticBaseUriString());
            cco.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            cco.open();
            cco.startDocument(0);
            Saxon.Hej.om.SequenceTool.supply(arguments[0].iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(/* Unresolved method reference */cco.append));
            cco.endDocument();
            cco.close();
            return builder.getCurrentRoot();
        }
    
    }
}