using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public abstract class PatternWatch : Saxon.Eej.ee.stream.watch.AbstractWatch {
        private Saxon.Hej.pattern.Pattern selection;
        private Saxon.Eej.ee.stream.om.FleetingParentNode anchor;
        public PatternWatch()  {
        }
        public override bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.expr.XPathContext context) {
            return getSelection().matchesBeneathAnchor(node, getAnchorNode(), context);
        }
        public override bool matchesNodesOfKind(Saxon.Hej.type.UType kind) {
            return getSelection().getUType().overlaps(kind);
        }
        public virtual void setSelection(Saxon.Hej.pattern.Pattern selection) {
            this.selection = selection;
        }
        public Saxon.Hej.pattern.Pattern getSelection() {
            return selection;
        }
        public override void setAnchorNode(Saxon.Eej.ee.stream.om.FleetingParentNode anchor) {
            this.anchor = anchor;
        }
        public override Saxon.Eej.ee.stream.om.FleetingParentNode getAnchorNode() {
            return anchor;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
        }
        public override void processItem(Saxon.Hej.om.Item item) {
        }
        public override void close() {
        }
    
    }
}