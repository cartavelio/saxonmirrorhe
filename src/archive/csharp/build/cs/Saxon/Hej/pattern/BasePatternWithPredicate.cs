using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class BasePatternWithPredicate : Saxon.Hej.pattern.Pattern, Saxon.Hej.pattern.PatternWithPredicate {
        internal Saxon.Hej.expr.Operand basePatternOp;
        internal Saxon.Hej.expr.Operand predicateOp;
        public BasePatternWithPredicate(Saxon.Hej.pattern.Pattern basePattern, Saxon.Hej.expr.Expression predicate)  {
            basePatternOp = new Saxon.Hej.expr.Operand(this, basePattern, Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE);
            predicateOp = new Saxon.Hej.expr.Operand(this, predicate, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
            adoptChildExpression(getBasePattern());
            adoptChildExpression(getPredicate());
        }
        public Saxon.Hej.expr.Expression getPredicate() {
            return predicateOp.getChildExpression();
        }
        public Saxon.Hej.pattern.Pattern getBasePattern() {
            return (Saxon.Hej.pattern.Pattern)(basePatternOp.getChildExpression());
        }
        public override void bindCurrent(Saxon.Hej.expr.LocalBinding binding) {
            Saxon.Hej.expr.Expression predicate = getPredicate();
            if (predicate.isCallOn(typeof(Saxon.Hej.functions.Current))) {
                predicateOp.setChildExpression(new Saxon.Hej.expr.LocalVariableReference(binding));
            } else {
                if (Saxon.Hej.expr.parser.ExpressionTool.callsFunction(predicate, Saxon.Hej.functions.Current.FN_CURRENT, false)) {
                    replaceCurrent(predicate, binding);
                }
            }
            getBasePattern().bindCurrent(binding);
        }
        public override bool matchesCurrentGroup() {
            return getBasePattern().matchesCurrentGroup();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(basePatternOp, predicateOp);
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            int n = Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(getPredicate(), nextFree, slotManager);
            return getBasePattern().allocateSlots(slotManager, n);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            if (!(getBasePattern().matches(item, context))) {
                return false;
            }
            return matchesPredicate(item, context);
        }
        private bool matchesPredicate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            Saxon.Hej.tree.iter.ManualIterator si = new Saxon.Hej.tree.iter.ManualIterator(item);
            c2.setCurrentIterator(si);
            c2.setCurrentOutputUri(null);
            try {
                return getPredicate().effectiveBooleanValue(c2);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException.Circularity || e is Saxon.Hej.trans.XPathException.StackOverflow) {
                throw;
            } catch (Saxon.Hej.trans.XPathException ex) {
                handleDynamicError(ex, c2);
                return false;
            }
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return (getBasePattern().matchesBeneathAnchor(node, anchor, context) && matchesPredicate(node, context));
        }
        public override Saxon.Hej.type.UType getUType() {
            return getBasePattern().getUType();
        }
        public override int getFingerprint() {
            return getBasePattern().getFingerprint();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBasePattern().getItemType();
        }
        public override int getDependencies() {
            return getPredicate().getDependencies();
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            basePatternOp.setChildExpression(getBasePattern().typeCheck(visitor, contextItemType));
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(getBasePattern().getItemType(), false);
            predicateOp.setChildExpression(getPredicate().typeCheck(visitor, cit));
            return this;
        }
        public override Saxon.Hej.pattern.Pattern optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            basePatternOp.setChildExpression(getBasePattern().optimize(visitor, contextInfo));
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(getBasePattern().getItemType(), false);
            predicateOp.setChildExpression(getPredicate().optimize(visitor, cit));
            predicateOp.setChildExpression(visitor.obtainOptimizer().eliminateCommonSubexpressions(getPredicate()));
            return this;
        }
        public override Saxon.Hej.pattern.Pattern convertToTypedPattern(string val) {
            Saxon.Hej.pattern.Pattern b2 = getBasePattern().convertToTypedPattern(val);
            if ((b2 == getBasePattern())) {
                return this;
            } else {
                return new Saxon.Hej.pattern.BasePatternWithPredicate(b2, getPredicate());
            }
        }
        public override bool isMotionless() {
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE cio = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(getConfiguration().makeContextItemStaticInfo(getItemType(), false));
            cio.setContextPostureStriding();
            /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(getPredicate(), cio, new System.Collections.Generic.List<string>());
            return (getBasePattern().isMotionless() && (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(getPredicate()) == Saxon.Eej.ee.stream.Sweep.MOTIONLESS));
        }
        public override string reconstruct() {
            return (((getBasePattern()+"[")+getPredicate())+"]");
        }
        public override string toShortString() {
            return (((getBasePattern().toShortString()+"[")+getPredicate().toShortString())+"]");
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.BasePatternWithPredicate n = new Saxon.Hej.pattern.BasePatternWithPredicate(getBasePattern().copy(rebindings), getPredicate().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Hej.pattern.BasePatternWithPredicate) && ((Saxon.Hej.pattern.BasePatternWithPredicate)(obj)).getBasePattern().isEqual(getBasePattern())) && ((Saxon.Hej.pattern.BasePatternWithPredicate)(obj)).getPredicate().isEqual(getPredicate()));
        }
        protected internal override int computeHashCode() {
            return (getBasePattern().GetHashCode()^getPredicate().GetHashCode());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.withPredicate");
            getBasePattern().export(presenter);
            getPredicate().export(presenter);
            presenter.endElement();
        }
    
    }
}