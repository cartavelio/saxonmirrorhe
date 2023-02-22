using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class TransformFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            try {
                Saxon.Hej.value.AnyExternalObject templatesObject = (Saxon.Hej.value.AnyExternalObject)(arguments[0].head());
                if ((templatesObject == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Hej.PreparedStylesheet pss = (Saxon.Hej.PreparedStylesheet)(templatesObject.getWrappedObject());
                Saxon.Hej.om.NodeInfo source = (Saxon.Hej.om.NodeInfo)(arguments[1].head());
                if ((source == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Hej.trans.XsltController transformer = pss.newController();
                Saxon.Hej.expr.instruct.GlobalParameterSet @params = new Saxon.Hej.expr.instruct.GlobalParameterSet();
                if ((arguments.Length == 3)) {
                    Saxon.Hej.om.SequenceIterator iter = arguments[2].iterate();
                    Saxon.Hej.om.Item param;
                    while (((param = iter.next()) != null)) {
                        if ((param is Saxon.Hej.om.NodeInfo)) {
                            switch (((Saxon.Hej.om.NodeInfo)(param)).getNodeKind()) {
                              case Saxon.Hej.type.Type.ELEMENT:
                              case Saxon.Hej.type.Type.ATTRIBUTE:
                                setTransformerParameter(param, @params);
                                break ;
                              case Saxon.Hej.type.Type.DOCUMENT:
                                foreach (Saxon.Hej.om.NodeInfo kid in ((Saxon.Hej.om.NodeInfo)(param)).children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                                    setTransformerParameter(kid, @params);
                                }
                                break ;
                              default:
                                throw new Saxon.Hej.trans.XPathException("Parameters passed to saxon:transform() must be element, attribute, or document nodes");
                            }
                        } else {
                            if ((param is Saxon.Hej.ma.map.MapItem)) {
                                foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(param)).keyValuePairs()) {
                                    Saxon.Hej.value.AtomicValue key = pair.key;
                                    Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.om.Sequence)(pair.value)).materialize();
                                    if ((key is Saxon.Hej.value.StringValue)) {
                                        @params.put(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, key.getStringValue()), val);
                                    } else {
                                        if ((key is Saxon.Hej.value.QNameValue)) {
                                            @params.put(((Saxon.Hej.value.QNameValue)(key)).getStructuredQName(), val);
                                        }
                                    }
                                }
                            } else {
                                throw new Saxon.Hej.trans.XPathException("Parameters passed to saxon:transform() must be nodes");
                            }
                        }
                    }
                }
                Saxon.Hej.@event.Builder builder = context.getController().makeBuilder();
                if ((builder is Saxon.Hej.tree.tiny.TinyBuilder)) {
                    ((Saxon.Hej.tree.tiny.TinyBuilder)(builder)).setStatistics(context.getConfiguration().getTreeStatistics().RESULT_TREE_STATISTICS);
                }
                transformer.setApplyFunctionConversionRulesToExternalVariables(true);
                transformer.initializeController(@params);
                transformer.applyTemplates(source, new Saxon.Hej.@event.TreeReceiver(builder));
                Saxon.Hej.om.NodeInfo result = builder.getCurrentRoot();
                if ((result == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                return result;
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        private static void setTransformerParameter(Saxon.Hej.om.Item param, Saxon.Hej.expr.instruct.GlobalParameterSet @params) {
            Saxon.Hej.om.NodeName name = Saxon.Hej.om.NameOfNode.makeName((Saxon.Hej.om.NodeInfo)(param));
            Saxon.Hej.om.AtomicSequence val = param.atomize();
            @params.put(name.getStructuredQName(), val);
        }
    
    }
}