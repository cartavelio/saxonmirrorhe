using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class SequenceNormalizerWithSpaceSeparator : Saxon.Hej.@event.SequenceNormalizer {
        public SequenceNormalizerWithSpaceSeparator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            decompose(item, locationId, copyNamespaces);
        }
        protected internal override string getErrorCodeForDecomposingFunctionItems() {
            return "SENR0001";
        }
    
    }
}