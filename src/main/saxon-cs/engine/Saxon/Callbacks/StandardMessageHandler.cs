using System.Xml;
using Saxon.Api;

namespace Saxon.Callbacks {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StandardMessageHandler {
        private readonly net.sf.saxon.Configuration config;
        private net.sf.saxon.str.UnicodeWriter writer;
        private readonly net.sf.saxon.serialize.SerializationProperties serializationProperties;

        public StandardMessageHandler(net.sf.saxon.Configuration config) {
            this.config = config;
            java.util.Properties props = new java.util.Properties();
            props.setProperty(javax.xml.transform.OutputKeys.METHOD, "xml");
            props.setProperty(javax.xml.transform.OutputKeys.INDENT, "yes");
            props.setProperty(javax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
            serializationProperties = new net.sf.saxon.serialize.SerializationProperties(props);
        }

        public void setUnicodeWriter(net.sf.saxon.str.UnicodeWriter writer) {
            this.writer = writer;
        }

        public net.sf.saxon.str.UnicodeWriter getUnicodeWriter() {
            return this.writer;
        }

        public void accept(Saxon.Api.Message message) {
            lock (this) {
                try {
                    Saxon.Api.XdmNode node = message.Content;
                    System.Diagnostics.Debug.Assert((node.NodeKind == XmlNodeType.Document));
                    if ((writer == null)) {
                        System.IO.TextWriter w = config.getLogger().asWriter();
                        writer = new net.sf.saxon.str.UnicodeWriterToWriter(w);
                    }

                    net.sf.saxon.serialize.UnicodeWriterResult result =
                        new net.sf.saxon.serialize.UnicodeWriterResult(writer, null);
                    net.sf.saxon.@event.Receiver output = config.getSerializerFactory()
                        .getReceiver(result, serializationProperties);
                    output.open();
                    output.append(((net.sf.saxon.om.NodeInfo) node.Unwrap()));
                    output.close();
                    writer.writeCodePoint(10);
                }
                catch (Exception e) when (e is net.sf.saxon.trans.XPathException || e is System.IO.IOException) {
                }
            }
        }
    }
}
