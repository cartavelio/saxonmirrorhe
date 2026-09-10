using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class HTMLIndenter : Saxon.Hej.@event.ProxyReceiver {

        static HTMLIndenter() {
            Saxon.Ejava.util.Collections.addAll(inlineTable, inlineTags);
            Saxon.Ejava.util.Collections.addAll(formattedTable, formattedTags);
        }
        readonly private static string[] formattedTags = {"pre", "script", "style", "textarea", "title", "xmp"};
        readonly private static string[] inlineTags = {"a", "abbr", "acronym", "applet", "area", "audio", "b", "basefont", "bdi", "bdo", "big", "br", "button", "canvas", "cite", "code", "data", "datalist", "del", "dfn", "em", "embed", "font", "i", "iframe", "img", "input", "ins", "kbd", "label", "map", "mark", "math", "meter", "noscript", "object", "output", "picture", "progress", "q", "ruby", "s", "samp", "script", "select", "small", "span", "strike", "strong", "sub", "sup", "svg", "template", "textarea", "time", "tt", "u", "var", "video", "wbr"};
        readonly private static System.Collections.Generic.ISet<string> inlineTable = new System.Collections.Generic.HashSet<string>(70);
        readonly private static System.Collections.Generic.ISet<string> formattedTable = new System.Collections.Generic.HashSet<string>(10);
        protected internal char[] indentChars = {'\n', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '};
        private const int IS_INLINE = 1;
        private const int IS_FORMATTED = 2;
        private const int IS_SUPPRESSED = 4;
        private string method;
        private int level = 0;
        private bool sameLine = false;
        private bool inFormattedTag = false;
        private bool afterInline = false;
        private bool afterEndElement = false;
        private int[] propertyStack = new int[20];
        private System.Collections.Generic.ISet<string> suppressed = null;
        public HTMLIndenter(Saxon.Hej.@event.Receiver next, string method) : base (next) {
        }
        public virtual void setOutputProperties(Saxon.Ejava.util.Properties props) {
            string s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION);
            if ((s != null)) {
                suppressed = new System.Collections.Generic.HashSet<string>(8);
                Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(s, " \t\r\n");
                while (st.hasMoreTokens()) {
                    string eqName = st.nextToken();
                    suppressed.Add(Saxon.Hej.om.FingerprintedQName.fromEQName(eqName).getLocalPart().ToLower());
                }
            }
        }
        public int classifyTag(Saxon.Hej.om.NodeName name) {
            int r = 0;
            if (inlineTable.Contains(name.getLocalPart().ToLower())) {
                r |= IS_INLINE;
            }
            if (formattedTable.Contains(name.getLocalPart().ToLower())) {
                r |= IS_FORMATTED;
            }
            if (((suppressed != null) && suppressed.Contains(name.getLocalPart().ToLower()))) {
                r |= IS_SUPPRESSED;
            }
            return r;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            int withinSuppressed = ((level == 0) ? 0 : ((propertyStack[(level-1)]&IS_SUPPRESSED)));
            int tagProps = (classifyTag(elemName)|withinSuppressed);
            if ((level>=propertyStack.Length)) {
                propertyStack = Saxon.Ejava.util.Arrays.copyOf(propertyStack, (level*2));
            }
            propertyStack[level] = tagProps;
            bool inlineTag = (((tagProps&IS_INLINE)) != 0);
            if (((((!(inlineTag) && !(inFormattedTag)) && !(afterInline)) && (withinSuppressed == 0)) && (level != 0))) {
                indent();
            }
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            inFormattedTag = (inFormattedTag || ((((tagProps&IS_FORMATTED)) != 0)));
            level++;
            sameLine = true;
            afterInline = false;
            afterEndElement = false;
        }
        public override void endElement() {
            level--;
            bool thisInline = (((propertyStack[level]&IS_INLINE)) != 0);
            bool thisFormatted = (((propertyStack[level]&IS_FORMATTED)) != 0);
            bool thisSuppressed = (((propertyStack[level]&IS_SUPPRESSED)) != 0);
            if ((((((afterEndElement && !(thisInline)) && !(thisSuppressed)) && !(afterInline)) && !(sameLine)) && !(inFormattedTag))) {
                indent();
                afterInline = false;
            } else {
                afterInline = thisInline;
            }
            nextReceiver.endElement();
            inFormattedTag = (inFormattedTag && !(thisFormatted));
            sameLine = false;
            afterEndElement = true;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            int withinSuppressed = ((level == 0) ? 0 : ((propertyStack[(level-1)]&IS_SUPPRESSED)));
            if ((((inFormattedTag || (withinSuppressed>0)) || Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS)) || Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING))) {
                nextReceiver.characters(chars, locationId, properties);
            } else {
                Saxon.Hej.str.UnicodeString t = chars.tidy();
                int lastNL = 0;
                Saxon.Hej.z.IntIterator iter = t.codePoints();
                int i = 0;
                while (iter.hasNext()) {
                    int ch = iter.next();
                    if (((ch == '\n') || ((((i-lastNL)>getLineLength()) && (ch == ' '))))) {
                        sameLine = false;
                        nextReceiver.characters(t.substring(lastNL, i), locationId, properties);
                        indent();
                        lastNL = (i+1);
                        while (((lastNL<t.length()) && (t.codePointAt(lastNL) == ' '))) {
                            lastNL++;
                        }
                    }
                    i++;
                }
                if ((lastNL<t.length())) {
                    nextReceiver.characters(t.substring(lastNL, t.length()), locationId, properties);
                }
            }
            afterInline = false;
            afterEndElement = false;
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((afterEndElement && (level != 0)) && (((propertyStack[(level-1)]&IS_INLINE)) == 0))) {
                indent();
            }
            nextReceiver.processingInstruction(target, data, locationId, properties);
            afterEndElement = false;
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((afterEndElement && (level != 0)) && (((propertyStack[(level-1)]&IS_INLINE)) == 0))) {
                indent();
            }
            nextReceiver.comment(chars, locationId, properties);
            afterEndElement = false;
        }
        protected internal virtual int getLineLength() {
            return 80;
        }
        private void indent() {
            int spaces = (level*getIndentation());
            nextReceiver.characters(Saxon.Hej.str.CompressedWhitespace.makeIndent(1, spaces), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            sameLine = false;
        }
        protected internal virtual int getIndentation() {
            return 3;
        }
    
    }
}