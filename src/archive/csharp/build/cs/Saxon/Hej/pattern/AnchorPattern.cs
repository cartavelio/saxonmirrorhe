using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class AnchorPattern : Saxon.Hej.pattern.Pattern {
        private static readonly Saxon.Hej.pattern.AnchorPattern THE_INSTANCE = new Saxon.Hej.pattern.AnchorPattern();
        public static Saxon.Hej.pattern.AnchorPattern getInstance() {
            return THE_INSTANCE;
        }
        protected internal AnchorPattern()  {
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.PARENT_NODE_KINDS;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return ((anchor == null) || (node == anchor));
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            throw new Saxon.Ejava.lang.AssertionError();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.AnyNodeTest.getInstance();
        }
        public override string reconstruct() {
            return ".";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.anchor");
            presenter.endElement();
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return this;
        }
    
    }
}