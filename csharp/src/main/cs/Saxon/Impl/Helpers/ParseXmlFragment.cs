using System;
using System.Impl.Helpers;
using System.Xml;
using System.Xml.XPath;
using Saxon.Hej.functions;
using Saxon.Hej.pull;
using Saxon.Hej.value;

namespace Saxon.Impl.Helpers {
    public class ParseXmlFragment : SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration config = controller.getConfiguration();
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
                        throw new Saxon.Hej.trans.XPathException(
                            "Encoding attribute in XML fragment declaration is required", "FODC006");
                    }

                    string standalone = ProcInstParser.getPseudoAttribute(decl, "standalone");
                    if (standalone != null) {
                        throw new Saxon.Hej.trans.XPathException(
                            "Standalone attribute in XML fragment declaration is not allowed", "FODC0006");
                    }
                }
            }
            
            XmlTextReader reader = new(content, XmlNodeType.Element, 
                new XmlParserContext(null, null, null, XmlSpace.None));
            reader.Normalization = true;
            DotNetPullProvider provider = new(reader) {
                AcceptFragment = true
            };
            PullSource source = new(provider);
            return config.buildDocumentTree(source).getRootNode();
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited