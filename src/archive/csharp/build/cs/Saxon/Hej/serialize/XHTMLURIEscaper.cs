using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XHTMLURIEscaper : Saxon.Hej.serialize.HTMLURIEscaper {

        static XHTMLURIEscaper() {
            setUrlAttributeX("form", "action");
            setUrlAttributeX("object", "archive");
            setUrlAttributeX("body", "background");
            setUrlAttributeX("q", "cite");
            setUrlAttributeX("blockquote", "cite");
            setUrlAttributeX("del", "cite");
            setUrlAttributeX("ins", "cite");
            setUrlAttributeX("object", "classid");
            setUrlAttributeX("object", "codebase");
            setUrlAttributeX("applet", "codebase");
            setUrlAttributeX("object", "data");
            setUrlAttributeX("button", "datasrc");
            setUrlAttributeX("div", "datasrc");
            setUrlAttributeX("input", "datasrc");
            setUrlAttributeX("object", "datasrc");
            setUrlAttributeX("select", "datasrc");
            setUrlAttributeX("span", "datasrc");
            setUrlAttributeX("table", "datasrc");
            setUrlAttributeX("textarea", "datasrc");
            setUrlAttributeX("script", "for");
            setUrlAttributeX("a", "href");
            setUrlAttributeX("a", "name");
            setUrlAttributeX("area", "href");
            setUrlAttributeX("link", "href");
            setUrlAttributeX("base", "href");
            setUrlAttributeX("img", "longdesc");
            setUrlAttributeX("frame", "longdesc");
            setUrlAttributeX("iframe", "longdesc");
            setUrlAttributeX("head", "profile");
            setUrlAttributeX("script", "src");
            setUrlAttributeX("input", "src");
            setUrlAttributeX("frame", "src");
            setUrlAttributeX("iframe", "src");
            setUrlAttributeX("img", "src");
            setUrlAttributeX("img", "usemap");
            setUrlAttributeX("input", "usemap");
            setUrlAttributeX("object", "usemap");
        }
        private static readonly System.Collections.Generic.HashSet<string> urlTable = new System.Collections.Generic.HashSet<string>(70);
        private static readonly System.Collections.Generic.HashSet<string> attTable = new System.Collections.Generic.HashSet<string>(20);
        private static void setUrlAttributeX(string element, string attribute) {
            attTable.Add(attribute);
            urlTable.Add(((element+"+")+attribute));
        }
        public XHTMLURIEscaper(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        private static bool isURLAttribute(Saxon.Hej.om.NodeName elcode, Saxon.Hej.om.NodeName atcode) {
            if (!(elcode.hasURI(Saxon.Hej.om.NamespaceUri.XHTML))) {
                return false;
            }
            if (!(atcode.hasURI(Saxon.Hej.om.NamespaceUri.NULL))) {
                return false;
            }
            string attName = atcode.getLocalPart();
            return (attTable.Contains(attName) && urlTable.Contains(((elcode.getLocalPart()+"+")+attName)));
        }
        public override void startElement(Saxon.Hej.om.NodeName nameCode, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            currentElement = nameCode;
            Saxon.Hej.om.AttributeMap atts2 = attributes;
            if (escapeURIAttributes) {
                try {
                    atts2 = attributes.apply(att => {
                        if (!(Saxon.Hej.@event.ReceiverOption.contains(att.getProperties(), Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING))) {
                            Saxon.Hej.om.NodeName attName = att.getNodeName();
                            if (isUrlAttribute(nameCode, attName)) {
                                string value = att.getValue();
                                try {
                                    string normalized = ((isAllAscii(value) ? value : Saxon.Ejava.text.Normalizer.normalize(value, Saxon.Ejava.text.Normalizer.Form.NFC)));
                                    return new Saxon.Hej.om.AttributeInfo(attName, att.getType(), escapeURL(normalized, true, getConfiguration()), att.getLocation(), (att.getProperties()|Saxon.Hej.@event.ReceiverOption.DISABLE_CHARACTER_MAPS));
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                                }
                            } else {
                                return att;
                            }
                        } else {
                            return att;
                        }
                    } );
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            }
            nextReceiver.startElement(nameCode, type, atts2, namespaces, location, properties);
        }
        private static bool isAllAscii(string value) {
            for (int i = 0; (i<value.Length); i++) {
                if ((value[i]>127)) {
                    return false;
                }
            }
            return true;
        }
    
    }
}