using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ParseXmlFragment : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.EmptySequence.getInstance(); // TODO: implement me!
        }
        private class OuterElementStripper : Saxon.Hej.@event.ProxyReceiver {
            public OuterElementStripper(Saxon.Hej.@event.Receiver next) : base (next) {
            }
            private int level = 0;
            private readonly bool suppressStartContent = false;
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                if ((level++>0)) {
                    base.startElement(elemName, type, attributes, namespaces, location, properties);
                }
            }
            public override void endElement() {
                if ((--level>0)) {
                    base.endElement();
                }
            }
        
        }
    
    }
}