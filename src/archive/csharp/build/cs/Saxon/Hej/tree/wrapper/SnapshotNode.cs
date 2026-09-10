using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class SnapshotNode : Saxon.Hej.tree.wrapper.VirtualCopy, Saxon.Hej.om.NodeInfo {
        protected internal Saxon.Hej.om.NodeInfo pivot;
        protected internal SnapshotNode(Saxon.Hej.om.NodeInfo @base, Saxon.Hej.om.NodeInfo pivot) : base (@base, pivot.getRoot()) {
            this.pivot = pivot;
        }
        public static Saxon.Hej.tree.wrapper.SnapshotNode makeSnapshot(Saxon.Hej.om.NodeInfo original) {
            Saxon.Hej.tree.wrapper.SnapshotNode vc = new Saxon.Hej.tree.wrapper.SnapshotNode(original, original);
            Saxon.Hej.Configuration config = original.getConfiguration();
            Saxon.Hej.tree.wrapper.VirtualTreeInfo doc = new Saxon.Hej.tree.wrapper.VirtualTreeInfo(config);
            long docNr = config.getDocumentNumberAllocator().allocateDocumentNumber();
            doc.setDocumentNumber(docNr);
            doc.setCopyAccumulators(true);
            vc.tree = doc;
            doc.setRootNode(vc.getRoot());
            return vc;
        }
        protected internal override Saxon.Hej.tree.wrapper.SnapshotNode wrap(Saxon.Hej.om.NodeInfo node) {
            Saxon.Hej.tree.wrapper.SnapshotNode vc = new Saxon.Hej.tree.wrapper.SnapshotNode(node, pivot);
            vc.tree = tree;
            return vc;
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            if (Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(original, pivot)) {
                return pivot.getUnicodeStringValue();
            } else {
                return original.getUnicodeStringValue();
            }
        }
        public override Saxon.Hej.om.NodeInfo getParent() {
            if ((parent == null)) {
                Saxon.Hej.om.NodeInfo basep = original.getParent();
                if ((basep == null)) {
                    return null;
                }
                parent = wrap(basep);
            }
            return parent;
        }
        public override Saxon.Hej.om.NodeInfo getRoot() {
            return base.getRoot();
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.tree.util.Navigator.copy(this, @out, copyOptions, locationId);
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
              case Saxon.Hej.type.Type.TEXT:
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
              case Saxon.Hej.type.Type.NAMESPACE:
                return original.atomize();
              default:
                if (Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(pivot, original)) {
                    return original.atomize();
                } else {
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(pivot.getUnicodeStringValue());
                }
            }
        }
        public override bool isId() {
            return original.isId();
        }
        public override bool isIdref() {
            return original.isIdref();
        }
        public override bool isNilled() {
            return original.isNilled();
        }
        public override string getPublicId() {
            return ((original != null) ? original.getPublicId() : null);
        }
        public override Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
              case Saxon.Hej.type.Type.NAMESPACE:
              case Saxon.Hej.type.Type.TEXT:
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                switch (axisNumber) {
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                  case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                  default:
                    return base.iterateAxis(axisNumber, nodeTest);
                }
              default:
                if ((!(original.isSameNodeInfo(pivot)) && Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(original, pivot))) {
                    switch (axisNumber) {
                      case Saxon.Hej.om.AxisInfo.CHILD:
                        return Saxon.Hej.tree.util.Navigator.filteredSingleton(getChildOfAncestorNode(), nodeTest);
                      case Saxon.Hej.om.AxisInfo.DESCENDANT:
                      case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                        Saxon.Hej.tree.iter.AxisIterator iter = new Saxon.Hej.tree.util.Navigator.DescendantEnumeration(this, (axisNumber == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF), true);
                        if (!(((nodeTest is Saxon.Hej.pattern.AnyNodeTest)))) {
                            iter = new Saxon.Hej.tree.util.Navigator.AxisFilter(iter, nodeTest);
                        }
                        return iter;
                      case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
                      case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                      case Saxon.Hej.om.AxisInfo.PRECEDING:
                      case Saxon.Hej.om.AxisInfo.FOLLOWING:
                        return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                      default:
                        return base.iterateAxis(axisNumber, nodeTest);
                    }
                } else {
                    return base.iterateAxis(axisNumber, nodeTest);
                }
            }
        }
        private Saxon.Hej.om.NodeInfo getChildOfAncestorNode() {
            int pivotKind = pivot.getNodeKind();
            Saxon.Hej.tree.wrapper.SnapshotNode p = wrap(pivot);
            if (((((pivotKind == Saxon.Hej.type.Type.ATTRIBUTE) || (pivotKind == Saxon.Hej.type.Type.NAMESPACE))) && p.getParent().isSameNodeInfo(this))) {
                return null;
            }
            while (true) {
                Saxon.Hej.tree.wrapper.SnapshotNode q = (Saxon.Hej.tree.wrapper.SnapshotNode)(p.getParent());
                if ((q == null)) {
                    throw new Saxon.Ejava.lang.AssertionError();
                }
                if (q.isSameNodeInfo(this)) {
                    return p;
                }
                p = q;
            }
        }
        protected internal override bool isIncludedInCopy(Saxon.Hej.om.NodeInfo sourceNode) {
            switch (sourceNode.getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
              case Saxon.Hej.type.Type.NAMESPACE:
                return isIncludedInCopy(sourceNode.getParent());
              default:
                return (Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(pivot, sourceNode) || Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(sourceNode, pivot));
            }
        }
    
    }
}