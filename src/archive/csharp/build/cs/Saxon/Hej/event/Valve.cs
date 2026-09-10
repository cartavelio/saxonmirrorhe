using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class Valve : Saxon.Hej.@event.ProxyReceiver {
        private bool started = false;
        private readonly Saxon.Hej.om.NamespaceUri testNamespace;
        private readonly Saxon.Hej.@event.Receiver alternativeReceiver;
        public Valve(Saxon.Hej.om.NamespaceUri testNamespace, Saxon.Hej.@event.Receiver primary, Saxon.Hej.@event.Receiver secondary) : base (primary) {
            this.testNamespace = testNamespace;
            this.alternativeReceiver = secondary;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (!(started)) {
                started = true;
                if (elemName.getNamespaceUri().Equals(testNamespace)) {
                    alternativeReceiver.open();
                    alternativeReceiver.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                    try {
                        getNextReceiver().close();
                    } catch (Saxon.Hej.trans.XPathException err) {
                    }
                    setUnderlyingReceiver(alternativeReceiver);
                }
            }
            base.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public bool wasDiverted() {
            return (getNextReceiver() == alternativeReceiver);
        }
    
    }
}