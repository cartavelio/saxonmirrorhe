using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XMLIndenter : Saxon.Hej.@event.ProxyReceiver {
        private int level = 0;
        private bool sameline = false;
        private bool afterStartTag = false;
        private bool afterEndTag = true;
        private Saxon.Hej.@event.Event.Text pendingWhitespace = null;
        private int line = 0;
        private int column = 0;
        private int suppressedAtLevel = -1;
        private System.Collections.Generic.ISet<Saxon.Hej.om.NodeName> suppressedElements = null;
        private readonly Saxon.Hej.serialize.XMLEmitter emitter;
        public XMLIndenter(Saxon.Hej.serialize.XMLEmitter next) : base (next) {
            emitter = next;
        }
        public virtual void setOutputProperties(Saxon.Ejava.util.Properties props) {
            string omit = props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION);
            afterEndTag = (((omit == null) || !("yes".Equals(Saxon.Hej.value.Whitespace.trim(omit)))) || (props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM) != null));
            string s = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION);
            if ((s == null)) {
                s = props.getProperty("{http://saxon.sf.net/}suppress-indentation");
            }
            if ((s != null)) {
                suppressedElements = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeName>(8);
                Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(s, " \t\r\n");
                while (st.hasMoreTokens()) {
                    string eqName = st.nextToken();
                    suppressedElements.Add(Saxon.Hej.om.FingerprintedQName.fromEQName(eqName));
                }
            }
        }
        public override void open() {
            emitter.open();
        }
        public override void startElement(Saxon.Hej.om.NodeName nameCode, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((afterStartTag || afterEndTag)) {
                if (isDoubleSpaced(nameCode)) {
                    nextReceiver.characters(Saxon.Hej.str.BMPString.of("\n"), location, Saxon.Hej.@event.ReceiverOption.NONE);
                    line = 0;
                    column = 0;
                }
                indent();
            } else {
                flushPendingWhitespace();
            }
            level++;
            if ((suppressedAtLevel<0)) {
                string xmlSpace = attributes.getValue(Saxon.Hej.lib.NamespaceConstant.XML, "space");
                if (((xmlSpace != null) && xmlSpace.Trim().Equals("preserve"))) {
                    suppressedAtLevel = level;
                }
            }
            sameline = true;
            afterStartTag = true;
            afterEndTag = false;
            line = 0;
            if ((((suppressedElements != null) && (suppressedAtLevel == -1)) && suppressedElements.Contains(nameCode))) {
                suppressedAtLevel = level;
            }
            if ((((((type != Saxon.Hej.type.AnyType.getInstance()) && (type != Saxon.Hej.type.Untyped.getInstance())) && (suppressedAtLevel<0)) && type.isComplexType()) && ((Saxon.Hej.type.ComplexType)(type)).isMixedContent())) {
                suppressedAtLevel = level;
            }
            if ((suppressedAtLevel<0)) {
                int len = 0;
                foreach (Saxon.Hej.om.NamespaceBindingSet nbs in namespaces) {
                    foreach (Saxon.Hej.om.NamespaceBinding binding in nbs) {
                        string prefix = binding.getPrefix();
                        if (prefix.Length==0) {
                            len += (9+binding.getURI().Length);
                        } else {
                            len += ((prefix.Length+10)+binding.getURI().Length);
                        }
                    }
                }
                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    Saxon.Hej.om.NodeName name = att.getNodeName();
                    string prefix = name.getPrefix();
                    len += (((name.getLocalPart().Length+att.getValue().Length)+4)+((prefix.Length==0 ? 4 : (prefix.Length+5))));
                }
                if ((len>getLineLength())) {
                    int indent = ((((level-1))*getIndentation())+3);
                    emitter.setIndentForNextAttribute(indent);
                }
            }
            nextReceiver.startElement(nameCode, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            level--;
            if ((afterEndTag && !(sameline))) {
                indent();
            } else {
                flushPendingWhitespace();
            }
            emitter.endElement();
            sameline = false;
            afterEndTag = true;
            afterStartTag = false;
            line = 0;
            if ((level == ((suppressedAtLevel-1)))) {
                suppressedAtLevel = -1;
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (afterEndTag) {
                indent();
            } else {
                flushPendingWhitespace();
            }
            emitter.processingInstruction(target, data, locationId, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((suppressedAtLevel<0) && Saxon.Hej.value.Whitespace.isAllWhite(chars))) {
                pendingWhitespace = new Saxon.Hej.@event.Event.Text(chars, locationId, properties);
            } else {
                flushPendingWhitespace();
                Saxon.Hej.z.IntIterator iter = chars.codePoints();
                while (iter.hasNext()) {
                    int c = iter.next();
                    if ((c == '\n')) {
                        sameline = false;
                        line++;
                        column = 0;
                    }
                    column++;
                }
                emitter.characters(chars, locationId, properties);
                afterStartTag = false;
                afterEndTag = false;
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (afterEndTag) {
                indent();
            } else {
                flushPendingWhitespace();
            }
            emitter.comment(chars, locationId, properties);
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        private void indent() {
            if ((suppressedAtLevel>=0)) {
                flushPendingWhitespace();
                return ;
            }
            pendingWhitespace = null;
            int spaces = (level*getIndentation());
            if ((line>0)) {
                spaces -= column;
                if ((spaces<=0)) {
                    return ;
                }
            }
            emitter.characters(Saxon.Hej.str.CompressedWhitespace.makeIndent(((line == 0) ? 1 : 0), spaces), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS);
            sameline = false;
        }
        private void flushPendingWhitespace() {
            if ((pendingWhitespace != null)) {
                pendingWhitespace.replay(nextReceiver);
                pendingWhitespace = null;
            }
        }
        public override void endDocument() {
            if (afterEndTag) {
                emitter.characters(Saxon.Hej.str.BMPString.of("\n"), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            base.endDocument();
        }
        protected internal virtual int getIndentation() {
            return 3;
        }
        protected internal virtual bool isDoubleSpaced(Saxon.Hej.om.NodeName name) {
            return false;
        }
        protected internal virtual int getLineLength() {
            return 80;
        }
    
    }
}