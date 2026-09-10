using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class TextImpl : Saxon.Hej.tree.linked.NodeImpl {
        private Saxon.Hej.str.UnicodeString content;
        public TextImpl(Saxon.Hej.str.UnicodeString content)  {
            this.content = content;
        }
        public void appendStringValue(Saxon.Hej.str.UnicodeString content) {
            this.content = this.content.concat(content);
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return content;
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.TEXT;
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.characters(content, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            if (stringValue.isEmpty()) {
                delete();
            } else {
                content = stringValue;
            }
        }
    
    }
}