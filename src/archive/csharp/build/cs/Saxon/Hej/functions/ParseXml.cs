using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ParseXml : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue input = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((input == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return parseXmlPull(input, context);
            }
        }
        private static bool usePushParser() {
            return false;
        }
        private Saxon.Hej.om.NodeInfo parseXmlPull(Saxon.Hej.value.StringValue inputArg, Saxon.Hej.expr.XPathContext context) {
            string baseURI = getRetainedStaticContext().getStaticBaseUriString();
            try {
                Saxon.Hej.Controller controller = context.getController();
                if ((controller == null)) {
                    throw new Saxon.Hej.trans.XPathException("parse-xml() function is not available in this environment");
                }
                Saxon.Hej.Configuration config = context.getConfiguration();
                System.IO.StringReader sr = new System.IO.StringReader(inputArg.getStringValue());
                Saxon.Ejavax.xml.transform.stream.StreamSource ss = new Saxon.Ejavax.xml.transform.stream.StreamSource(sr, baseURI);
                Saxon.Ejavax.xml.transform.Source pullSource = Saxon.Hej.Version.platform.resolveSource(ss, config);
                Saxon.Hej.@event.Builder b = Saxon.Hej.om.TreeModel.TINY_TREE.makeBuilder(controller.makePipelineConfiguration());
                Saxon.Hej.@event.Receiver s = b;
                Saxon.Hej.lib.ParseOptions options = config.getParseOptions();
                Saxon.Hej.expr.PackageData pd = getRetainedStaticContext().getPackageData();
                if ((pd is Saxon.Hej.style.StylesheetPackage)) {
                    options = options.withSpaceStrippingRule(((Saxon.Hej.style.StylesheetPackage)(pd)).getSpaceStrippingRule());
                    if (((Saxon.Hej.style.StylesheetPackage)(pd)).isStripsTypeAnnotations()) {
                        s = config.getAnnotationStripper(s);
                    }
                } else {
                    options = options.withSpaceStrippingRule(Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance());
                }
                s.setPipelineConfiguration(b.getPipelineConfiguration());
                Saxon.Hej.@event.Sender.send(pullSource, s, options);
                Saxon.Hej.om.NodeInfo root = b.getCurrentRoot();
                if ((root is Saxon.Hej.tree.tiny.TinyDocumentImpl)) {
                    Saxon.Hej.tree.tiny.TinyDocumentImpl node = (Saxon.Hej.tree.tiny.TinyDocumentImpl)(root);
                    node.setBaseURI(baseURI);
                    node.getTreeInfo().setUserData("saxon:document-uri", "");
                } else {
                    if ((root is Saxon.Hej.tree.linked.DocumentImpl)) {
                        Saxon.Hej.tree.linked.DocumentImpl node = (Saxon.Hej.tree.linked.DocumentImpl)(root);
                        node.setBaseURI(baseURI);
                        node.getTreeInfo().setUserData("saxon:document-uri", "");
                    }
                }
                b.reset();
                return root;
            } catch (Saxon.Hej.trans.XPathException err) {
                string msg = makeParsingErrorMessage(err);
                Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(msg, "FODC0006");
                xe.maybeSetContext(context);
                throw xe;
            }
        }
        private string makeParsingErrorMessage(Saxon.Hej.trans.XPathException err) {
            string msg = "First argument to parse-xml() is not a well-formed and namespace-well-formed XML document. ";
            msg += err.Message;
            System.Exception cause = err.InnerException;
            if ((cause != null)) {
                msg += cause.Message;
            }
            return msg;
        }
    
    }
}