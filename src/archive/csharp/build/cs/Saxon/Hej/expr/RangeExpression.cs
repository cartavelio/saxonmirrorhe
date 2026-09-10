using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class RangeExpression : Saxon.Hej.expr.Expression {
        private Saxon.Hej.expr.Operand start;
        private Saxon.Hej.expr.Operand step;
        private Saxon.Hej.expr.Operand end;
        private bool defaultedStep;
        public static Saxon.Hej.expr.RangeExpression makeSteppingExpression(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs) {
            if (!((((lhs is Saxon.Hej.expr.RangeExpression) && ((Saxon.Hej.expr.RangeExpression)(lhs)).defaultedStep)))) {
                throw new Saxon.Hej.trans.XPathException("Temporary restriction: 'by' operator must be preceded by 'x to y'");
            }
            return new Saxon.Hej.expr.RangeExpression(((Saxon.Hej.expr.RangeExpression)(lhs)).getStartExpression(), rhs, ((Saxon.Hej.expr.RangeExpression)(lhs)).getEndExpression());
        }
        public bool isDefaultedStep() {
            return defaultedStep;
        }
        public RangeExpression(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression end) : this (start, new Saxon.Hej.expr.Literal(Saxon.Hej.value.Int64Value.PLUS_ONE), end) {
            defaultedStep = true;
        }
        public RangeExpression(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression step, Saxon.Hej.expr.Expression end)  {
            this.start = new Saxon.Hej.expr.Operand(this, start, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            this.step = new Saxon.Hej.expr.Operand(this, step, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            this.end = new Saxon.Hej.expr.Operand(this, end, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            adoptChildExpression(start);
            adoptChildExpression(step);
            adoptChildExpression(end);
        }
        public Saxon.Hej.expr.Expression getStartExpression() {
            return start.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getStepExpression() {
            return step.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getEndExpression() {
            return end.getChildExpression();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            start.typeCheck(visitor, contextInfo);
            step.typeCheck(visitor, contextInfo);
            end.typeCheck(visitor, contextInfo);
            bool backCompat = visitor.getStaticContext().isInBackwardsCompatibleMode();
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(backCompat);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role0 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "to", 0);
            start.setChildExpression(tc.staticTypeCheck(getStartExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_INTEGER, role0, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role1 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "by", 1);
            step.setChildExpression(tc.staticTypeCheck(getStepExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_INTEGER, role1, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role2 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "to", 1);
            end.setChildExpression(tc.staticTypeCheck(getEndExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_INTEGER, role2, visitor));
            return makeConstantRange();
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            start.optimize(visitor, contextInfo);
            step.optimize(visitor, contextInfo);
            end.optimize(visitor, contextInfo);
            return makeConstantRange();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(start, step, end);
        }
        private Saxon.Hej.expr.Expression makeConstantRange() {
            if ((((getStartExpression() is Saxon.Hej.expr.Literal) && (getStepExpression() is Saxon.Hej.expr.Literal)) && (getEndExpression() is Saxon.Hej.expr.Literal))) {
                Saxon.Hej.expr.Expression result;
                Saxon.Hej.om.GroundedValue v0 = ((Saxon.Hej.expr.Literal)(getStartExpression())).getGroundedValue();
                Saxon.Hej.om.GroundedValue v1 = ((Saxon.Hej.expr.Literal)(getStepExpression())).getGroundedValue();
                Saxon.Hej.om.GroundedValue v2 = ((Saxon.Hej.expr.Literal)(getEndExpression())).getGroundedValue();
                if ((((v0.getLength() == 0) || (v1.getLength() == 0)) || (v2.getLength() == 0))) {
                    result = Saxon.Hej.expr.Literal.makeEmptySequence();
                } else {
                    if ((((v0 is Saxon.Hej.value.Int64Value) && (v1 is Saxon.Hej.value.Int64Value)) && (v2 is Saxon.Hej.value.Int64Value))) {
                        long i0 = ((Saxon.Hej.value.Int64Value)(v0)).longValue();
                        long i1 = ((Saxon.Hej.value.Int64Value)(v1)).longValue();
                        long i2 = ((Saxon.Hej.value.Int64Value)(v2)).longValue();
                        if (((i1 == 0) || (i0>i2))) {
                            result = Saxon.Hej.expr.Literal.makeEmptySequence();
                        } else {
                            if ((System.Math.Abs((((i2-i0))/i1))>System.Int32.MaxValue)) {
                                throw new Saxon.Hej.trans.XPathException(("Maximum length of sequence in Saxon is "+System.Int32.MaxValue), "XPDY0130");
                            }
                            result = Saxon.Hej.expr.Literal.makeLiteral(((i1<0) ? new Saxon.Hej.value.IntegerRange(i2, i1, i0) : new Saxon.Hej.value.IntegerRange(i0, i1, i2)), this);
                        }
                    } else {
                        System.Numerics.BigInteger i0 = ((Saxon.Hej.value.IntegerValue)(v0)).asBigInteger();
                        System.Numerics.BigInteger i1 = ((Saxon.Hej.value.IntegerValue)(v1)).asBigInteger();
                        System.Numerics.BigInteger i2 = ((Saxon.Hej.value.IntegerValue)(v2)).asBigInteger();
                        if ((i0.Equals(System.Numerics.BigInteger.Zero) || (i0.CompareTo(i2)>0))) {
                            result = Saxon.Hej.expr.Literal.makeEmptySequence();
                        } else {
                            if (i0.Equals(i2)) {
                                result = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(i0), this);
                            } else {
                                return this;
                            }
                        }
                    }
                }
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, result);
                return result;
            }
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.DECIMAL;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            Saxon.Hej.value.IntegerValue[] start = getStartExpression().getIntegerBounds();
            Saxon.Hej.value.IntegerValue[] end = getEndExpression().getIntegerBounds();
            if (((start == null) || (end == null))) {
                return null;
            } else {
                return new Saxon.Hej.value.IntegerValue[]{start[0], end[1]};
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.RangeExpression exp = new Saxon.Hej.expr.RangeExpression(getStartExpression().copy(rebindings), getStepExpression().copy(rebindings), getEndExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override string getExpressionName() {
            return "range";
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override bool Equals(System.Object other) {
            if (((other is Saxon.Hej.expr.RangeExpression) && hasCompatibleStaticContext((Saxon.Hej.expr.Expression)(other)))) {
                Saxon.Hej.expr.RangeExpression b = (Saxon.Hej.expr.RangeExpression)(other);
                Saxon.Hej.expr.Expression start1 = getStartExpression();
                Saxon.Hej.expr.Expression step1 = getStepExpression();
                Saxon.Hej.expr.Expression end1 = getEndExpression();
                Saxon.Hej.expr.Expression start2 = b.getStartExpression();
                Saxon.Hej.expr.Expression step2 = b.getStepExpression();
                Saxon.Hej.expr.Expression end2 = b.getEndExpression();
                return ((start1.Equals(start2) && step1.Equals(step2)) && end1.Equals(end2));
            }
            return false;
        }
        protected internal override int computeHashCode() {
            return ((getStartExpression().GetHashCode()^((getStepExpression().GetHashCode()<<3)))^((getEndExpression().GetHashCode()<<7)));
        }
        public override string ToString() {
            string by = getStepExpression().ToString();
            return (((getStartExpression().ToString()+" to ")+getEndExpression().ToString())+((by.Equals("1") ? "" : ((" by "+by)))));
        }
        public override string toShortString() {
            string by = getStepExpression().toShortString();
            return (((getStartExpression().toShortString()+" to ")+getEndExpression().toShortString())+((by.Equals("1") ? "" : ((" by "+by)))));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("to", this);
            getStartExpression().export(@out);
            getEndExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.IntegerValue av1 = (Saxon.Hej.value.IntegerValue)(getStartExpression().evaluateItem(context));
            Saxon.Hej.value.IntegerValue av2 = (Saxon.Hej.value.IntegerValue)(getStepExpression().evaluateItem(context));
            Saxon.Hej.value.IntegerValue av3 = (Saxon.Hej.value.IntegerValue)(getEndExpression().evaluateItem(context));
            return Saxon.Hej.expr.AscendingRangeIterator.makeRangeIterator(av1, av2, av3);
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.RangeExpression.RangeElaborator();
        }
        public class RangeElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.RangeExpression expr = (Saxon.Hej.expr.RangeExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator iv1 = expr.getStartExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator iv2 = expr.getStepExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator iv3 = expr.getEndExpression().makeElaborator().elaborateForItem();
                return (context) => {
                    Saxon.Hej.value.IntegerValue av1 = (Saxon.Hej.value.IntegerValue)(/*Delegate.eval()*/iv1(context));
                    Saxon.Hej.value.IntegerValue av2 = (Saxon.Hej.value.IntegerValue)(/*Delegate.eval()*/iv2(context));
                    Saxon.Hej.value.IntegerValue av3 = (Saxon.Hej.value.IntegerValue)(/*Delegate.eval()*/iv3(context));
                    return Saxon.Hej.expr.AscendingRangeIterator.makeRangeIterator(av1, av2, av3);
                } ;
            }
        
        }
    
    }
}