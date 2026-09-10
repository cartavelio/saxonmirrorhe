using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class SpaceStrippedDocument : Saxon.Hej.om.GenericTreeInfo {
        private readonly Saxon.Hej.om.SpaceStrippingRule strippingRule;
        private readonly bool preservesSpace;
        private readonly bool _containsAssertions;
        private readonly Saxon.Hej.om.TreeInfo underlyingTree;
        public SpaceStrippedDocument(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.om.SpaceStrippingRule strippingRule) : base (doc.getConfiguration()) {
            setRootNode(wrap(doc.getRootNode()));
            this.strippingRule = strippingRule;
            this.underlyingTree = doc;
            preservesSpace = findPreserveSpace(doc);
            _containsAssertions = findAssertions(doc);
        }
        public Saxon.Hej.tree.wrapper.SpaceStrippedNode wrap(Saxon.Hej.om.NodeInfo node) {
            return Saxon.Hej.tree.wrapper.SpaceStrippedNode.makeWrapper(node, this, null);
        }
        public override  /* REPLACING public */ bool isTyped() {
            return underlyingTree.isTyped();
        }
        public Saxon.Hej.om.SpaceStrippingRule getStrippingRule() {
            return strippingRule;
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            Saxon.Hej.om.NodeInfo n = underlyingTree.selectID(id, false);
            if ((n == null)) {
                return null;
            } else {
                return wrap(n);
            }
        }
        public override System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            return underlyingTree.getUnparsedEntityNames();
        }
        public override string[] getUnparsedEntity(string name) {
            return underlyingTree.getUnparsedEntity(name);
        }
        private static bool findPreserveSpace(Saxon.Hej.om.TreeInfo doc) {
            if ((doc is Saxon.Hej.tree.tiny.TinyTree)) {
                return ((Saxon.Hej.tree.tiny.TinyTree)(doc)).hasXmlSpacePreserveAttribute();
            } else {
                Saxon.Hej.tree.iter.AxisIterator iter = doc.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                Saxon.Hej.om.NodeInfo node;
                while (((node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                    string val = node.getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XML, "space");
                    if ("preserve".Equals(val)) {
                        return true;
                    }
                }
                return false;
            }
        }
        private static bool findAssertions(Saxon.Hej.om.TreeInfo doc) {
            if (doc.isTyped()) {
                Saxon.Hej.tree.iter.AxisIterator iter = doc.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                while (true) {
                    Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
                    if ((node == null)) {
                        return false;
                    }
                    Saxon.Hej.type.SchemaType type = node.getSchemaType();
                    if ((type.isComplexType() && ((Saxon.Hej.type.ComplexType)(type)).hasAssertions())) {
                        return true;
                    }
                }
            } else {
                return false;
            }
        }
        public bool containsPreserveSpace() {
            return preservesSpace;
        }
        public bool containsAssertions() {
            return _containsAssertions;
        }
    
    }
}