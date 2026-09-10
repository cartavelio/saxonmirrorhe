using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ArithmeticExpression : Saxon.Hej.expr.BinaryExpression {
        protected internal Saxon.Hej.expr.Calculator calculator;
        private Saxon.Hej.type.PlainType itemType;
        public ArithmeticExpression(Saxon.Hej.expr.Expression p0, int @operator, Saxon.Hej.expr.Expression p1) : base (p0, @operator, p1) {
        }
        public override string getExpressionName() {
            return "arithmetic";
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC);
        }
        public virtual void setCalculator(Saxon.Hej.expr.Calculator calculator) {
            this.calculator = calculator;
        }
        public Saxon.Hej.expr.Calculator getCalculator() {
            return calculator;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            resetLocalStaticProperties();
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.expr.Expression oldOp0 = getLhsExpression();
            Saxon.Hej.expr.Expression oldOp1 = getRhsExpression();
            Saxon.Hej.value.SequenceType atomicType = Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC;
            Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 0);
            setLhsExpression(tc.staticTypeCheck(getLhsExpression(), atomicType, role0, visitor));
            Saxon.Hej.type.ItemType itemType0 = getLhsExpression().getItemType();
            if ((itemType0 is Saxon.Hej.type.ErrorType)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.type.AtomicType type0 = (Saxon.Hej.type.AtomicType)(itemType0.getPrimitiveItemType());
            if ((type0.getFingerprint() == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                setLhsExpression(Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(config, getLhsExpression(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE));
                type0 = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            } else {
                if (((((getLhsExpression().getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC)) == 0) && (th.relationship(type0, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT))) {
                    setLhsExpression(Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(config, getLhsExpression(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE));
                    type0 = (Saxon.Hej.type.AtomicType)(getLhsExpression().getItemType().getPrimitiveItemType());
                }
            }
            Saxon.Hej.expr.parser.RoleDiagnostic role1 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 1);
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), atomicType, role1, visitor));
            Saxon.Hej.type.ItemType itemType1 = getRhsExpression().getItemType();
            if ((itemType1 is Saxon.Hej.type.ErrorType)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.type.AtomicType type1 = (Saxon.Hej.type.AtomicType)(itemType1.getPrimitiveItemType());
            if ((type1.getFingerprint() == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                setRhsExpression(Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(config, getRhsExpression(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE));
                type1 = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            } else {
                if (((((getRhsExpression().getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC)) == 0) && (th.relationship(type1, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT))) {
                    setRhsExpression(Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(config, getRhsExpression(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE));
                    type1 = (Saxon.Hej.type.AtomicType)(getRhsExpression().getItemType().getPrimitiveItemType());
                }
            }
            if (itemType0.getUType().union(itemType1.getUType()).overlaps(Saxon.Hej.type.UType.EXTENSION)) {
                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Arithmetic operators are not defined for external objects");
                de.setLocation(getLocation());
                de.setErrorCode("XPTY0004");
                throw de;
            }
            if ((getLhsExpression() != oldOp0)) {
                adoptChildExpression(getLhsExpression());
            }
            if ((getRhsExpression() != oldOp1)) {
                adoptChildExpression(getRhsExpression());
            }
            if ((Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression()))) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.NEGATE)) {
                if (((getRhsExpression() is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(getRhsExpression())).getGroundedValue() is Saxon.Hej.value.NumericValue))) {
                    Saxon.Hej.value.NumericValue nv = (Saxon.Hej.value.NumericValue)(((Saxon.Hej.expr.Literal)(getRhsExpression())).getGroundedValue());
                    return Saxon.Hej.expr.Literal.makeLiteral(nv.negate(), this);
                } else {
                    Saxon.Hej.expr.NegateExpression ne = new Saxon.Hej.expr.NegateExpression(getRhsExpression());
                    ne.setBackwardsCompatible(false);
                    return ne.typeCheck(visitor, contextInfo);
                }
            }
            bool mustResolve = !(((((type0.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || type1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) || type0.Equals(Saxon.Hej.type.NumericType.getInstance())) || type1.Equals(Saxon.Hej.type.NumericType.getInstance()))));
            calculator = Saxon.Hej.expr.Calculator.getCalculator(type0.getFingerprint(), type1.getFingerprint(), mapOpCode(@operator), mustResolve);
            if ((calculator == null)) {
                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((((("Arithmetic operator is not defined for arguments of types ("+type0.getDescription())+", ")+type1.getDescription())+")"));
                de.setLocation(getLocation());
                de.setIsTypeError(true);
                de.setErrorCode("XPTY0004");
                throw de;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(calculator.code(), "d.d")) {
                if (((getLhsExpression() is Saxon.Hej.expr.Literal) && !(type0.Equals(Saxon.Hej.type.BuiltInAtomicType.DOUBLE)))) {
                    Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getLhsExpression())).getGroundedValue();
                    if ((value is Saxon.Hej.value.NumericValue)) {
                        setLhsExpression(Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(value)).getDoubleValue()), this));
                    }
                }
                if (((getRhsExpression() is Saxon.Hej.expr.Literal) && !(type1.Equals(Saxon.Hej.type.BuiltInAtomicType.DOUBLE)))) {
                    Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(getRhsExpression())).getGroundedValue();
                    if ((value is Saxon.Hej.value.NumericValue)) {
                        setRhsExpression(Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(value)).getDoubleValue()), this));
                    }
                }
            }
            try {
                if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                    return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext()).materialize(), this);
                }
            } catch (Saxon.Hej.trans.XPathException err) {
            }
            return this;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            Saxon.Hej.value.IntegerValue[] bounds0 = getLhsExpression().getIntegerBounds();
            Saxon.Hej.value.IntegerValue[] bounds1 = getRhsExpression().getIntegerBounds();
            if (((bounds0 == null) || (bounds1 == null))) {
                return null;
            } else {
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.PLUS:
                    return new Saxon.Hej.value.IntegerValue[]{bounds0[0].plus(bounds1[0]), bounds0[1].plus(bounds1[1])};
                  case Saxon.Hej.expr.parser.Token.MINUS:
                    return new Saxon.Hej.value.IntegerValue[]{bounds0[0].minus(bounds1[1]), bounds0[1].minus(bounds1[0])};
                  case Saxon.Hej.expr.parser.Token.MULT:
                    if ((getRhsExpression() is Saxon.Hej.expr.Literal)) {
                        Saxon.Hej.value.IntegerValue val1 = bounds1[0];
                        if ((val1.signum()>0)) {
                            return new Saxon.Hej.value.IntegerValue[]{bounds0[0].times(val1), bounds0[1].times(val1)};
                        } else {
                            return null;
                        }
                    } else {
                        if ((getLhsExpression() is Saxon.Hej.expr.Literal)) {
                            Saxon.Hej.value.IntegerValue val0 = bounds1[0];
                            if ((val0.signum()>0)) {
                                return new Saxon.Hej.value.IntegerValue[]{bounds1[0].times(val0), bounds1[1].times(val0)};
                            } else {
                                return null;
                            }
                        }
                    }
                    return null;
                  case Saxon.Hej.expr.parser.Token.DIV:
                  case Saxon.Hej.expr.parser.Token.IDIV:
                    if ((getRhsExpression() is Saxon.Hej.expr.Literal)) {
                        Saxon.Hej.value.IntegerValue val1 = bounds1[0];
                        if ((val1.signum()>0)) {
                            try {
                                return new Saxon.Hej.value.IntegerValue[]{bounds0[0].idiv(val1), bounds0[1].idiv(val1)};
                            } catch (Saxon.Hej.trans.XPathException e) {
                                return null;
                            }
                        }
                    }
                    return null;
                  default:
                    return null;
                }
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ArithmeticExpression ae = new Saxon.Hej.expr.ArithmeticExpression(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ae);
            ae.calculator = calculator;
            return ae;
        }
        public static Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue value0, int @operator, Saxon.Hej.value.AtomicValue value1, Saxon.Hej.expr.XPathContext context) {
            int p0 = value0.getPrimitiveType().getFingerprint();
            int p1 = value1.getPrimitiveType().getFingerprint();
            Saxon.Hej.expr.Calculator calculator = Saxon.Hej.expr.Calculator.getCalculator(p0, p1, @operator, false);
            return calculator.compute(value0, value1, context);
        }
        public static int mapOpCode(int op) {
            switch (op) {
              case Saxon.Hej.expr.parser.Token.PLUS:
                return Saxon.Hej.expr.Calculator.PLUS;
              case Saxon.Hej.expr.parser.Token.MINUS:
              case Saxon.Hej.expr.parser.Token.NEGATE:
                return Saxon.Hej.expr.Calculator.MINUS;
              case Saxon.Hej.expr.parser.Token.MULT:
                return Saxon.Hej.expr.Calculator.TIMES;
              case Saxon.Hej.expr.parser.Token.DIV:
                return Saxon.Hej.expr.Calculator.DIV;
              case Saxon.Hej.expr.parser.Token.IDIV:
                return Saxon.Hej.expr.Calculator.IDIV;
              case Saxon.Hej.expr.parser.Token.MOD:
                return Saxon.Hej.expr.Calculator.MOD;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public override Saxon.Hej.type.PlainType getItemType() {
            if ((itemType != null)) {
                return itemType;
            }
            if ((calculator == null)) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            } else {
                Saxon.Hej.type.ItemType t1 = getLhsExpression().getItemType();
                if (!(((t1 is Saxon.Hej.type.AtomicType)))) {
                    t1 = t1.getAtomizedItemType();
                }
                Saxon.Hej.type.ItemType t2 = getRhsExpression().getItemType();
                if (!(((t2 is Saxon.Hej.type.AtomicType)))) {
                    t2 = t2.getAtomizedItemType();
                }
                Saxon.Hej.type.PlainType resultType = calculator.getResultType((Saxon.Hej.type.AtomicType)(t1.getPrimitiveItemType()), (Saxon.Hej.type.AtomicType)(t2.getPrimitiveItemType()));
                if (resultType.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                    Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                    if (((((@operator == Saxon.Hej.expr.parser.Token.PLUS) || (@operator == Saxon.Hej.expr.parser.Token.MINUS))) && ((Saxon.Hej.type.NumericType.isNumericType(t2) || Saxon.Hej.type.NumericType.isNumericType(t1))))) {
                        resultType = Saxon.Hej.type.NumericType.getInstance();
                    }
                }
                return itemType = resultType;
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if (((getParentExpression() is Saxon.Hej.expr.FilterExpression) && (((Saxon.Hej.expr.FilterExpression)(getParentExpression())).getRhsExpression() == this))) {
                return Saxon.Hej.type.UType.NUMERIC;
            } else {
                if ((@operator == Saxon.Hej.expr.parser.Token.NEGATE)) {
                    return Saxon.Hej.type.UType.NUMERIC;
                } else {
                    return Saxon.Hej.type.UType.ANY_ATOMIC;
                }
            }
        }
        public override void resetLocalStaticProperties() {
            base.resetLocalStaticProperties();
            itemType = null;
        }
        public override Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue v0 = (Saxon.Hej.value.AtomicValue)(getLhsExpression().evaluateItem(context));
            if ((v0 == null)) {
                return null;
            }
            Saxon.Hej.value.AtomicValue v1 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
            if ((v1 == null)) {
                return null;
            }
            try {
                return calculator.compute(v0, v1, context);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetFailingExpression(this);
                e.maybeSetContext(context);
                throw;
            }
        }
        protected internal override string tag() {
            return "arith";
        }
        protected internal override void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            if ((calculator != null)) {
                @out.emitAttribute("calc", calculator.code());
            }
        }
    
    }
}