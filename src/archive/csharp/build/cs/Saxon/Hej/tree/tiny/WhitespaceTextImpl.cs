using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public sealed class WhitespaceTextImpl : Saxon.Hej.tree.tiny.TinyNodeImpl {
        public WhitespaceTextImpl(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr)  {
            this.tree = tree;
            this.nodeNr = nodeNr;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            long value = ((((long)(tree.alpha[nodeNr])<<32))|(((long)(tree.beta[nodeNr])&unchecked((long)0xffffffffL))));
            return new Saxon.Hej.str.CompressedWhitespace(value);
        }
        public static Saxon.Hej.str.UnicodeString getStringValue(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr) {
            long value = ((((long)(tree.alpha[nodeNr])<<32))|(((long)(tree.beta[nodeNr])&unchecked((long)0xffffffffL))));
            return new Saxon.Hej.str.CompressedWhitespace(value);
        }
        public static void appendStringValue(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr, Saxon.Hej.str.UnicodeBuilder buffer) {
            long value = ((((long)(tree.alpha[nodeNr])<<32))|(((long)(tree.beta[nodeNr])&unchecked((long)0xffffffffL))));
            buffer.append(Saxon.Hej.str.CompressedWhitespace.uncompress(value));
        }
        public override Saxon.Hej.value.StringValue atomize() {
            return new Saxon.Hej.value.UntypedAtomicValue(getUnicodeStringValue());
        }
        public static long getLongValue(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr) {
            return ((((long)(tree.alpha[nodeNr])<<32))|(((long)(tree.beta[nodeNr])&unchecked((long)0xffffffffL))));
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.TEXT;
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.characters(getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
    
    }
}