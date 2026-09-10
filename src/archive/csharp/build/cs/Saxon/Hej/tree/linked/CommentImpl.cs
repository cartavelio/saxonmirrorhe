using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class CommentImpl : Saxon.Hej.tree.linked.NodeImpl {
        internal Saxon.Hej.str.UnicodeString comment;
        internal string systemId;
        internal int lineNumber = -1;
        internal int columnNumber = -1;
        public CommentImpl(Saxon.Hej.str.UnicodeString content)  {
            this.comment = content;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return comment;
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return new Saxon.Hej.value.StringValue(getUnicodeStringValue());
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.COMMENT;
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.comment(comment, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            comment = stringValue;
        }
        public void setLocation(string uri, int lineNumber, int columnNumber) {
            this.systemId = uri;
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
        }
        public override string getSystemId() {
            return systemId;
        }
        public override int getLineNumber() {
            return lineNumber;
        }
        public override int getColumnNumber() {
            return columnNumber;
        }
    
    }
}