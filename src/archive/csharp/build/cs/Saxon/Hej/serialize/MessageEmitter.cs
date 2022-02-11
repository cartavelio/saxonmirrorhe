using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class MessageEmitter : Saxon.Hej.serialize.XMLEmitter {
        public MessageEmitter()  {
        }
        public void setWriter(System.IO.TextWriter writer) {
            this.writer = new Saxon.Hej.str.UnicodeWriterToWriter(writer);
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipelineConfiguration) {
            base.setPipelineConfiguration(pipelineConfiguration);
            if ((writer == null)) {
                System.IO.TextWriter w = getConfiguration().getLogger().asWriter();
                writer = new Saxon.Hej.str.UnicodeWriterToWriter(w);
            }
            try {
                Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, "xml");
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT, "yes");
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
                setOutputProperties(props);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e);
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(suppressProcessingInstruction(target, data, locationId, properties))) {
                base.processingInstruction(target, data, locationId, properties);
            }
        }
        protected internal bool suppressProcessingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            return target.Equals("error-code");
        }
        public override void endDocument() {
            try {
                if ((writer != null)) {
                    writer.writeCodePoint('\n');
                    writer.flush();
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
            base.endDocument();
        }
        public override void close() {
            try {
                if ((writer != null)) {
                    writer.flush();
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
            base.close();
        }
    
    }
}