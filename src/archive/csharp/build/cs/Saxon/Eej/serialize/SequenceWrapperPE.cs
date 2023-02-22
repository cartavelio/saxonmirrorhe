using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class SequenceWrapperPE : Saxon.Hej.query.SequenceWrapper {
        private Saxon.Hej.om.FingerprintedQName resultMap;
        private Saxon.Hej.om.FingerprintedQName resultMapEntry;
        private Saxon.Hej.om.FingerprintedQName resultMapKey;
        private Saxon.Hej.om.FingerprintedQName resultMapValue;
        public SequenceWrapperPE(Saxon.Hej.@event.Receiver destination) : base (destination) {
        }
        public override void open() {
            base.open();
            resultMap = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "map");
            resultMapEntry = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "map-entry");
            resultMapKey = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "map-key");
            resultMapValue = new Saxon.Hej.om.FingerprintedQName("result", RESULT_NS, "map-value");
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item is Saxon.Hej.ma.map.MapItem)) {
                Saxon.Hej.@event.ComplexContentOutputter @out = getDestination();
                @out.startElement(resultMap, Saxon.Hej.type.Untyped.getInstance(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(item);
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                    @out.startElement(resultMapEntry, Saxon.Hej.type.Untyped.getInstance(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    @out.startElement(resultMapKey, Saxon.Hej.type.Untyped.getInstance(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    append(pair.key);
                    @out.endElement();
                    @out.startElement(resultMapValue, Saxon.Hej.type.Untyped.getInstance(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    Saxon.Hej.om.SequenceIterator value = pair.value.iterate();
                    Saxon.Hej.om.Item valItem;
                    while (((valItem = value.next()) != null)) {
                        append(valItem);
                    }
                    @out.endElement();
                    @out.endElement();
                }
                @out.endElement();
            } else {
                base.append(item, locationId, copyNamespaces);
            }
        }
    
    }
}