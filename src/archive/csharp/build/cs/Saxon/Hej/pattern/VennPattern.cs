using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public abstract class VennPattern : Saxon.Hej.pattern.Pattern {
        protected internal Saxon.Hej.pattern.Pattern p1, p2;
        public VennPattern(Saxon.Hej.pattern.Pattern p1, Saxon.Hej.pattern.Pattern p2)  {
            this.p1 = p1;
            this.p2 = p2;
            adoptChildExpression(p1);
            adoptChildExpression(p2);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(new Saxon.Hej.expr.Operand(this, p1, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION), new Saxon.Hej.expr.Operand(this, p2, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION));
        }
        public override Saxon.Hej.pattern.Pattern simplify() {
            p1 = p1.simplify();
            p2 = p2.simplify();
            return this;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            mustBeNodePattern(p1);
            p1 = p1.typeCheck(visitor, contextItemType);
            mustBeNodePattern(p2);
            p2 = p2.typeCheck(visitor, contextItemType);
            return this;
        }
        private void mustBeNodePattern(Saxon.Hej.pattern.Pattern p) {
            if ((p is Saxon.Hej.pattern.NodeTestPattern)) {
                Saxon.Hej.type.ItemType it = p.getItemType();
                if (!(((it is Saxon.Hej.pattern.NodeTest)))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("The operands of a union, intersect, or except pattern "+"must be patterns that match nodes"), "XPTY0004");
                    err.setIsTypeError(true);
                    throw err;
                }
            }
        }
        public override void bindCurrent(Saxon.Hej.expr.LocalBinding binding) {
            p1.bindCurrent(binding);
            p2.bindCurrent(binding);
        }
        public override bool isMotionless() {
            return (p1.isMotionless() && p2.isMotionless());
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            nextFree = p1.allocateSlots(slotManager, nextFree);
            nextFree = p2.allocateSlots(slotManager, nextFree);
            return nextFree;
        }
        public void gatherComponentPatterns(System.Collections.Generic.ISet<Saxon.Hej.pattern.Pattern> set) {
            if ((p1 is Saxon.Hej.pattern.VennPattern)) {
                ((Saxon.Hej.pattern.VennPattern)(p1)).gatherComponentPatterns(set);
            } else {
                set.Add(p1);
            }
            if ((p2 is Saxon.Hej.pattern.VennPattern)) {
                ((Saxon.Hej.pattern.VennPattern)(p2)).gatherComponentPatterns(set);
            } else {
                set.Add(p2);
            }
        }
        public override int getDependencies() {
            return (p1.getDependencies()|p2.getDependencies());
        }
        public Saxon.Hej.pattern.Pattern getLHS() {
            return p1;
        }
        public Saxon.Hej.pattern.Pattern getRHS() {
            return p2;
        }
        public override bool matchesCurrentGroup() {
            return (p1.matchesCurrentGroup() || p2.matchesCurrentGroup());
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.pattern.VennPattern)) {
                System.Collections.Generic.ISet<Saxon.Hej.pattern.Pattern> s0 = new System.Collections.Generic.HashSet<Saxon.Hej.pattern.Pattern>(10);
                gatherComponentPatterns(s0);
                System.Collections.Generic.ISet<Saxon.Hej.pattern.Pattern> s1 = new System.Collections.Generic.HashSet<Saxon.Hej.pattern.Pattern>(10);
                ((Saxon.Hej.pattern.VennPattern)(other)).gatherComponentPatterns(s1);
                return s0.Equals(s1);
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            return ((0x6bd723a6^p1.GetHashCode())^p2.GetHashCode());
        }
        protected internal abstract string getOperatorName();
        public override string reconstruct() {
            return ((((p1+" ")+getOperatorName())+" ")+p2);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.venn");
            presenter.emitAttribute("op", getOperatorName());
            p1.export(presenter);
            p2.export(presenter);
            presenter.endElement();
        }
    
    }
}