using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Serialize : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((node == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            Saxon.Ejava.util.Properties props = null;
            Saxon.Hej.serialize.SerializationProperties outputProps;
            Saxon.Hej.om.Item secondArg = arguments[1].head();
            if ((secondArg is Saxon.Hej.value.StringValue)) {
                string lexicalName = secondArg.getStringValue();
                Saxon.Hej.expr.PackageData pack = getRetainedStaticContext().getPackageData();
                if ((pack is Saxon.Hej.style.StylesheetPackage)) {
                    Saxon.Hej.om.StructuredQName qName;
                    try {
                        qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalName, false, true, getRetainedStaticContext());
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.XPathException(((("Invalid output format name "+lexicalName)+". ")+e.Message));
                    }
                    props = ((Saxon.Hej.style.StylesheetPackage)(pack)).getNamedOutputProperties(qName);
                    if ((props == null)) {
                        throw new Saxon.Hej.trans.XPathException(("Unknown output format "+lexicalName));
                    }
                    outputProps = new Saxon.Hej.serialize.SerializationProperties(props);
                } else {
                    outputProps = new Saxon.Hej.serialize.SerializationProperties();
                    outputProps.setProperty("method", lexicalName);
                }
            } else {
                if (((((secondArg is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(secondArg)).getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) && ((Saxon.Hej.om.NodeInfo)(secondArg)).getURI().Equals(Saxon.Hej.lib.NamespaceConstant.XSLT)) && ((Saxon.Hej.om.NodeInfo)(secondArg)).getLocalPart().Equals("output"))) {
                    props = new Saxon.Ejava.util.Properties();
                    Saxon.Hej.expr.instruct.ResultDocument.processXslOutputElement((Saxon.Hej.om.NodeInfo)(secondArg), props, context);
                    outputProps = new Saxon.Hej.serialize.SerializationProperties(props);
                } else {
                    throw new Saxon.Hej.trans.XPathException(("The second argument of saxon:serialize must either be "+"a string literal, or an xsl:output element"));
                }
            }
            System.IO.StringWriter result = new System.IO.StringWriter();
            Saxon.Hej.lib.SerializerFactory sf = context.getConfiguration().getSerializerFactory();
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            pipe.setXPathContext(context);
            Saxon.Hej.@event.Receiver @out = sf.getReceiver(new Saxon.Ejavax.xml.transform.stream.StreamResult(result), outputProps, pipe);
            @out.open();
            node.copy(@out, (Saxon.Hej.om.CopyOptions.ALL_NAMESPACES|Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS), Saxon.Hej.expr.parser.Loc.NONE);
            @out.close();
            return new Saxon.Hej.value.StringValue(result.ToString());
        }
    
    }
}