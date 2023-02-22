using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class NamePoolConverter : Saxon.Hej.@event.ProxyReceiver {
        internal Saxon.Hej.om.NamePool oldPool;
        internal Saxon.Hej.om.NamePool newPool;
        public NamePoolConverter(Saxon.Hej.@event.Receiver next, Saxon.Hej.om.NamePool oldPool, Saxon.Hej.om.NamePool newPool) : base (next) {
            this.oldPool = oldPool;
            this.newPool = newPool;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            checkType(type);
            int fp = newPool.allocateFingerprint(elemName.getURI(), elemName.getLocalPart());
            Saxon.Hej.om.CodedName newElemName = new Saxon.Hej.om.CodedName(fp, elemName.getPrefix(), newPool);
            Saxon.Hej.om.AttributeMap newAtts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                checkType(att.getType());
                int afp = newPool.allocateFingerprint(att.getNodeName().getURI(), att.getNodeName().getLocalPart());
                Saxon.Hej.om.NodeName newAttName = new Saxon.Hej.om.CodedName(afp, att.getNodeName().getPrefix(), newPool);
                newAtts = newAtts.put(new Saxon.Hej.om.AttributeInfo(newAttName, att.getType(), att.getValue(), att.getLocation(), att.getProperties()));
            }
            nextReceiver.startElement(newElemName, type, newAtts, namespaces, location, properties);
        }
        private void checkType(Saxon.Hej.type.SchemaType type) {
            if ((((type.getFingerprint()&Saxon.Hej.om.NamePool.USER_DEFINED_MASK)) != 0)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot convert a user-typed node to a different name pool");
            }
        }
    
    }
}