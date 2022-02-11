using System;
using System.Impl.Helpers;
using System.Xml;
using System.Xml.XPath;
using net.sf.saxon.functions;
using net.sf.saxon.pull;
using net.sf.saxon.value;

namespace Saxon.Impl.Helpers {
    public class ParseXmlFragment : SystemFunction {
        public override net.sf.saxon.om.Sequence call(net.sf.saxon.expr.XPathContext context, net.sf.saxon.om.Sequence[] arguments) {
            net.sf.saxon.Controller controller = context.getController();
            net.sf.saxon.Configuration config = controller.getConfiguration();
            AtomicValue arg = (AtomicValue)arguments[0].head();

            if (arg == null) {
                return EmptySequence.getInstance();
            }
            string content = arg.getStringValue();
            
            // The Microsoft parser does inadequate checking of the text declaration (which is not quite
            // the same as an XML declaration). So we apply some extra checks ourselves.

            if (content.StartsWith("<?xml")) {
                int index = content.IndexOf("?>", StringComparison.InvariantCulture);
                if (index > 0) {
                    string decl = content.Substring(5, index - 5);
                    string encoding = ProcInstParser.getPseudoAttribute(decl, "encoding");
                    if (encoding == null) {
                        throw new net.sf.saxon.trans.XPathException(
                            "Encoding attribute in XML fragment declaration is required", "FODC006");
                    }

                    string standalone = ProcInstParser.getPseudoAttribute(decl, "standalone");
                    if (standalone != null) {
                        throw new net.sf.saxon.trans.XPathException(
                            "Standalone attribute in XML fragment declaration is not allowed", "FODC0006");
                    }
                }
            }
            
            XmlTextReader reader = new XmlTextReader(content, XmlNodeType.Element, 
                new XmlParserContext(null, null, null, XmlSpace.None));
            reader.Normalization = true;
            DotNetPullProvider provider = new DotNetPullProvider(reader) {
                AcceptFragment = true
            };
            PullSource source = new PullSource(provider);
            return config.buildDocumentTree(source).getRootNode();
        }
    }
}