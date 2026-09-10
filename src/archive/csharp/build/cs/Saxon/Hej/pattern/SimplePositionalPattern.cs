using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public sealed class SimplePositionalPattern : Saxon.Hej.pattern.Pattern {
        private readonly Saxon.Hej.pattern.NodeTest nodeTest;
        private readonly int position;
        public SimplePositionalPattern(Saxon.Hej.pattern.NodeTest nodeTest, int position)  {
            this.nodeTest = nodeTest;
            this.position = position;
        }
        public int getPosition() {
            return position;
        }
        public Saxon.Hej.pattern.NodeTest getNodeTest() {
            return nodeTest;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return ((item is Saxon.Hej.om.NodeInfo) && matchesBeneathAnchor((Saxon.Hej.om.NodeInfo)(item), null, context));
        }
        public override Saxon.Hej.type.UType getUType() {
            return nodeTest.getUType();
        }
        public override int getFingerprint() {
            return nodeTest.getFingerprint();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return nodeTest.getPrimitiveItemType();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.pattern.SimplePositionalPattern)) {
                Saxon.Hej.pattern.SimplePositionalPattern fp = (Saxon.Hej.pattern.SimplePositionalPattern)(other);
                return (nodeTest.Equals(fp.nodeTest) && (position == fp.position));
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            return (nodeTest.GetHashCode()^((position<<3)));
        }
        public override bool isMotionless() {
            return false;
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            if (!(nodeTest.test(node))) {
                return false;
            }
            if (((anchor != null) && (node.getParent() != anchor))) {
                return false;
            }
            return (position == Saxon.Hej.tree.util.Navigator.getSiblingPosition(node, nodeTest, position));
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.SimplePositionalPattern n = new Saxon.Hej.pattern.SimplePositionalPattern(nodeTest.copy(), position);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override string reconstruct() {
            return (((nodeTest+"[")+position)+"]");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.simPos");
            presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(nodeTest));
            presenter.emitAttribute("pos", (position+""));
            presenter.endElement();
        }
    
    }
}