using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class SequenceNormalizer : Saxon.Hej.@event.ProxyReceiver {
        protected internal int level = 0;
        private System.Collections.Generic.IList<Saxon.Hej.s9api.Action> actionList;
        private bool failed = false;
        public SequenceNormalizer(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void open() {
            level = 0;
            previousAtomic = false;
            base.open();
            getNextReceiver().startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override void startDocument(int properties) {
            level++;
            previousAtomic = false;
        }
        public override void endDocument() {
            level--;
            previousAtomic = false;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            try {
                level++;
                base.startElement(elemName, type, attributes, namespaces, location, properties);
                previousAtomic = false;
            } catch (Saxon.Hej.trans.XPathException e) {
                failed = true;
                throw;
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                base.characters(chars, locationId, properties);
                previousAtomic = false;
            } catch (Saxon.Hej.trans.XPathException e) {
                failed = true;
                throw;
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                base.processingInstruction(target, data, locationId, properties);
                previousAtomic = false;
            } catch (Saxon.Hej.trans.XPathException e) {
                failed = true;
                throw;
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                base.comment(chars, locationId, properties);
                previousAtomic = false;
            } catch (Saxon.Hej.trans.XPathException e) {
                failed = true;
                throw;
            }
        }
        public override void endElement() {
            try {
                level--;
                base.endElement();
                previousAtomic = false;
            } catch (Saxon.Hej.trans.XPathException e) {
                failed = true;
                throw;
            }
        }
        public override void close() {
            if (failed) {
                base.close();
            } else {
                getNextReceiver().endDocument();
                base.close();
                try {
                    if ((actionList != null)) {
                        foreach (Saxon.Hej.s9api.Action action in actionList) {
                            /*Delegate.act()*/action();
                        }
                    }
                } catch (Saxon.Api.SaxonApiException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
        }
        public void onClose(System.Collections.Generic.IList<Saxon.Hej.s9api.Action> actionList) {
            this.actionList = actionList;
        }
        public void onClose(Saxon.Hej.s9api.Action action) {
            if ((actionList == null)) {
                actionList = new System.Collections.Generic.List<Saxon.Hej.s9api.Action>();
            }
            actionList.Add(action);
        }
    
    }
}