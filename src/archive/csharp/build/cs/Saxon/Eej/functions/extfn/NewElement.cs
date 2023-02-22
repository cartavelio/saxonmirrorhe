using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class NewElement : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.@event.Builder builder = context.getController().makeBuilder();
            builder.setBaseURI(getStaticBaseUriString());
            Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(builder);
            cco.setSystemId(getStaticBaseUriString());
            cco.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            cco.open();
            Saxon.Hej.value.AtomicValue nameArg = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            Saxon.Hej.value.QNameValue name;
            if ((nameArg is Saxon.Hej.value.QNameValue)) {
                name = (Saxon.Hej.value.QNameValue)(nameArg);
            } else {
                string s = nameArg.getStringValue();
                Saxon.Hej.om.StructuredQName q = Saxon.Hej.om.StructuredQName.fromLexicalQName(s, false, true, getRetainedStaticContext());
                name = new Saxon.Hej.value.QNameValue(q, Saxon.Hej.type.BuiltInAtomicType.QNAME);
            }
            cco.startElement(new Saxon.Hej.om.FingerprintedQName(name.getStructuredQName(), context.getNamePool()), Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, 0);
            Saxon.Hej.om.SequenceTool.supply(arguments[1].iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(/* Unresolved method reference */cco.append));
            cco.endElement();
            cco.close();
            return builder.getCurrentRoot();
        }
    
    }
}