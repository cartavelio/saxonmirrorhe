using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class ReplaceAttributeAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.NodeInfo element;
        private readonly Saxon.Hej.om.NodeInfo oldAttribute;
        private readonly Saxon.Hej.om.AttributeMap newAttributes;
        private readonly bool inherit;
        internal ReplaceAttributeAction(Saxon.Hej.om.NodeInfo element, Saxon.Hej.om.NodeInfo oldAttribute, Saxon.Hej.om.AttributeMap newAttributes, bool inherit)  {
            this.element = element;
            this.oldAttribute = oldAttribute;
            this.newAttributes = newAttributes;
            this.inherit = inherit;
        }
        public bool isInheritNamespaces() {
            return inherit;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            if (!((((element is Saxon.Hej.om.MutableNodeInfo) && element.getTreeInfo().isMutable())))) {
                throw new Saxon.Hej.trans.XPathException("Attempting to update a non-updatable element", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
            }
            affectedRootSet.Add((Saxon.Hej.om.MutableNodeInfo)(element.getRoot()));
            ((Saxon.Hej.om.MutableNodeInfo)(element)).removeAttribute(oldAttribute);
            int len = newAttributes.size();
            foreach (Saxon.Hej.om.AttributeInfo att in newAttributes) {
                Saxon.Hej.om.NodeName name = att.getNodeName();
                Saxon.Hej.type.SimpleType typeCode = att.getType();
                string value = att.getValue();
                int properties = att.getProperties();
                if ((element.getAttributeValue(name.getNamespaceUri(), name.getLocalPart()) != null)) {
                    throw new Saxon.Hej.trans.XPathException("Attempting to add an attribute that already exists");
                }
                ((Saxon.Hej.om.MutableNodeInfo)(element)).addAttribute(name, typeCode, value, properties, inherit);
            }
            ((Saxon.Hej.om.MutableNodeInfo)(element)).removeTypeAnnotation();
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return element;
        }
        public Saxon.Hej.om.NodeInfo getOldAttribute() {
            return oldAttribute;
        }
        public Saxon.Hej.om.AttributeMap getNewAttributes() {
            return newAttributes;
        }
        public override int getApplyPhase() {
            return 3;
        }
    
    }
}