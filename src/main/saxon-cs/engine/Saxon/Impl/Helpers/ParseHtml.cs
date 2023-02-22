using HtmlAgilityPack;
using Saxon.Hej.functions;
using Saxon.Impl.Dom;

namespace Saxon.Impl.Helpers {
    public class ParseHtml : SystemFunction {

        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration config = controller.getConfiguration();
            Saxon.Hej.value.AtomicValue content = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content.getStringValue());
            return new HtmlDocumentWrapper(doc, getStaticBaseUriString(), config);
            
        }
    }
}

// Copyright (c) 2021 Saxonica Limited