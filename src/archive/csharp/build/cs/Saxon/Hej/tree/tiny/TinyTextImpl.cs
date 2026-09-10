using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public sealed class TinyTextImpl : Saxon.Hej.tree.tiny.TinyNodeImpl {
        public TinyTextImpl(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr)  {
            this.tree = tree;
            this.nodeNr = nodeNr;
        }
        public static Saxon.Hej.str.UnicodeString getStringValue(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr) {
            int start = tree.alpha[nodeNr];
            int len = tree.beta[nodeNr];
            return tree.textBuffer.substring(start, (start+len));
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return getStringValue(tree, nodeNr);
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.TEXT;
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.characters(getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return Saxon.Hej.value.StringValue.makeUntypedAtomic(getUnicodeStringValue());
        }
    
    }
}