using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class CheckSumFilter : Saxon.Hej.@event.ProxyReceiver {
        private const bool DEBUG = false;
        private int checksum = 0;
        private int sequence = 0;
        private bool checkExistingChecksum = false;
        private bool checksumCorrect = false;
        private bool checksumFound = false;
        public const string SIGMA = "\u03A3";
        public CheckSumFilter(Saxon.Hej.@event.Receiver nextReceiver) : base (nextReceiver) {
        }
        public void setCheckExistingChecksum(bool check) {
            this.checkExistingChecksum = check;
        }
        private static void trace(string message) {
            
        }
        public override void startDocument(int properties) {
            trace("CHECKSUM - START DOC");
            base.startDocument(properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            checksum ^= hash(item.ToString(), sequence++);
            trace(("After append: "+Saxon.Impl.Helpers.Utils.intToHexString(checksum)));
            base.append(item, locationId, copyNamespaces);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(Saxon.Hej.value.Whitespace.isAllWhite(chars))) {
                checksum ^= hash(chars.ToString(), sequence++);
                trace(((("After characters "+chars)+": ")+Saxon.Impl.Helpers.Utils.intToHexString(checksum)));
            }
            base.characters(chars, locationId, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            checksum ^= hash(elemName, sequence++);
            trace(((("After startElement "+elemName.getDisplayName())+": ")+checksum));
            checksumCorrect = false;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                checksum ^= hash(att.getNodeName(), sequence);
                checksum ^= hash(att.getValue(), sequence);
            }
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            checksum ^= 1;
            trace(("After endElement: "+checksum));
            base.endElement();
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (target.Equals(SIGMA)) {
                checksumFound = true;
                if (checkExistingChecksum) {
                    try {
                        int found = (int)(System.Int64.Parse(("0"+data), System.Globalization.NumberStyles.HexNumber));
                        checksumCorrect = (found == checksum);
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        checksumCorrect = false;
                    }
                }
            }
            base.processingInstruction(target, data, locationId, properties);
        }
        public bool isChecksumFound() {
            return checksumFound;
        }
        public int getChecksum() {
            return checksum;
        }
        public bool isChecksumCorrect() {
            return (checksumCorrect || "skip".Equals(Saxon.Ejava.lang.System.getProperty("saxon-checksum")));
        }
        private int hash(string s, int sequence) {
            int h = (sequence<<8);
            for (int i = 0; (i<s.Length); i++) {
                h = (((h<<1))+s[i]);
            }
            return h;
        }
        private int hash(Saxon.Hej.om.NodeName n, int sequence) {
            return (hash(n.getLocalPart(), sequence)^hash(n.getURI(), sequence));
        }
    
    }
}