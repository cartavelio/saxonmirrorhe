using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class ProcInstImpl : Saxon.Hej.tree.linked.NodeImpl {
        internal Saxon.Hej.str.UnicodeString content;
        internal string name;
        internal string systemId;
        internal int lineNumber = -1;
        internal int columnNumber = -1;
        public ProcInstImpl(string name, Saxon.Hej.str.UnicodeString content)  {
            this.name = name;
            this.content = content;
        }
        public override Saxon.Hej.om.NodeName getNodeName() {
            return new Saxon.Hej.om.NoNamespaceName(name);
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return content;
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return new Saxon.Hej.value.StringValue(getUnicodeStringValue());
        }
        public override int getNodeKind() {
            return Saxon.Hej.type.Type.PROCESSING_INSTRUCTION;
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
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            @out.processingInstruction(getLocalPart(), content, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public override void rename(Saxon.Hej.om.NodeName newNameCode, bool inherit) {
            name = newNameCode.getLocalPart();
        }
        public override void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            content = stringValue;
        }
    
    }
}