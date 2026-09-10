using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class TEXTEmitter : Saxon.Hej.serialize.XMLEmitter {
        private Saxon.Ejava.util.regex.Pattern newlineMatcher = null;
        private string newlineRepresentation = null;
        public override void open() {
        }
        protected internal override void openDocument() {
            System.Diagnostics.Debug.Assert((writer != null));
            if ((characterSet == null)) {
                characterSet = Saxon.Hej.serialize.charcode.UTF8CharacterSet.getInstance();
            }
            string encoding = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
            if (((encoding == null) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "utf8"))) {
                encoding = "UTF-8";
            }
            string byteOrderMark = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.BYTE_ORDER_MARK);
            string nl = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NEWLINE);
            if (((nl != null) && !(nl.Equals("\n")))) {
                newlineRepresentation = nl;
                newlineMatcher = Saxon.Ejava.util.regex.Pattern.compile("\\n");
            }
            if (("yes".Equals(byteOrderMark) && (((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-8", encoding) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-16LE", encoding)) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-16BE", encoding))))) {
                try {
                    writer.writeCodePoint(0xFEFF);
                } catch (System.IO.IOException err) {
                }
            }
            started = true;
        }
        public override void writeDeclaration() {
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(started)) {
                openDocument();
            }
            if (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS))) {
                int badchar = testCharacters(chars);
                if ((badchar != 0)) {
                    throw new Saxon.Hej.trans.XPathException((("Output character not available in this encoding (x"+Saxon.Impl.Helpers.Utils.intToHexString(badchar))+")"), "SERE0008");
                }
            }
            if ((newlineMatcher != null)) {
                chars = Saxon.Hej.str.StringView.of(newlineMatcher.matcher(chars.ToString()).replaceAll(newlineRepresentation));
            }
            try {
                writer.write(chars);
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            previousAtomic = false;
        }
        public override void endElement() {
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString value, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
    
    }
}