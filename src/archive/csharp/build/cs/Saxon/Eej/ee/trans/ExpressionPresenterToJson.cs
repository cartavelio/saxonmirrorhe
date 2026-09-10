using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class ExpressionPresenterToJson : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Hej.serialize.JSONEmitter @out;
        private bool pending = false;
        private int level = 0;
        public ExpressionPresenterToJson(Saxon.Hej.serialize.JSONEmitter @out, Saxon.Hej.@event.PipelineConfiguration pipe) : base (new Saxon.Hej.@event.Sink(pipe)) {
            this.@out = @out;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (pending) {
                @out.writeKey("C");
                @out.startArray(false);
            }
            @out.startMap(false);
            @out.writeKey("N");
            @out.writeAtomicValue(new Saxon.Hej.value.StringValue(elemName.getLocalPart()));
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                @out.writeKey(att.getNodeName().getLocalPart());
                @out.writeAtomicValue(new Saxon.Hej.value.StringValue(att.getValue()));
            }
            pending = true;
            level++;
        }
        public override void endElement() {
            if (!(pending)) {
                @out.endArray();
            }
            if ((--level>0)) {
                @out.endMap();
            }
            pending = false;
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            @out.writeKey(target);
            @out.writeAtomicValue(new Saxon.Hej.value.StringValue(data.tidy()));
        }
        public override void endDocument() {
            @out.endMap();
        }
        public override void close() {
            @out.close();
        }
    
    }
}