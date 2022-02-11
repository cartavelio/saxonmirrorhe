using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class HTML40Emitter : Saxon.Hej.serialize.HTMLEmitter {

        static HTML40Emitter() {
            setEmptyTag("area");
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
            setEmptyTag("link");
            setEmptyTag("meta");
            setEmptyTag("param");
        }
        public HTML40Emitter()  {
        }
        protected internal override bool isHTMLElement(Saxon.Hej.om.NodeName name) {
            return name.getURI().Equals("");
        }
        protected internal override void openDocument() {
            string versionProperty = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.HTML_VERSION);
            if ((versionProperty == null)) {
                versionProperty = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION);
            }
            if ((versionProperty != null)) {
                if ((versionProperty.Equals("4.0") || versionProperty.Equals("4.01"))) {
                    version = 4;
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unsupported HTML version: "+versionProperty));
                    err.setErrorCode("SESU0013");
                    throw err;
                }
            }
            base.openDocument();
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
                if (((systemId != null) || (publicId != null))) {
                    writeDocType(null, "html", systemId, publicId);
                }
                started = true;
            }
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        protected internal override bool rejectControlCharacters() {
            return true;
        }
    
    }
}