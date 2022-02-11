using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class MetaTagAdjuster : Saxon.Hej.@event.ProxyReceiver {
        private bool seekingHead = true;
        private int droppingMetaTags = -1;
        private bool inMetaTag = false;
        internal string encoding;
        private string mediaType;
        private int level = 0;
        private bool isXHTML = false;
        private int htmlVersion = 4;
        public MetaTagAdjuster(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            encoding = details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
            if ((encoding == null)) {
                encoding = "UTF-8";
            }
            mediaType = details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.MEDIA_TYPE);
            if ((mediaType == null)) {
                mediaType = "text/html";
            }
            string htmlVn = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.HTML_VERSION);
            if (((htmlVn == null) && !(isXHTML))) {
                htmlVn = details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION);
            }
            if (((htmlVn != null) && htmlVn.StartsWith("5"))) {
                htmlVersion = 5;
            }
        }
        public void setIsXHTML(bool xhtml) {
            isXHTML = xhtml;
        }
        private bool comparesEqual(string name1, string name2) {
            if (isXHTML) {
                return name1.Equals(name2);
            } else {
                return Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(name1, name2);
            }
        }
        private bool matchesName(Saxon.Hej.om.NodeName name, string local) {
            if (isXHTML) {
                if (!(name.getLocalPart().Equals(local))) {
                    return false;
                }
                if ((htmlVersion == 5)) {
                    return (name.hasURI("") || name.hasURI(Saxon.Hej.lib.NamespaceConstant.XHTML));
                } else {
                    return name.hasURI(Saxon.Hej.lib.NamespaceConstant.XHTML);
                }
            } else {
                return Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(name.getLocalPart(), local);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((droppingMetaTags == level)) {
                if (matchesName(elemName, "meta")) {
                    bool found = false;
                    foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                        string name = att.getNodeName().getLocalPart();
                        if (comparesEqual(name, "http-equiv")) {
                            string value = Saxon.Hej.value.Whitespace.trim(att.getValue());
                            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(value, "Content-Type")) {
                                found = true;
                                break ;
                            }
                        }
                    }
                    inMetaTag = found;
                    if (found) {
                        return ;
                    }
                }
            }
            level++;
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            if ((seekingHead && matchesName(elemName, "head"))) {
                string headPrefix = elemName.getPrefix();
                string headURI = elemName.getURI();
                Saxon.Hej.om.FingerprintedQName metaCode = new Saxon.Hej.om.FingerprintedQName(headPrefix, headURI, "meta");
                Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                atts = atts.put(new Saxon.Hej.om.AttributeInfo(new Saxon.Hej.om.NoNamespaceName("http-equiv"), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, "Content-Type", Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE));
                atts = atts.put(new Saxon.Hej.om.AttributeInfo(new Saxon.Hej.om.NoNamespaceName("content"), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, ((mediaType+"; charset=")+encoding), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE));
                nextReceiver.startElement(metaCode, Saxon.Hej.type.Untyped.getInstance(), atts, namespaces, location, Saxon.Hej.@event.ReceiverOption.NONE);
                droppingMetaTags = level;
                seekingHead = false;
                nextReceiver.endElement();
            }
        }
        public override void endElement() {
            if (inMetaTag) {
                inMetaTag = false;
            } else {
                level--;
                if ((droppingMetaTags == (level+1))) {
                    droppingMetaTags = -1;
                }
                nextReceiver.endElement();
            }
        }
    
    }
}