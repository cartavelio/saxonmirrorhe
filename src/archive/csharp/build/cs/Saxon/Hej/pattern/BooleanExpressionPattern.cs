using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class BooleanExpressionPattern : Saxon.Hej.pattern.Pattern, Saxon.Hej.pattern.PatternWithPredicate {
        private readonly Saxon.Hej.expr.Operand expressionOp;
        public BooleanExpressionPattern(Saxon.Hej.expr.Expression expression)  {
            this.expressionOp = new Saxon.Hej.expr.Operand(this, expression, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            setPriority(1);
        }
        public Saxon.Hej.expr.Expression getPredicate() {
            return expressionOp.getChildExpression();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return expressionOp;
        }
        public override Saxon.Hej.type.UType getUType() {
            if ((getPredicate() is Saxon.Hej.expr.InstanceOfExpression)) {
                return ((Saxon.Hej.expr.InstanceOfExpression)(getPredicate())).getRequiredItemType().getUType();
            } else {
                return Saxon.Hej.type.UType.ANY;
            }
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            return Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(getPredicate(), nextFree, slotManager);
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().getDefaultContextItemStaticInfo();
            expressionOp.setChildExpression(getPredicate().typeCheck(visitor, cit));
            return this;
        }
        public override Saxon.Hej.pattern.Pattern optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().getDefaultContextItemStaticInfo();
            expressionOp.setChildExpression(getPredicate().optimize(visitor, cit));
            return this;
        }
        public override bool isMotionless() {
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE cio = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(getConfiguration().makeContextItemStaticInfo(getItemType(), false));
            cio.setContextPostureStriding();
            /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(getPredicate(), cio, null);
            return (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(getPredicate()) == Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator(item);
            c2.setCurrentIterator(iter);
            c2.setCurrentOutputUri(null);
            try {
                return getPredicate().effectiveBooleanValue(c2);
            } catch (Saxon.Hej.trans.XPathException e) {
                return false;
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((getPredicate() is Saxon.Hej.expr.InstanceOfExpression)) {
                Saxon.Hej.expr.InstanceOfExpression ioe = (Saxon.Hej.expr.InstanceOfExpression)(getPredicate());
                if ((ioe.getBaseExpression() is Saxon.Hej.expr.ContextItemExpression)) {
                    return ioe.getRequiredItemType();
                }
            }
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override int getFingerprint() {
            return -1;
        }
        public override string reconstruct() {
            return ((".["+getPredicate())+"]");
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.pattern.BooleanExpressionPattern)) && ((Saxon.Hej.pattern.BooleanExpressionPattern)(other)).getPredicate().isEqual(getPredicate()));
        }
        protected internal override int computeHashCode() {
            return (0x7aeffea9^getPredicate().GetHashCode());
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.BooleanExpressionPattern n = new Saxon.Hej.pattern.BooleanExpressionPattern(getPredicate().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.booleanExp");
            getPredicate().export(presenter);
            presenter.endElement();
        }
    
    }
}