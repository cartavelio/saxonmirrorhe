using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class IntegerRangeTest : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand valueOp;
        private readonly Saxon.Hej.expr.Operand minOp;
        private readonly Saxon.Hej.expr.Operand maxOp;
        public IntegerRangeTest(Saxon.Hej.expr.Expression value, Saxon.Hej.expr.Expression min, Saxon.Hej.expr.Expression max)  {
            valueOp = new Saxon.Hej.expr.Operand(this, value, Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE);
            minOp = new Saxon.Hej.expr.Operand(this, min, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            maxOp = new Saxon.Hej.expr.Operand(this, max, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(valueOp, minOp, maxOp);
        }
        public Saxon.Hej.expr.Expression getValue() {
            return valueOp.getChildExpression();
        }
        public void setValue(Saxon.Hej.expr.Expression value) {
            valueOp.setChildExpression(value);
        }
        public Saxon.Hej.expr.Expression getMin() {
            return minOp.getChildExpression();
        }
        public void setMin(Saxon.Hej.expr.Expression min) {
            minOp.setChildExpression(min);
        }
        public Saxon.Hej.expr.Expression getMax() {
            return maxOp.getChildExpression();
        }
        public void setMax(Saxon.Hej.expr.Expression max) {
            maxOp.setChildExpression(max);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if (((Saxon.Hej.expr.Literal.isEmptySequence(getMin()) || Saxon.Hej.expr.Literal.isEmptySequence(getMax())) || Saxon.Hej.expr.Literal.isEmptySequence(getValue()))) {
                return new Saxon.Hej.expr.Literal(Saxon.Hej.value.BooleanValue.FALSE);
            }
            if ((((getMin() is Saxon.Hej.expr.Literal) && (getMax() is Saxon.Hej.expr.Literal)) && (getValue() is Saxon.Hej.expr.Literal))) {
                Saxon.Hej.value.BooleanValue result = evaluateItem(visitor.makeDynamicContext());
                return new Saxon.Hej.expr.Literal(result);
            }
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.IntegerRangeTest exp = new Saxon.Hej.expr.IntegerRangeTest(getValue().copy(rebindings), getMin().copy(rebindings), getMax().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.expr.IntegerRangeTest) && ((Saxon.Hej.expr.IntegerRangeTest)(other)).getValue().isEqual(getValue())) && ((Saxon.Hej.expr.IntegerRangeTest)(other)).getMin().isEqual(getMin())) && ((Saxon.Hej.expr.IntegerRangeTest)(other)).getMax().isEqual(getMax()));
        }
        protected internal override int computeHashCode() {
            int h = (getValue().GetHashCode()+77);
            h ^= (getMin().GetHashCode()^getMax().GetHashCode());
            return h;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.value.IntegerValue minVal = null;
            Saxon.Hej.value.IntegerValue maxVal = null;
            Saxon.Hej.type.StringConverter toDouble = null;
            Saxon.Hej.om.SequenceIterator iter = getValue().iterate(c);
            Saxon.Hej.value.AtomicValue atom;
            while (((atom = (Saxon.Hej.value.AtomicValue)(iter.next())) != null)) {
                if ((minVal == null)) {
                    minVal = (Saxon.Hej.value.IntegerValue)(getMin().evaluateItem(c));
                    if ((minVal == null)) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                    maxVal = (Saxon.Hej.value.IntegerValue)(getMax().evaluateItem(c));
                    if (((maxVal == null) || (maxVal.compareTo(minVal)<0))) {
                        return Saxon.Hej.value.BooleanValue.FALSE;
                    }
                }
                Saxon.Hej.value.NumericValue v;
                if (atom.isUntypedAtomic()) {
                    if ((toDouble == null)) {
                        toDouble = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.getStringConverter(c.getConfiguration().getConversionRules());
                    }
                    Saxon.Hej.type.ConversionResult result = toDouble.convertString(atom.getUnicodeStringValue());
                    if ((result is Saxon.Hej.type.ValidationFailure)) {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Failed to convert untypedAtomic value {"+atom.getUnicodeStringValue())+"}  to xs:integer"), "FORG0001");
                        e.setLocation(getLocation());
                        throw e;
                    } else {
                        v = (Saxon.Hej.value.DoubleValue)(result.asAtomic());
                    }
                } else {
                    if ((atom is Saxon.Hej.value.NumericValue)) {
                        v = (Saxon.Hej.value.NumericValue)(atom);
                    } else {
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Cannot compare value of type "+atom.getUType())+" to xs:integer"), "XPTY0004");
                        e.setIsTypeError(true);
                        e.setLocation(getLocation());
                        throw e;
                    }
                }
                if (((v.isWholeNumber() && (v.compareTo(minVal)>=0)) && (v.compareTo(maxVal)<=0))) {
                    return Saxon.Hej.value.BooleanValue.TRUE;
                }
            }
            return Saxon.Hej.value.BooleanValue.FALSE;
        }
        public override string getExpressionName() {
            return "intRangeTest";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("intRangeTest", this);
            getValue().export(destination);
            getMin().export(destination);
            getMax().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return (((((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getValue())+" = (")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getMin()))+" to ")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getMax()))+")");
        }
    
    }
}