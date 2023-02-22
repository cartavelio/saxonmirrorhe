using System;
using System.Collections.Generic;
using System.Xml;
using com.saxonica.dotnetdom;
using net.sf.saxon.functions;
using net.sf.saxon.om;
using net.sf.saxon.pattern;
using net.sf.saxon.s9api;

namespace net.sf.saxon.dotnet {
    public abstract class DotNetObjectModel : net.sf.saxon.lib.ExternalObjectModel {
        public string getDocumentClassName() {
            return "cli.System.Xml.XmlDocument";
        }
        public string getIdentifyingURI() {
            return net.sf.saxon.lib.NamespaceConstant.OBJECT_MODEL_DOT_NET_DOM;
        }
        public net.sf.saxon.expr.PJConverter getPJConverter(System.Type targetClass) {
            if (isRecognizedNodeClass(targetClass)) {
                return new Inner_PJConverter_d978e163(this);
            } else {
                return null;
            }
        }
        public net.sf.saxon.expr.JPConverter getJPConverter(System.Type sourceClass, net.sf.saxon.Configuration config) {
            if (isRecognizedNodeClass(sourceClass)) {
                return new Inner_JPConverter_d978e331(this);
            } else {
                return null;
            }
        }
        public net.sf.saxon.expr.PJConverter getNodeListCreator(System.Object node) {
            return null;
        }
        public abstract bool isXdmValue(System.Object @object);
        public abstract bool isXdmAtomicValueType(Type type);
        public abstract bool isXdmValueType(Type type);
        public abstract GroundedValue unwrapXdmValue(System.Object @object);
        public abstract System.Object wrapAsXdmValue(Sequence value);
        public abstract bool isXmlNodeType(Type type);
        public Sequence convertObjectToXPathValue(System.Object @object, net.sf.saxon.Configuration config) {
            if ((@object is XmlNode)) {
                XmlNode node = (XmlNode)@object;
                DotNetDocumentWrapper dw = new DotNetDocumentWrapper(node.OwnerDocument, node.BaseURI, config);
                return dw.wrap(node);
            } else {
                if (isXdmValue(@object)) {
                    return unwrapXdmValue(@object);
                } else {
                    return null;
                }
            }
        }
        public System.Object convertXPathValueToObject(Sequence value, Type targetClass) {
            if ((isXmlNodeType(targetClass)&&(value is DotNetNodeWrapper))) {
                return ((DotNetNodeWrapper)value).getRealNode();
            } else {
                if (isXdmAtomicValueType(targetClass)) {
                    SequenceIterator atomIterator = net.sf.saxon.expr.Atomizer.getAtomizingIterator(value.iterate(), false);
                    GroundedValue extent = atomIterator.materialize();
                    if ((extent.getLength()==0)) {
                        return null;
                    } else {
                        if ((extent is net.sf.saxon.value.AtomicValue)) {
                            return wrapAsXdmValue(extent);
                        } else {
                            throw new net.sf.saxon.trans.XPathException((("Calling external method in "+targetClass)+": supplied value is a sequence of more than one item"));
                        }
                    }
                } else {
                    if (isXdmValueType(targetClass)) {
                        return wrapAsXdmValue(value);
                    }
                }
            }
            return null;
        }
        public net.sf.saxon.@event.Receiver getDocumentBuilder(javax.xml.transform.Result result) {
            return null;
        }
        public bool isRecognizedNode(System.Object @object) {
            return (@object is XmlNode);
        }
        private bool isRecognizedNodeClass(System.Type nodeClass) {
            return typeof(XmlNode).IsAssignableFrom(nodeClass);
        }
        public bool sendSource(javax.xml.transform.Source source, net.sf.saxon.@event.Receiver receiver) {
            return false;
        }
        public NodeInfo unravel(javax.xml.transform.Source source, net.sf.saxon.Configuration config) {
            return null;
        }
        public TreeInfo wrapDocument(System.Object node, string baseURI, net.sf.saxon.Configuration config) {
            if ((node is XmlDocument)) {
                return new DotNetDocumentWrapper((XmlDocument)node, baseURI, config);
            } else {
                return null;
            }
        }
        public NodeInfo wrapNode(TreeInfo document, System.Object node) {
            if (((document is DotNetDocumentWrapper)&&(node is XmlNode))) {
                return ((DotNetDocumentWrapper)document).wrap((XmlNode)node);
            } else {
                return null;
            }
        }
    
        private class Inner_PJConverter_d978e163 : net.sf.saxon.expr.PJConverter {
            private readonly DotNetObjectModel outerInstance;
            public Inner_PJConverter_d978e163(DotNetObjectModel outerInstance) {
                this.outerInstance = outerInstance;
            }
            public System.Object convert(Sequence value, System.Type targetClass, net.sf.saxon.expr.XPathContext context) {
                if ((/*localparam*/ value is TreatFn.ZeroOrOne)) {
                    NodeInfo node = (NodeInfo)(()((TreatFn.ZeroOrOne)/*localparam*/ value).head());
                    if ((/*localvar*/ node is net.sf.saxon.tree.wrapper.VirtualNode)) {
                        System.Object u = ((net.sf.saxon.tree.wrapper.VirtualNode)/*localvar*/ node).getRealNode();
                        if (/*localparam*/ targetClass.IsAssignableFrom(/*localvar*/ u.GetType())) {
                            return /*localvar*/ u;
                        }
                    }
                } else {
                    if ((/*localparam*/ value is net.sf.saxon.tree.wrapper.VirtualNode)) {
                        System.Object u = ((net.sf.saxon.tree.wrapper.VirtualNode)/*localparam*/ value).getRealNode();
                        if (/*localparam*/ targetClass.IsAssignableFrom(/*localvar*/ u.GetType())) {
                            return /*localvar*/ u;
                        }
                    }
                }
                return null;
            }
        }

        private class Inner_JPConverter_d978e331 : net.sf.saxon.expr.JPConverter {
            private readonly DotNetObjectModel outerInstance;
            public Inner_JPConverter_d978e331(DotNetObjectModel outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override GroundedValue convert(System.Object @object, net.sf.saxon.expr.XPathContext context) {
                return outerInstance.unwrapXdmValue(/*localparam*/ object);
            }
            public override net.sf.saxon.type.ItemType getItemType() {
                return AnyNodeTest.getInstance();
            }
        }
}
}