using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class AttributeInfo {
        private readonly Saxon.Hej.om.NodeName nodeName;
        private readonly Saxon.Hej.type.SimpleType type;
        private readonly string value;
        private readonly Saxon.Hej.s9api.Location location;
        private readonly int properties;
        public AttributeInfo(Saxon.Hej.om.NodeName nodeName, Saxon.Hej.type.SimpleType type, string value, Saxon.Hej.s9api.Location location, int properties)  {
            this.nodeName = nodeName;
            this.type = type;
            this.value = value;
            this.location = location;
            this.properties = properties;
        }
        public Saxon.Hej.om.NodeName getNodeName() {
            return nodeName;
        }
        public Saxon.Hej.type.SimpleType getType() {
            return type;
        }
        public string getValue() {
            return value;
        }
        public Saxon.Hej.value.StringValue getXdmStringValue() {
            return new Saxon.Hej.value.StringValue(value);
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return location;
        }
        public int getProperties() {
            return properties;
        }
        public bool isId() {
            try {
                return ((Saxon.Hej.om.StandardNames.XML_ID_NAME.Equals(nodeName) || Saxon.Hej.@event.ReceiverOption.contains(getProperties(), Saxon.Hej.@event.ReceiverOption.IS_ID)) || getType().isIdType());
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
        }
        public Saxon.Hej.om.AttributeInfo withNodeName(Saxon.Hej.om.NodeName newName) {
            return new Saxon.Hej.om.AttributeInfo(newName, type, value, location, properties);
        }
        public class Deleted : Saxon.Hej.om.AttributeInfo {
            public Deleted(Saxon.Hej.om.AttributeInfo att) : base (att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties()) {
            }
        
        }
    
    }
}