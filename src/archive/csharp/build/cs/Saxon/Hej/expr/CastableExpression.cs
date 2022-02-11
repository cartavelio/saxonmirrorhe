using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class CastableExpression : Saxon.Hej.expr.CastingExpression {
        public CastableExpression(Saxon.Hej.expr.Expression source, Saxon.Hej.type.AtomicType target, bool allowEmpty) : base (source, target, allowEmpty) {
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.value.SequenceType atomicType = Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE;
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.TYPE_OP, "castable as", 0);
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.expr.Expression operand = tc.staticTypeCheck(getBaseExpression(), atomicType, role, visitor);
            setBaseExpression(operand);
            if ((operand is Saxon.Hej.expr.Literal)) {
                return preEvaluate();
            }
            return this;
        }
        private Saxon.Hej.expr.Expression preEvaluate() {
            Saxon.Hej.om.GroundedValue literalOperand = ((Saxon.Hej.expr.Literal)(getBaseExpression())).getGroundedValue();
            if (((literalOperand is Saxon.Hej.value.AtomicValue) && (converter != null))) {
                Saxon.Hej.type.ConversionResult result = converter.convert((Saxon.Hej.value.AtomicValue)(literalOperand));
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(!(((result is Saxon.Hej.type.ValidationFailure)))), this);
            }
            int length = literalOperand.getLength();
            if ((length == 0)) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(allowsEmpty()), this);
            }
            if ((length>1)) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            optimizeChildren(visitor, contextInfo);
            if ((getBaseExpression() is Saxon.Hej.expr.Literal)) {
                return preEvaluate();
            }
            return this;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.expr.CastableExpression) && getBaseExpression().isEqual(((Saxon.Hej.expr.CastableExpression)(other)).getBaseExpression())) && (getTargetType() == ((Saxon.Hej.expr.CastableExpression)(other)).getTargetType())) && (allowsEmpty() == ((Saxon.Hej.expr.CastableExpression)(other)).allowsEmpty()));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^0x5555);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.CastableExpression ce = new Saxon.Hej.expr.CastableExpression(getBaseExpression().copy(rebindings), getTargetType(), allowsEmpty());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ce);
            ce.setRetainedStaticContext(getRetainedStaticContext());
            ce.converter = converter;
            return ce;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            int count = 0;
            Saxon.Hej.om.SequenceIterator iter = getBaseExpression().iterate(context);
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    Saxon.Hej.om.AtomicSequence atomizedValue = item.atomize();
                    int length = Saxon.Hej.om.SequenceTool.getLength(atomizedValue);
                    count += length;
                    if ((count>1)) {
                        return false;
                    }
                    if ((length != 0)) {
                        Saxon.Hej.value.AtomicValue av = ((/*1430*/Saxon.Hej.value.AtomicValue)atomizedValue.head());
                        if (!(isCastable(av, getTargetType(), context))) {
                            return false;
                        }
                    }
                } else {
                    if ((item is Saxon.Hej.value.AtomicValue)) {
                        Saxon.Hej.value.AtomicValue av = (Saxon.Hej.value.AtomicValue)(item);
                        count++;
                        if ((count>1)) {
                            return false;
                        }
                        if (!(isCastable(av, getTargetType(), context))) {
                            return false;
                        }
                    } else {
                        if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                            return false;
                        } else {
                            throw new Saxon.Hej.trans.XPathException("Input to cast cannot be atomized", "XPTY0004");
                        }
                    }
                }
            }
            return ((count != 0) || allowsEmpty());
        }
        private bool isCastable(Saxon.Hej.value.AtomicValue value, Saxon.Hej.type.AtomicType targetType, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.Converter converter = this.converter;
            if ((converter == null)) {
                converter = context.getConfiguration().getConversionRules().getConverter(value.getPrimitiveType(), targetType);
                if ((converter == null)) {
                    return false;
                }
                if (converter.isAlwaysSuccessful()) {
                    return true;
                }
                if (getTargetType().isNamespaceSensitive()) {
                    converter = converter.setNamespaceResolver(getRetainedStaticContext());
                }
            }
            return !(((converter.convert(value) is Saxon.Hej.type.ValidationFailure)));
        }
        public override string getExpressionName() {
            return "castable";
        }
        public override string ToString() {
            return ((getBaseExpression().ToString()+" castable as ")+getTargetType().getEQName());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            export(@out, "castable");
        }
    
    }
}