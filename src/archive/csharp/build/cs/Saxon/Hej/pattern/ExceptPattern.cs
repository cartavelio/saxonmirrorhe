using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class ExceptPattern : Saxon.Hej.pattern.VennPattern {
        public ExceptPattern(Saxon.Hej.pattern.Pattern p1, Saxon.Hej.pattern.Pattern p2) : base (p1, p2) {
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return p1.getItemType();
        }
        public override Saxon.Hej.type.UType getUType() {
            return p1.getUType();
        }
        public override double getDefaultPriority() {
            return p1.getDefaultPriority();
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return (p1.matches(item, context) && !(p2.matches(item, context)));
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return (p1.matchesBeneathAnchor(node, anchor, context) && !(p2.matchesBeneathAnchor(node, anchor, context)));
        }
        public override Saxon.Hej.pattern.Pattern convertToTypedPattern(string val) {
            Saxon.Hej.pattern.Pattern np1 = p1.convertToTypedPattern(val);
            Saxon.Hej.pattern.Pattern np2 = p2.convertToTypedPattern(val);
            if (((p1 == np1) && (p2 == np2))) {
                return this;
            } else {
                return new Saxon.Hej.pattern.ExceptPattern(np1, np2);
            }
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.pattern.ExceptPattern)) {
                System.Collections.Generic.ISet<Saxon.Hej.pattern.Pattern> s0 = new System.Collections.Generic.HashSet<Saxon.Hej.pattern.Pattern>(10);
                gatherComponentPatterns(s0);
                System.Collections.Generic.ISet<Saxon.Hej.pattern.Pattern> s1 = new System.Collections.Generic.HashSet<Saxon.Hej.pattern.Pattern>(10);
                ((Saxon.Hej.pattern.ExceptPattern)(other)).gatherComponentPatterns(s1);
                return s0.Equals(s1);
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            return ((0x7bd7dfa6^p1.GetHashCode())^p2.GetHashCode());
        }
        protected internal override string getOperatorName() {
            return "except";
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.ExceptPattern n = new Saxon.Hej.pattern.ExceptPattern(p1.copy(rebindings), p2.copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
    
    }
}