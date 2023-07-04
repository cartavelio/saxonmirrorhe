using System.Xml;
using Saxon.Api;

namespace Saxon.Callbacks {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StandardMessageHandler {
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.str.UnicodeWriter writer;
        private readonly Saxon.Hej.serialize.SerializationProperties serializationProperties;

        public StandardMessageHandler(Saxon.Hej.Configuration config) {
            this.config = config;
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, "xml");
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT, "yes");
            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
            serializationProperties = new Saxon.Hej.serialize.SerializationProperties(props);
        }

        public void setUnicodeWriter(Saxon.Hej.str.UnicodeWriter writer) {
            this.writer = writer;
        }

        public Saxon.Hej.str.UnicodeWriter getUnicodeWriter() {
            return this.writer;
        }

        public void accept(Saxon.Api.Message message) {
            lock (this) {
                try {
                    Saxon.Api.XdmNode node = message.Content;
                    System.Diagnostics.Debug.Assert((node.NodeKind == XdmNodeKind.Document));
                    if ((writer == null)) {
                        System.IO.TextWriter w = config.getLogger().asWriter();
                        writer = new Saxon.Hej.str.UnicodeWriterToWriter(w);
                    }

                    Saxon.Hej.serialize.UnicodeWriterResult result =
                        new Saxon.Hej.serialize.UnicodeWriterResult(writer, null);
                    Saxon.Hej.@event.Receiver output = config.getSerializerFactory()
                        .getReceiver(result, serializationProperties);
                    output.open();
                    output.append(((Saxon.Hej.om.NodeInfo) node.Unwrap()));
                    output.close();
                }
                catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.IO.IOException) {
                }
            }
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
