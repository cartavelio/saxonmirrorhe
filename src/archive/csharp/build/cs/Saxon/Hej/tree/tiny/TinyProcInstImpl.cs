using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    sealed class TinyProcInstImpl : Saxon.Hej.tree.tiny.TinyNodeImpl {
        public TinyProcInstImpl(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr)  {
            this.tree = tree;
            this.nodeNr = nodeNr;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            int start = tree.alpha[nodeNr];
            int len = tree.beta[nodeNr];
            if ((len == 0)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return tree.commentBuffer.substring(start, (start+len));
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return new Saxon.Hej.value.StringValue(getUnicodeStringValue());
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.PROCESSING_INSTRUCTION;
        }
        public override string getBaseURI() {
            return Saxon.Hej.tree.util.Navigator.getBaseURI(this);
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.processingInstruction(getDisplayName(), getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public string getTarget() {
            return getDisplayName();
        }
        public Saxon.Hej.str.UnicodeString getData() {
            return getUnicodeStringValue();
        }
    
    }
}