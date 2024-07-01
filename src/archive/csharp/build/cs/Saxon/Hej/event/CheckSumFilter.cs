using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class CheckSumFilter : Saxon.Hej.@event.ProxyReceiver {
        private const bool DEBUG = false;
        private Saxon.Impl.Helpers.DigestMaker digest = null;
        private int checksum = 0;
        private int sequence = 0;
        private bool checkExistingChecksum = false;
        private bool checksumCorrect = false;
        private bool checksumFound = false;
        private bool digestCorrect = false;
        private bool digestFound = false;
        private bool requireDigest = false;
        private bool rootElement = true;
        public const string SIGMA = "Σ";
        public const string SIGMA2 = "Σ2";
        public CheckSumFilter(Saxon.Hej.@event.Receiver nextReceiver) : base (nextReceiver) {
            rootElement = true;
            digest = new Saxon.Impl.Helpers.DigestMaker();
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
        public override void endDocument() {
            nextReceiver.endDocument();
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
            if (rootElement) {
                rootElement = false;
                bool scm_schema = ((elemName.getNamespaceUri() == Saxon.Hej.om.NamespaceUri.of("http://ns.saxonica.com/schema-component-model")) && "schema".Equals(elemName.getLocalPart()));
                string version = attributes.getValue("saxonVersion");
                requireDigest = ((version == null) && !(scm_schema));
                if ((version != null)) {
                    string minorVersion = "x";
                    int dpos = version.IndexOf('.');
                    if ((dpos>0)) {
                        minorVersion = version.Substring((dpos+1));
                        version = version.Substring(0, dpos - 0);
                    }
                    try {
                        int majorVersion = Saxon.Impl.Helpers.Utils.ParseInt(version);
                        if ((majorVersion>12)) {
                            requireDigest = true;
                        } else {
                            if ((majorVersion == 12)) {
                                requireDigest = (Saxon.Impl.Helpers.Utils.ParseInt(minorVersion)>=5);
                            }
                        }
                    } catch (Saxon.Ejava.lang.NumberFormatException e) {
                        requireDigest = true;
                    }
                }
            }
            System.Collections.Generic.Dictionary<string,Saxon.Hej.om.NodeName> namemap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.NodeName>();
            System.Collections.Generic.Dictionary<string,string> attrmap = new System.Collections.Generic.Dictionary<string, string>();
            string[] names = new string[attributes.size()];
            int index = 0;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                string key = (att.getNodeName().getLocalPart()+att.getNodeName().getNamespaceUri());
                Saxon.Impl.Helpers.MapUtils.Put(attrmap, key, att.getValue());
                Saxon.Impl.Helpers.MapUtils.Put(namemap, key, att.getNodeName());
                names[index++] = key;
            }
            Saxon.Ejava.util.Arrays.sort(names);
            foreach (string key in names) {
                Saxon.Hej.om.NodeName name = Saxon.Impl.Helpers.MapUtils.Get(namemap, key);
                string value = Saxon.Impl.Helpers.MapUtils.Get(attrmap, key);
                checksum ^= hash(name, sequence);
                checksum ^= hash(value, sequence);
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
                    if (data.ToString().Equals(getDigest())) {
                        digestFound = true;
                        digestCorrect = true;
                        checksumCorrect = true;
                    }
                }
            }
            if (target.Equals(SIGMA2)) {
                digestFound = true;
                if (checkExistingChecksum) {
                    digestCorrect = data.ToString().Equals(getDigest());
                }
            }
            base.processingInstruction(target, data, locationId, properties);
        }
        public bool isChecksumFound() {
            return checksumFound;
        }
        public bool isDigestFound() {
            return digestFound;
        }
        public int getChecksum() {
            return checksum;
        }
        public string getDigest() {
            return digest.getDigest();
        }
        public bool isChecksumCorrect() {
            if ((requireDigest && !(digestCorrect))) {
                return false;
            }
            return (checksumCorrect || "skip".Equals(Saxon.Ejava.lang.System.getProperty("saxon-checksum")));
        }
        private int hash(string s, int sequence) {
            digest.update(sequence);
            digest.update(s);
            int h = (sequence<<8);
            for (int i = 0; (i<s.Length); i++) {
                h = (((h<<1))+s[i]);
            }
            return h;
        }
        private int hash(Saxon.Hej.om.NodeName n, int sequence) {
            return (hash(n.getLocalPart(), sequence)^hash(n.getNamespaceUri().ToString(), sequence));
        }
    
    }
}