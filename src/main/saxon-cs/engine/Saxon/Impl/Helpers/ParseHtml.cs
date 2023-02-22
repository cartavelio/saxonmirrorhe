using HtmlAgilityPack;
using net.sf.saxon.functions;
using Saxon.Impl.Dom;

namespace Saxon.Impl.Helpers {
    public class ParseHtml : SystemFunction {

        public override net.sf.saxon.om.Sequence call(net.sf.saxon.expr.XPathContext context, net.sf.saxon.om.Sequence[] arguments) {
            net.sf.saxon.Controller controller = context.getController();
            net.sf.saxon.Configuration config = controller.getConfiguration();
            net.sf.saxon.value.AtomicValue content = (net.sf.saxon.value.AtomicValue)(arguments[0].head());
            HtmlDocument doc = new HtmlDocument();
            return new HtmlDocumentWrapper(doc, getStaticBaseUriString(), config);
            
        }
    }
}