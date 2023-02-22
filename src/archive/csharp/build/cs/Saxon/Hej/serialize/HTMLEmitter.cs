using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public abstract class HTMLEmitter : Saxon.Hej.serialize.XMLEmitter {

        static HTMLEmitter() {
            setBooleanAttribute("*", "hidden");
            setBooleanAttribute("area", "nohref");
            setBooleanAttribute("audio", "autoplay");
            setBooleanAttribute("audio", "controls");
            setBooleanAttribute("audio", "loop");
            setBooleanAttribute("audio", "muted");
            setBooleanAttribute("button", "disabled");
            setBooleanAttribute("button", "autofocus");
            setBooleanAttribute("button", "formnovalidate");
            setBooleanAttribute("details", "open");
            setBooleanAttribute("dialog", "open");
            setBooleanAttribute("dir", "compact");
            setBooleanAttribute("dl", "compact");
            setBooleanAttribute("fieldset", "disabled");
            setBooleanAttribute("form", "novalidate");
            setBooleanAttribute("frame", "noresize");
            setBooleanAttribute("hr", "noshade");
            setBooleanAttribute("img", "ismap");
            setBooleanAttribute("input", "checked");
            setBooleanAttribute("input", "disabled");
            setBooleanAttribute("input", "multiple");
            setBooleanAttribute("input", "readonly");
            setBooleanAttribute("input", "required");
            setBooleanAttribute("input", "autofocus");
            setBooleanAttribute("input", "formnovalidate");
            setBooleanAttribute("iframe", "seamless");
            setBooleanAttribute("keygen", "autofocus");
            setBooleanAttribute("keygen", "disabled");
            setBooleanAttribute("menu", "compact");
            setBooleanAttribute("object", "declare");
            setBooleanAttribute("object", "typemustmatch");
            setBooleanAttribute("ol", "compact");
            setBooleanAttribute("ol", "reversed");
            setBooleanAttribute("optgroup", "disabled");
            setBooleanAttribute("option", "selected");
            setBooleanAttribute("option", "disabled");
            setBooleanAttribute("script", "defer");
            setBooleanAttribute("script", "async");
            setBooleanAttribute("select", "multiple");
            setBooleanAttribute("select", "disabled");
            setBooleanAttribute("select", "autofocus");
            setBooleanAttribute("select", "required");
            setBooleanAttribute("style", "scoped");
            setBooleanAttribute("td", "nowrap");
            setBooleanAttribute("textarea", "disabled");
            setBooleanAttribute("textarea", "readonly");
            setBooleanAttribute("textarea", "autofocus");
            setBooleanAttribute("textarea", "required");
            setBooleanAttribute("th", "nowrap");
            setBooleanAttribute("track", "default");
            setBooleanAttribute("ul", "compact");
            setBooleanAttribute("video", "autoplay");
            setBooleanAttribute("video", "controls");
            setBooleanAttribute("video", "loop");
            setBooleanAttribute("video", "muted");
        }
        private const int REP_NATIVE = 0;
        private const int REP_ENTITY = 1;
        private const int REP_DECIMAL = 2;
        private const int REP_HEX = 3;
        private readonly int nonASCIIRepresentation = REP_NATIVE;
        private readonly int excludedRepresentation = REP_ENTITY;
        private int inScript;
        protected internal int version = 5;
        private string parentElement;
        private string uri;
        private bool escapeNonAscii = false;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NodeName> nodeNameStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NodeName>();
        private static int representationCode(string rep) {
            rep = rep.ToLower();
            switch (rep) {
              case "native":
                return REP_NATIVE;
              case "entity":
                return REP_ENTITY;
              case "decimal":
                return REP_DECIMAL;
              case "hex":
                return REP_HEX;
              default:
                return REP_ENTITY;
            }
        }
        static Saxon.Hej.serialize.HTMLTagHashSet emptyTags = new Saxon.Hej.serialize.HTMLTagHashSet(31);
        protected internal static void setEmptyTag(string tag) {
            emptyTags.add(tag);
        }
        protected internal static bool isEmptyTag(string tag) {
            return emptyTags.contains(tag);
        }
        private static readonly Saxon.Hej.serialize.HTMLTagHashSet booleanAttributes = new Saxon.Hej.serialize.HTMLTagHashSet(43);
        private static readonly Saxon.Hej.serialize.HTMLTagHashSet booleanCombinations = new Saxon.Hej.serialize.HTMLTagHashSet(57);
        private static void setBooleanAttribute(string element, string attribute) {
            booleanAttributes.add(attribute);
            booleanCombinations.add(((element+'+')+attribute));
        }
        private static bool isBooleanAttribute(string element, string attribute, string value) {
            return ((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(attribute, value) && booleanAttributes.contains(attribute)) && ((booleanCombinations.contains(((element+'+')+attribute)) || booleanCombinations.contains(("*+"+attribute)))));
        }
        public HTMLEmitter()  {
        }
        public override void setEscapeNonAscii(bool escape) {
            escapeNonAscii = escape;
        }
        protected internal abstract bool isHTMLElement(Saxon.Hej.om.NodeName name);
        public override void open() {
        }
        protected internal override void openDocument() {
            System.Diagnostics.Debug.Assert((writer != null));
            if (started) {
                return ;
            }
            string byteOrderMark = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.BYTE_ORDER_MARK);
            if (("yes".Equals(byteOrderMark) && Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-8", outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING)))) {
                try {
                    writer.writeCodePoint(0xFEFF);
                } catch (System.IO.IOException err) {
                }
            }
            if ("yes".Equals(outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.SINGLE_QUOTES))) {
                delimiter = '\'';
                attSpecials = specialInAttSingle;
            }
            inScript = -1000000;
        }
        protected internal override void writeDocType(Saxon.Hej.om.NodeName name, string displayName, string systemId, string publicId) {
            base.writeDocType(name, displayName, systemId, publicId);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            uri = elemName.getURI();
            base.startElement(elemName, type, attributes, namespaces, location, properties);
            parentElement = elementStack.Peek();
            if ((isHTMLElement(elemName) && ((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(parentElement, "script") || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(parentElement, "style"))))) {
                inScript = 0;
            }
            inScript++;
            nodeNameStack.Push(elemName);
        }
        public void startContentOLD() {
            closeStartTag();
        }
        protected internal override void writeAttribute(Saxon.Hej.om.NodeName elCode, string attname, string value, int properties) {
            try {
                if (isHTMLElement(elCode)) {
                    if (isBooleanAttribute(elCode.getLocalPart(), attname, value)) {
                        writer.write(attname);
                        return ;
                    }
                }
                if ((inScript>0)) {
                    properties |= Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING;
                }
                base.writeAttribute(elCode, attname, value, properties);
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        protected internal override void writeEscape(Saxon.Hej.str.UnicodeString chars, bool inAttribute) {
            int segstart = 0;
            bool[] specialChars = (inAttribute ? attSpecials : specialInText);
            if ((chars is Saxon.Hej.str.WhitespaceString)) {
                ((Saxon.Hej.str.WhitespaceString)(chars)).writeEscape(specialChars, writer);
                return ;
            }
            bool disabled = false;
            chars = chars.tidy();
            int[] codePoints = Saxon.Hej.str.StringTool.expand(chars);
            while ((segstart<codePoints.Length)) {
                int i = segstart;
                if (escapeNonAscii) {
                    int c;
                    while ((((i<codePoints.Length) && ((c = codePoints[i])<127)) && !(specialChars[c]))) {
                        i++;
                    }
                } else {
                    int c;
                    while (((i<codePoints.Length) && ((((c = codePoints[i])<127) ? !(specialChars[c]) : ((characterSet.inCharset(c) && (c>160))))))) {
                        i++;
                    }
                }
                if ((i == codePoints.Length)) {
                    writer.write(chars.substring(segstart));
                    return ;
                }
                if ((i>segstart)) {
                    writer.write(chars.substring(segstart, i));
                }
                int ch = codePoints[i];
                if ((ch == 0)) {
                    disabled = !(disabled);
                } else {
                    if (disabled) {
                        writeCodePoint(ch);
                    } else {
                        if ((ch<=127)) {
                            if (inAttribute) {
                                if ((ch == '<')) {
                                    writer.writeCodePoint('<');
                                } else {
                                    if ((ch == '>')) {
                                        writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_GT);
                                    } else {
                                        if ((ch == '&')) {
                                            if ((((i+1)<codePoints.Length) && (codePoints[(i+1)] == '{'))) {
                                                writer.writeCodePoint('&');
                                            } else {
                                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_AMP);
                                            }
                                        } else {
                                            if ((ch == '\"')) {
                                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_QUOT);
                                            } else {
                                                if ((ch == '\'')) {
                                                    writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_APOS);
                                                } else {
                                                    if ((ch == '\n')) {
                                                        writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_NL);
                                                    } else {
                                                        if ((ch == '\t')) {
                                                            writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_TAB);
                                                        } else {
                                                            if ((ch == '\r')) {
                                                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_CR);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } else {
                                if ((ch == '<')) {
                                    writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_LT);
                                } else {
                                    if ((ch == '>')) {
                                        writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_GT);
                                    } else {
                                        if ((ch == '&')) {
                                            writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_AMP);
                                        } else {
                                            if ((ch == '\r')) {
                                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_CR);
                                            }
                                        }
                                    }
                                }
                            }
                        } else {
                            if ((ch<160)) {
                                if (rejectControlCharacters()) {
                                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Illegal HTML character: decimal "+ch));
                                    err.setErrorCode("SERE0014");
                                    throw err;
                                } else {
                                    characterReferenceGenerator.outputCharacterReference(ch, writer);
                                }
                            } else {
                                if ((ch == 160)) {
                                    writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_NBSP);
                                } else {
                                    if ((((ch>65535) || escapeNonAscii) || !(characterSet.inCharset(ch)))) {
                                        characterReferenceGenerator.outputCharacterReference(ch, writer);
                                    } else {
                                        writer.writeCodePoint(ch);
                                    }
                                }
                            }
                        }
                    }
                }
                segstart = ++i;
            }
        }
        protected internal abstract bool rejectControlCharacters();
        protected internal override void writeEmptyElementTagCloser(string displayName, Saxon.Hej.om.NodeName nameCode) {
            if (isHTMLElement(nameCode)) {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_MIDDLE);
                writer.write(displayName);
                writer.writeCodePoint('>');
            } else {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_END);
            }
        }
        public override void endElement() {
            Saxon.Hej.om.NodeName nodeName = nodeNameStack.Pop();
            string name = elementStack.Peek();
            inScript--;
            if ((inScript == 0)) {
                inScript = -1000000;
            }
            if ((isEmptyTag(name) && isHTMLElement(nodeName))) {
                if (openStartTag) {
                    closeStartTag();
                }
                elementStack.Pop();
            } else {
                base.endElement();
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((inScript>0)) {
                properties |= Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING;
            }
            base.characters(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(started)) {
                openDocument();
            }
            Saxon.Hej.str.UnicodeString t = data.tidy();
            if ((t.indexOf('>')>=0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("A processing instruction in HTML must not contain a > character");
                err.setErrorCode("SERE0015");
                throw err;
            }
            try {
                if (openStartTag) {
                    closeStartTag();
                }
                writer.writeAscii(Saxon.Hej.str.StringConstants.PI_START);
                writer.write(target);
                writer.writeCodePoint(' ');
                writer.write(t);
                writer.writeCodePoint('>');
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
    
    }
}