using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree {
    public class AttributeLocation : Saxon.Hej.s9api.Location {
        private readonly string systemId;
        private readonly int lineNumber;
        private readonly int columnNumber;
        private readonly Saxon.Hej.om.StructuredQName elementName;
        private readonly Saxon.Hej.om.StructuredQName attributeName;
        private Saxon.Hej.om.NodeInfo elementNode;
        public AttributeLocation(Saxon.Hej.om.NodeInfo element, Saxon.Hej.om.StructuredQName attributeName)  {
            this.systemId = element.getSystemId();
            this.lineNumber = element.getLineNumber();
            this.columnNumber = element.getColumnNumber();
            this.elementName = Saxon.Hej.tree.util.Navigator.getNodeName(element);
            this.attributeName = attributeName;
            if (element.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.RETAIN_NODE_FOR_DIAGNOSTICS)) {
                this.elementNode = element;
            }
        }
        public AttributeLocation(Saxon.Hej.om.StructuredQName elementName, Saxon.Hej.om.StructuredQName attributeName, Saxon.Hej.s9api.Location location)  {
            this.systemId = location.getSystemId();
            this.lineNumber = location.getLineNumber();
            this.columnNumber = location.getColumnNumber();
            this.elementName = elementName;
            this.attributeName = attributeName;
        }
        public void setElementNode(Saxon.Hej.om.NodeInfo node) {
            elementNode = node;
        }
        public Saxon.Hej.om.NodeInfo getElementNode() {
            return elementNode;
        }
        public Saxon.Hej.om.StructuredQName getElementName() {
            return elementName;
        }
        public Saxon.Hej.om.StructuredQName getAttributeName() {
            return attributeName;
        }
        public int getColumnNumber() {
            return columnNumber;
        }
        public string getSystemId() {
            return systemId;
        }
        public string getPublicId() {
            return null;
        }
        public int getLineNumber() {
            return lineNumber;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
    
    }
}