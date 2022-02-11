using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class MessageListenerProxy : Saxon.Hej.@event.SequenceWriter {
        private readonly Saxon.Hej.s9api.MessageListener listener;
        private bool terminate;
        private Saxon.Hej.s9api.Location locationId;
        private string errorCode;
        protected internal MessageListenerProxy(Saxon.Hej.s9api.MessageListener listener, Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
            setTreeModel(Saxon.Hej.om.TreeModel.LINKED_TREE);
            this.listener = listener;
        }
        public Saxon.Hej.s9api.MessageListener getMessageListener() {
            return listener;
        }
        public override void startDocument(int properties) {
            terminate = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.TERMINATE);
            locationId = null;
            errorCode = null;
            base.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((this.locationId == null)) {
                this.locationId = location;
            }
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((target.Equals("error-code") && (errorCode == null))) {
                errorCode = data.ToString();
            } else {
                base.processingInstruction(target, data, locationId, properties);
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString s, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((this.locationId == null)) {
                this.locationId = locationId;
            }
            base.characters(s, locationId, properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((this.locationId == null)) {
                this.locationId = locationId;
            }
            base.append(item, locationId, copyNamespaces);
        }
        public override void write(Saxon.Hej.om.Item item) {
            Saxon.Hej.s9api.Location loc;
            if ((locationId == null)) {
                loc = Saxon.Hej.expr.parser.Loc.NONE;
            } else {
                loc = locationId.saveLocation();
            }
            listener.message(new Saxon.Api.XdmNode((Saxon.Hej.om.NodeInfo)(item)), terminate, loc);
        }
    
    }
}