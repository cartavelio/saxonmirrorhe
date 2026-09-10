using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class HTMLURIEscaper : Saxon.Hej.@event.ProxyReceiver {

        static HTMLURIEscaper() {
            setUrlAttribute("form", "action");
            setUrlAttribute("object", "archive");
            setUrlAttribute("body", "background");
            setUrlAttribute("q", "cite");
            setUrlAttribute("blockquote", "cite");
            setUrlAttribute("del", "cite");
            setUrlAttribute("ins", "cite");
            setUrlAttribute("object", "classid");
            setUrlAttribute("object", "codebase");
            setUrlAttribute("applet", "codebase");
            setUrlAttribute("object", "data");
            setUrlAttribute("button", "datasrc");
            setUrlAttribute("div", "datasrc");
            setUrlAttribute("input", "datasrc");
            setUrlAttribute("object", "datasrc");
            setUrlAttribute("select", "datasrc");
            setUrlAttribute("span", "datasrc");
            setUrlAttribute("table", "datasrc");
            setUrlAttribute("textarea", "datasrc");
            setUrlAttribute("script", "for");
            setUrlAttribute("a", "href");
            setUrlAttribute("a", "name");
            setUrlAttribute("area", "href");
            setUrlAttribute("link", "href");
            setUrlAttribute("base", "href");
            setUrlAttribute("img", "longdesc");
            setUrlAttribute("frame", "longdesc");
            setUrlAttribute("iframe", "longdesc");
            setUrlAttribute("head", "profile");
            setUrlAttribute("script", "src");
            setUrlAttribute("input", "src");
            setUrlAttribute("frame", "src");
            setUrlAttribute("iframe", "src");
            setUrlAttribute("img", "src");
            setUrlAttribute("img", "usemap");
            setUrlAttribute("input", "usemap");
            setUrlAttribute("object", "usemap");
        }
        private static readonly Saxon.Hej.serialize.HTMLTagHashSet urlAttributes = new Saxon.Hej.serialize.HTMLTagHashSet(47);
        private static readonly Saxon.Hej.serialize.HTMLTagHashSet urlCombinations = new Saxon.Hej.serialize.HTMLTagHashSet(101);
        private static void setUrlAttribute(string element, string attribute) {
            urlAttributes.add(attribute);
            urlCombinations.add(((element+'+')+attribute));
        }
        public bool isUrlAttribute(Saxon.Hej.om.NodeName element, Saxon.Hej.om.NodeName attribute) {
            if ((pool == null)) {
                pool = getNamePool();
            }
            string attributeName = attribute.getDisplayName();
            if (!(urlAttributes.contains(attributeName))) {
                return false;
            }
            string elementName = element.getDisplayName();
            return urlCombinations.contains(((elementName+'+')+attributeName));
        }
        protected internal Saxon.Hej.om.NodeName currentElement;
        protected internal bool escapeURIAttributes = true;
        protected internal Saxon.Hej.om.NamePool pool;
        public HTMLURIEscaper(Saxon.Hej.@event.Receiver nextReceiver) : base (nextReceiver) {
        }
        public override void startDocument(int properties) {
            nextReceiver.startDocument(properties);
            pool = getPipelineConfiguration().getConfiguration().getNamePool();
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
                                    return new Saxon.Hej.om.AttributeInfo(att.getNodeName(), att.getType(), escapeURL(Saxon.Hej.str.StringView.tidy(value), true, getConfiguration()).ToString(), att.getLocation(), (att.getProperties()|Saxon.Hej.@event.ReceiverOption.DISABLE_CHARACTER_MAPS));
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
        public static Saxon.Hej.str.UnicodeString escapeURL(Saxon.Hej.str.UnicodeString url, bool normalize, Saxon.Hej.Configuration config) {
            Saxon.Hej.z.IntIterator iter = url.codePoints();
            while (iter.hasNext()) {
                int ch = iter.next();
                if (((ch<32) || (ch>126))) {
                    if (normalize) {
                        string normalized = Saxon.Ejava.text.Normalizer.normalize(url.ToString(), Saxon.Ejava.text.Normalizer.Form.NFC);
                        return reallyEscapeURL(normalized);
                    } else {
                        return reallyEscapeURL(url.ToString());
                    }
                }
            }
            return url;
        }
        private static Saxon.Hej.str.UnicodeString reallyEscapeURL(string url) {
            Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
            string hex = "0123456789ABCDEF";
            byte[] array = new byte[4];
            Saxon.Hej.z.IntIterator iter = Saxon.Hej.str.StringTool.codePoints(url);
            while (iter.hasNext()) {
                int ch = iter.next();
                if (((ch<32) || (ch>126))) {
                    array = Saxon.Hej.serialize.charcode.UTF8CharacterSet.encode(new Saxon.Hej.z.IntSingletonIterator(ch));
                    foreach (byte value in array) {
                        int v = (((int)(value))&unchecked((int)0xff));
                        ub.append('%').append(hex[(v/16)]).append(hex[(v%16)]);
                    }
                } else {
                    ub.append(ch);
                }
            }
            return ub.toUnicodeString();
        }
    
    }
}