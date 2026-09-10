//using HtmlAgilityPack;

using System.IO;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Saxon.Hej.expr;
using Saxon.Hej.functions;
using Saxon.Hej.value;
using Saxon.Impl.Dom;

namespace Saxon.Impl.Helpers {
    public class ParseHtml : SystemFunction {

        public override Saxon.Hej.om.Sequence call(XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration config = controller.getConfiguration();
            Saxon.Hej.value.AtomicValue content = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if (content == null) {
                return EmptySequence.INSTANCE;
            }
            var angleSharpConfig = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            var angleSharpContext = BrowsingContext.New(angleSharpConfig);
            
            IHtmlParser parser = angleSharpContext.GetService<IHtmlParser>();
            IDocument htmlDoc = parser.ParseDocument(content.getStringValue());
            return new HtmlDocumentWrapper(htmlDoc, getRetainedStaticContext().getStaticBaseUriString(), config);
            
        }

        public static Saxon.Hej.om.Item ParseStream(XPathContext context, Stream input, string baseUri) {
            var angleSharpConfig = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            var angleSharpContext = BrowsingContext.New(angleSharpConfig);
            
            IHtmlParser parser = angleSharpContext.GetService<IHtmlParser>();
            IDocument htmlDoc = parser.ParseDocument(input);
            return new HtmlDocumentWrapper(htmlDoc,  baseUri, context.getConfiguration());

        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited