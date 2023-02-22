using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class HTML50Emitter : Saxon.Hej.serialize.HTMLEmitter {

        static HTML50Emitter() {
            setEmptyTag("area");
            setEmptyTag("base");
            setEmptyTag("base");
            setEmptyTag("basefont");
            setEmptyTag("br");
            setEmptyTag("col");
            setEmptyTag("embed");
            setEmptyTag("frame");
            setEmptyTag("hr");
            setEmptyTag("img");
            setEmptyTag("input");
            setEmptyTag("isindex");
            setEmptyTag("keygen");
            setEmptyTag("link");
            setEmptyTag("meta");
            setEmptyTag("param");
            setEmptyTag("source");
            setEmptyTag("track");
            setEmptyTag("wbr");
        }
        public HTML50Emitter()  {
            version = 5;
        }
        protected internal override bool isHTMLElement(Saxon.Hej.om.NodeName name) {
            Saxon.Hej.om.NamespaceUri uri = name.getNamespaceUri();
            return (uri.isEmpty() || uri.Equals(Saxon.Hej.om.NamespaceUri.XHTML));
        }
        protected internal override void openDocument() {
            version = 5;
            base.openDocument();
        }
        protected internal override void writeDocType(Saxon.Hej.om.NodeName name, string displayName, string systemId, string publicId) {
            try {
                if (((systemId == null) && (publicId == null))) {
                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(name.getLocalPart(), "html")) {
                        writer.writeAscii(DOCTYPE);
                    }
                } else {
                    base.writeDocType(name, displayName, systemId, publicId);
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        private readonly static new byte[] DOCTYPE = Saxon.Hej.str.StringConstants.bytes("<!DOCTYPE HTML>");
        protected internal override bool writeDocTypeWithNullSystemId() {
            return true;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (!(started)) {
                openDocument();
                string systemId = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM);
                string publicId = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_PUBLIC);
                if ("".Equals(systemId)) {
                    systemId = null;
                }
                if ("".Equals(publicId)) {
                    publicId = null;
                }
                writeDocType(elemName, "html", systemId, publicId);
                started = true;
            }
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        protected internal override bool rejectControlCharacters() {
            return false;
        }
    
    }
}