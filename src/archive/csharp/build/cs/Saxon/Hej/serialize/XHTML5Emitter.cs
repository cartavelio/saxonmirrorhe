using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XHTML5Emitter : Saxon.Hej.serialize.XMLEmitter {

        static XHTML5Emitter() {
            Saxon.Ejava.util.Collections.addAll(emptyTags5, emptyTagNames5);
            Saxon.Ejava.util.Collections.addAll(html5Elements, html5ElementNames);
        }
        private static readonly string[] html5ElementNames = {"a", "abbr", "address", "area", "article", "aside", "audio", "b", "base", "bdi", "bdo", "blockquote", "body", "br", "button", "canvas", "caption", "cite", "code", "col", "colgroup", "datalist", "dd", "del", "details", "dfn", "dialog", "div", "dl", "dt", "em", "embed", "fieldset", "figcaption", "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hgroup", "hr", "html", "i", "iframe", "img", "input", "ins", "kbd", "keygen", "label", "legend", "li", "link", "map", "mark", "menu", "meta", "meter", "nav", "noscript", "object", "ol", "optgroup", "option", "output", "p", "param", "pre", "progress", "q", "rp", "rt", "ruby", "s", "samp", "script", "section", "select", "small", "source", "span", "strong", "style", "sub", "summary", "sup", "table", "tbody", "td", "textarea", "tfoot", "th", "thead", "time", "title", "tr", "track", "u", "ul", "var", "video", "wbr"};
        static System.Collections.Generic.ISet<string> html5Elements = new System.Collections.Generic.HashSet<string>(128);
        static System.Collections.Generic.ISet<string> emptyTags5 = new System.Collections.Generic.HashSet<string>(31);
        private static readonly string[] emptyTagNames5 = {"area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr"};
        private bool isRecognizedHtmlElement(Saxon.Hej.om.NodeName name) {
            return (name.hasURI(Saxon.Hej.lib.NamespaceConstant.XHTML) || (name.hasURI("") && html5Elements.Contains(name.getLocalPart().ToLower())));
        }
        protected internal override void writeDocType(Saxon.Hej.om.NodeName name, string displayName, string systemId, string publicId) {
            if ((((systemId == null) && isRecognizedHtmlElement(name)) && name.getLocalPart().ToLower().Equals("html"))) {
                try {
                    writer.writeAscii(DOCTYPE);
                    writer.write(displayName);
                    writer.writeCodePoint('>');
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            } else {
                if ((systemId != null)) {
                    base.writeDocType(name, displayName, systemId, publicId);
                }
            }
        }
        protected internal override bool writeDocTypeWithNullSystemId() {
            return true;
        }
        protected internal override void writeEmptyElementTagCloser(string displayName, Saxon.Hej.om.NodeName name) {
            if ((isRecognizedHtmlElement(name) && emptyTags5.Contains(name.getLocalPart()))) {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_END);
            } else {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_MIDDLE);
                writer.write(displayName);
                writer.writeCodePoint('>');
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((!(started) && Saxon.Hej.value.Whitespace.isAllWhite(chars))) {
            } else {
                base.characters(chars, locationId, properties);
            }
        }
    
    }
}