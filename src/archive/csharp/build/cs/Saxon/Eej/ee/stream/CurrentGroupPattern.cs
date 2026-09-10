using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class CurrentGroupPattern : Saxon.Hej.pattern.AnchorPattern {
        private static readonly Saxon.Eej.ee.stream.CurrentGroupPattern THE_INSTANCE = new Saxon.Eej.ee.stream.CurrentGroupPattern();
        public static new Saxon.Eej.ee.stream.CurrentGroupPattern getInstance() {
            return THE_INSTANCE;
        }
        protected internal CurrentGroupPattern()  {
        }
        public override bool matchesCurrentGroup() {
            return true;
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return ((Saxon.Eej.ee.stream.ManualGroupIterator)(context.getCurrentGroupIterator())).currentGroupContains(node);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.currentGroup");
            presenter.endElement();
        }
    
    }
}