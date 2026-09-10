using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class InstanceOfExpression : Saxon.Hej.expr.UnaryExpression {
        internal Saxon.Hej.type.ItemType targetType;
        internal int targetCardinality;
        public InstanceOfExpression(Saxon.Hej.expr.Expression source, Saxon.Hej.value.SequenceType target) : base (source) {
            targetType = target.getPrimaryType();
            if ((targetType == null)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Primary item type must not be null");
            }
            targetCardinality = target.getCardinality();
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return ((targetType is Saxon.Hej.pattern.DocumentNodeTest) ? Saxon.Hej.expr.OperandRole.ABSORB : Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public Saxon.Hej.type.ItemType getRequiredItemType() {
            return targetType;
        }
        public int getRequiredCardinality() {
            return targetCardinality;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            if ((operand is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext()), this);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, lit);
                return lit;
            }
            if (Saxon.Hej.value.Cardinality.subsumes(targetCardinality, operand.getCardinality())) {
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                Saxon.Hej.type.Affinity relation = th.relationship(operand.getItemType(), targetType);
                if (((relation == Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, this);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, lit);
                    return lit;
                } else {
                    if ((relation == Saxon.Hej.type.Affinity.DISJOINT)) {
                        if ((!(Saxon.Hej.value.Cardinality.allowsZero(targetCardinality)) || !(Saxon.Hej.value.Cardinality.allowsZero(operand.getCardinality())))) {
                            Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, lit);
                            return lit;
                        }
                    }
                }
            } else {
                if ((((targetCardinality&operand.getCardinality())) == 0)) {
                    Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, lit);
                    return lit;
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            if (Saxon.Hej.value.Cardinality.subsumes(targetCardinality, getBaseExpression().getCardinality())) {
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                Saxon.Hej.type.Affinity relation = th.relationship(getBaseExpression().getItemType(), targetType);
                if (((relation == Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, this);
                } else {
                    if ((relation == Saxon.Hej.type.Affinity.DISJOINT)) {
                        if ((!(Saxon.Hej.value.Cardinality.allowsZero(targetCardinality)) || !(Saxon.Hej.value.Cardinality.allowsZero(getBaseExpression().getCardinality())))) {
                            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
                        }
                    }
                }
            }
            return this;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return ((base.Equals(other) && (targetType == ((Saxon.Hej.expr.InstanceOfExpression)(other)).targetType)) && (targetCardinality == ((Saxon.Hej.expr.InstanceOfExpression)(other)).targetCardinality));
        }
        protected internal override int computeHashCode() {
            return ((base.computeHashCode()^targetType.GetHashCode())^targetCardinality);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.InstanceOfExpression exp = new Saxon.Hej.expr.InstanceOfExpression(getBaseExpression().copy(rebindings), Saxon.Hej.value.SequenceType.makeSequenceType(targetType, targetCardinality));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.eval()*/makeElaborator().elaborateForBoolean()(context);
        }
        public override string getExpressionName() {
            return "instance";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("instance", this);
            Saxon.Hej.value.SequenceType st = Saxon.Hej.value.SequenceType.makeSequenceType(targetType, targetCardinality);
            @out.emitAttribute("of", st.toAlphaCode());
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string ToString() {
            string occ = Saxon.Hej.value.Cardinality.getOccurrenceIndicator(targetCardinality);
            return ((((("("+getBaseExpression().ToString())+" instance of ")+targetType.ToString())+occ)+")");
        }
        public override string toShortString() {
            string occ = Saxon.Hej.value.Cardinality.getOccurrenceIndicator(targetCardinality);
            return (((getBaseExpression().toShortString()+" instance of ")+targetType.ToString())+occ);
        }
        public override string getStreamerName() {
            return "InstanceOf";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.InstanceOfExpression.InstanceOfElaborator();
        }
        public class InstanceOfElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.InstanceOfExpression exp = (Saxon.Hej.expr.InstanceOfExpression)(getExpression());
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                Saxon.Hej.expr.Expression arg = exp.getBaseExpression();
                int requiredCardinality = exp.getRequiredCardinality();
                bool allowsMany = Saxon.Hej.value.Cardinality.allowsMany(requiredCardinality);
                bool allowsZero = Saxon.Hej.value.Cardinality.allowsZero(requiredCardinality);
                Saxon.Hej.type.ItemType requiredType = exp.getRequiredItemType();
                if (((requiredCardinality == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && !(Saxon.Hej.value.Cardinality.allowsMany(arg.getCardinality())))) {
                    Saxon.Hej.expr.elab.ItemEvaluator itemEval = arg.makeElaborator().elaborateForItem();
                    return context => {
                        Saxon.Hej.om.Item item = /*Delegate.eval()*/itemEval(context);
                        return ((item != null) && requiredType.matches(item, th));
                    } ;
                } else {
                    Saxon.Hej.expr.elab.PullEvaluator argPull = arg.makeElaborator().elaborateForPull();
                    bool itemTypeOK = th.isSubType(arg.getItemType(), requiredType);
                    return context => {
                        Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/argPull(context);
                        int count = 0;
                        for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                            count++;
                            if ((!(itemTypeOK) && !(requiredType.matches(item, th)))) {
                                iter.close();
                                return false;
                            }
                            if ((!(allowsMany) && (count == 2))) {
                                iter.close();
                                return false;
                            }
                        }
                        return (allowsZero || (count != 0));
                    } ;
                }
            }
        
        }
    
    }
}