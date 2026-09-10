using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class MessageWarner : Saxon.Hej.serialize.XMLEmitter {
        private bool abort = false;
        private string errorCode = null;
        private System.IO.StringWriter stringWriter;
        public override void startDocument(int properties) {
            stringWriter = new System.IO.StringWriter();
            writer = new Saxon.Hej.str.UnicodeWriterToWriter(stringWriter);
            abort = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.TERMINATE);
            base.startDocument(properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (target.Equals("error-code")) {
                errorCode = data.ToString();
            } else {
                base.processingInstruction(target, data, locationId, properties);
            }
        }
        public override void endDocument() {
            Saxon.Hej.lib.ErrorReporter reporter = getPipelineConfiguration().getErrorReporter();
            Saxon.Hej.trans.XmlProcessingIncident de = new Saxon.Hej.trans.XmlProcessingIncident(stringWriter.ToString(), ((errorCode == null) ? "XTMM9000" : errorCode));
            if (!(abort)) {
                de = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)de.asWarning());
            }
            reporter.report(de);
        }
        public override void close() {
        }
    
    }
}