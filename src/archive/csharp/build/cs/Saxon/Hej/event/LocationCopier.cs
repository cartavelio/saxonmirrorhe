using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class LocationCopier : Saxon.Hej.@event.CopyInformee {
        private readonly bool wholeDocument;
        private readonly string systemId;
        public LocationCopier(bool wholeDocument, string systemId)  {
            this.wholeDocument = wholeDocument;
            this.systemId = systemId;
        }
        public System.Object notifyElementNode(Saxon.Hej.om.NodeInfo element) {
            string systemId = (wholeDocument ? element.getSystemId() : element.getBaseURI());
            int lineNumber = element.getLineNumber();
            int columnNumber = element.getColumnNumber();
            return new Saxon.Hej.expr.parser.Loc(systemId, lineNumber, columnNumber);
        }
        public string getSystemId() {
            return systemId;
        }
    
    }
}