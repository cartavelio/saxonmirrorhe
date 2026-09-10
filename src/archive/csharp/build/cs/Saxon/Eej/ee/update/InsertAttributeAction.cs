using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class InsertAttributeAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.MutableNodeInfo element;
        private readonly Saxon.Hej.om.NodeName name;
        private readonly Saxon.Hej.type.SimpleType typeCode;
        private readonly string value;
        private readonly int properties;
        private readonly bool inherit;
        internal InsertAttributeAction(Saxon.Hej.om.MutableNodeInfo element, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SimpleType typeCode, string value, int properties, bool inherit)  {
            this.element = element;
            this.name = fixNodeName(name);
            this.typeCode = typeCode;
            this.value = value;
            this.properties = properties;
            this.inherit = inherit;
        }
        public bool isInheritNamespaces() {
            return inherit;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            if (!(element.getTreeInfo().isMutable())) {
                throw new Saxon.Hej.trans.XPathException("Attempting to update a non-updatable element", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
            }
            if ((element.getAttributeValue(name.getNamespaceUri(), name.getLocalPart()) != null)) {
                throw new Saxon.Hej.trans.XPathException((("Attempt to insert an attribute "+name.getDisplayName())+" that is already present on the target element"), "XUDY0021");
            }
            affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(element.getRoot()));
            element.addAttribute(name, typeCode, value, properties, inherit);
            element.removeTypeAnnotation();
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return element;
        }
        public Saxon.Hej.om.NodeName getAttributeName() {
            return name;
        }
        public Saxon.Hej.type.SimpleType getNewTypeCode() {
            return typeCode;
        }
        public string getNewStringValue() {
            return value;
        }
        public override int getApplyPhase() {
            return 2;
        }
    
    }
}