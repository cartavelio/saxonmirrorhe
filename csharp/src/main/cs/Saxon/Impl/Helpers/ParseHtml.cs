//using HtmlAgilityPack;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Saxon.Hej.functions;
using Saxon.Impl.Dom;

namespace Saxon.Impl.Helpers {
    public class ParseHtml : SystemFunction {

        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration config = controller.getConfiguration();
            Saxon.Hej.value.AtomicValue content = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            var angleSharpConfig = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            var angleSharpContext = BrowsingContext.New(angleSharpConfig);
            
            IHtmlParser parser = angleSharpContext.GetService<IHtmlParser>();
            IDocument htmlDoc = parser.ParseDocument(content.getStringValue());
            return new HtmlDocumentWrapper(htmlDoc, getRetainedStaticContext().getStaticBaseUriString(), config);


        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited