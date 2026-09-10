using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class CDATAFilter : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NodeName> stack = new System.Collections.Generic.Stack<Saxon.Hej.om.NodeName>();
        private System.Collections.Generic.ISet<Saxon.Hej.om.NodeName> nameList;
        private Saxon.Hej.serialize.charcode.CharacterSet characterSet;
        public CDATAFilter(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            getCdataElements(details);
            characterSet = getConfiguration().getCharacterSetFactory().getCharacterSet(details);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            flush();
            stack.Push(elemName);
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            flush();
            stack.Pop();
            nextReceiver.endElement();
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            flush();
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING))) {
                buffer.append(chars.ToString());
            } else {
                flush();
                nextReceiver.characters(chars, locationId, properties);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            flush();
            nextReceiver.comment(chars, locationId, properties);
        }
        private void flush() {
            bool cdata;
            int end = (int)(buffer.length());
            if ((end == 0)) {
                return ;
            }
            if ((stack.Count==0)) {
                cdata = false;
            } else {
                Saxon.Hej.om.NodeName top = stack.Peek();
                cdata = isCDATA(top);
            }
            if (cdata) {
                if ((getNextReceiver() is Saxon.Hej.serialize.UnicodeNormalizer)) {
                    Saxon.Hej.str.UnicodeString normal = ((Saxon.Hej.serialize.UnicodeNormalizer)(getNextReceiver())).normalize(buffer.toUnicodeString(), true);
                    buffer = new Saxon.Hej.str.UnicodeBuilder();
                    buffer.accept(normal);
                    end = (int)(buffer.length());
                }
                Saxon.Hej.str.UnicodeString bufferContent = buffer.toUnicodeString();
                int start = 0;
                int k = 0;
                while ((k<end)) {
                    int next = bufferContent.codePointAt(k);
                    if (((next != 0) && characterSet.inCharset(next))) {
                        k++;
                    } else {
                        flushCDATA(bufferContent.substring(start, k));
                        while (true) {
                            nextReceiver.characters(bufferContent.substring(k, (k+1)), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.DISABLE_CHARACTER_MAPS);
                            k++;
                            if ((k>=end)) {
                                break ;
                            }
                            next = bufferContent.codePointAt(k);
                            if (characterSet.inCharset(next)) {
                                break ;
                            }
                        }
                        start = k;
                    }
                }
                flushCDATA(bufferContent.substring(start, end));
            } else {
                nextReceiver.characters(buffer.toUnicodeString(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            buffer.clear();
        }
        private void flushCDATA(Saxon.Hej.str.UnicodeString data) {
            data = data.tidy();
            if (data.isEmpty()) {
                return ;
            }
            long len = data.length();
            int chprop = (Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING|Saxon.Hej.@event.ReceiverOption.DISABLE_CHARACTER_MAPS);
            Saxon.Hej.s9api.Location loc = Saxon.Hej.expr.parser.Loc.NONE;
            nextReceiver.characters(Saxon.Hej.str.BMPString.of("<![CDATA["), loc, chprop);
            long i = 0;
            long doneto = 0;
            while ((i<(len-2))) {
                if ((((data.codePointAt(i) == ']') && (data.codePointAt((i+1)) == ']')) && (data.codePointAt((i+2)) == '>'))) {
                    nextReceiver.characters(data.substring(doneto, (i+2)), loc, chprop);
                    nextReceiver.characters(Saxon.Hej.str.BMPString.of("]]><![CDATA["), loc, chprop);
                    doneto = (i+2);
                } else {
                    if ((data.codePointAt(i) == 0)) {
                        nextReceiver.characters(data.substring(doneto, i), loc, chprop);
                        doneto = (i+1);
                    }
                }
                i++;
            }
            nextReceiver.characters(data.substring(doneto, len), loc, chprop);
            nextReceiver.characters(Saxon.Hej.str.BMPString.of("]]>"), loc, chprop);
        }
        protected internal bool isCDATA(Saxon.Hej.om.NodeName elementName) {
            return nameList.Contains(elementName);
        }
        private void getCdataElements(Saxon.Ejava.util.Properties details) {
            bool isHTML = "html".Equals(details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD));
            bool isHTML5 = (isHTML && "5.0".Equals(details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION)));
            bool isHTML4 = (isHTML && !(isHTML5));
            string cdata = details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS);
            if ((cdata == null)) {
                nameList = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeName>(0);
                return ;
            }
            nameList = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeName>(10);
            Saxon.Ejava.util.StringTokenizer st2 = new Saxon.Ejava.util.StringTokenizer(cdata, " \t\n\r", false);
            while (st2.hasMoreTokens()) {
                string expandedName = st2.nextToken();
                Saxon.Hej.om.StructuredQName sq = Saxon.Hej.om.StructuredQName.fromClarkName(expandedName);
                string uri = sq.getURI();
                if (((!(isHTML) || ((isHTML4 && !(uri.Equals(""))))) || (((isHTML5 && !(uri.Equals(""))) && !(uri.Equals(Saxon.Hej.lib.NamespaceConstant.XHTML)))))) {
                    nameList.Add(new Saxon.Hej.om.FingerprintedQName("", sq.getURI(), sq.getLocalPart()));
                }
            }
        }
    
    }
}