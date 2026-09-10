using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XHTMLPrefixRemover : Saxon.Hej.@event.ProxyReceiver {
        public XHTMLPrefixRemover(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        private bool isSpecial(Saxon.Hej.om.NamespaceUri uri) {
            return ((uri.Equals(Saxon.Hej.om.NamespaceUri.XHTML) || uri.Equals(Saxon.Hej.om.NamespaceUri.SVG)) || uri.Equals(Saxon.Hej.om.NamespaceUri.MATHML));
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            foreach (Saxon.Hej.om.NamespaceBinding ns in namespaces) {
                if (isSpecial(ns.getNamespaceUri())) {
                    namespaces = namespaces.remove(ns.getPrefix());
                }
            }
            if (isSpecial(elemName.getNamespaceUri())) {
                Saxon.Hej.om.NamespaceUri uri = elemName.getNamespaceUri();
                if (!(elemName.getPrefix().Length==0)) {
                    elemName = new Saxon.Hej.om.FingerprintedQName("", uri, elemName.getLocalPart());
                }
                namespaces = namespaces.put("", uri);
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                if (isSpecial(att.getNodeName().getNamespaceUri())) {
                    namespaces = namespaces.put(att.getNodeName().getPrefix(), att.getNodeName().getNamespaceUri());
                }
            }
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
    
    }
}