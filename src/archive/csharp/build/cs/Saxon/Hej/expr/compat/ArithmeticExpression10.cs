using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.compat {
    public class ArithmeticExpression10 : Saxon.Hej.expr.ArithmeticExpression, Saxon.Hej.expr.Callable {
        public ArithmeticExpression10(Saxon.Hej.expr.Expression p0, int @operator, Saxon.Hej.expr.Expression p1) : base (p0, @operator, p1) {
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression())) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.DoubleValue.NaN, this);
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression())) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.DoubleValue.NaN, this);
            }
            Saxon.Hej.expr.Expression oldOp0 = getLhsExpression();
            Saxon.Hej.expr.Expression oldOp1 = getRhsExpression();
            Saxon.Hej.value.SequenceType atomicType = Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC;
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(true);
            Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 0);
            setLhsExpression(tc.staticTypeCheck(getLhsExpression(), atomicType, role0, visitor));
            Saxon.Hej.expr.parser.RoleDiagnostic role1 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 1);
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), atomicType, role1, visitor));
            Saxon.Hej.type.ItemType itemType0 = getLhsExpression().getItemType();
            if ((itemType0 is Saxon.Hej.type.ErrorType)) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.DoubleValue.NaN, this);
            }
            Saxon.Hej.type.AtomicType type0 = (Saxon.Hej.type.AtomicType)(itemType0.getPrimitiveItemType());
            Saxon.Hej.type.ItemType itemType1 = getRhsExpression().getItemType();
            if ((itemType1 is Saxon.Hej.type.ErrorType)) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.DoubleValue.NaN, this);
            }
            Saxon.Hej.type.AtomicType type1 = (Saxon.Hej.type.AtomicType)(itemType1.getPrimitiveItemType());
            if (((th.isSubType(type0, Saxon.Hej.type.BuiltInAtomicType.INTEGER) && th.isSubType(type1, Saxon.Hej.type.BuiltInAtomicType.INTEGER)) && ((((@operator == Saxon.Hej.expr.parser.Token.PLUS) || (@operator == Saxon.Hej.expr.parser.Token.MINUS)) || (@operator == Saxon.Hej.expr.parser.Token.MULT))))) {
                Saxon.Hej.expr.ArithmeticExpression arith = new Saxon.Hej.expr.ArithmeticExpression(getLhsExpression(), @operator, getRhsExpression());
                Saxon.Hej.expr.Expression n = Saxon.Hej.functions.SystemFunction.makeCall("number", getRetainedStaticContext(), arith);
                return n.typeCheck(visitor, contextInfo);
            }
            if ((calculator == null)) {
                setLhsExpression(createConversionCode(getLhsExpression(), config, type0));
            }
            type0 = (Saxon.Hej.type.AtomicType)(getLhsExpression().getItemType().getPrimitiveItemType());
            if ((calculator == null)) {
                setRhsExpression(createConversionCode(getRhsExpression(), config, type1));
            }
            type1 = (Saxon.Hej.type.AtomicType)(getRhsExpression().getItemType().getPrimitiveItemType());
            if ((getLhsExpression() != oldOp0)) {
                adoptChildExpression(getLhsExpression());
            }
            if ((getRhsExpression() != oldOp1)) {
                adoptChildExpression(getRhsExpression());
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.NEGATE)) {
                if ((getRhsExpression() is Saxon.Hej.expr.Literal)) {
                    Saxon.Hej.om.GroundedValue v = ((Saxon.Hej.expr.Literal)(getRhsExpression())).getGroundedValue();
                    if ((v is Saxon.Hej.value.NumericValue)) {
                        return Saxon.Hej.expr.Literal.makeLiteral(((Saxon.Hej.value.NumericValue)(v)).negate(), this);
                    }
                }
                Saxon.Hej.expr.NegateExpression ne = new Saxon.Hej.expr.NegateExpression(getRhsExpression());
                ne.setBackwardsCompatible(true);
                return ne.typeCheck(visitor, contextInfo);
            }
            bool mustResolve = !(((((type0.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || type1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) || type0.Equals(Saxon.Hej.type.NumericType.getInstance())) || type1.Equals(Saxon.Hej.type.NumericType.getInstance()))));
            calculator = assignCalculator(type0, type1, mustResolve);
            try {
                if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                    return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext()).materialize(), this);
                }
            } catch (Saxon.Hej.trans.XPathException err) {
            }
            return this;
        }
        public override void setCalculator(Saxon.Hej.expr.Calculator calc) {
            this.calculator = calc;
        }
        private Saxon.Hej.expr.Calculator assignCalculator(Saxon.Hej.type.AtomicType type0, Saxon.Hej.type.AtomicType type1, bool mustResolve) {
            Saxon.Hej.expr.Calculator calculator = Saxon.Hej.expr.Calculator.getCalculator(type0.getFingerprint(), type1.getFingerprint(), Saxon.Hej.expr.ArithmeticExpression.mapOpCode(@operator), mustResolve);
            if ((calculator == null)) {
                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((((("Arithmetic operator is not defined for arguments of types ("+type0.getDescription())+", ")+type1.getDescription())+")"));
                de.setLocation(getLocation());
                de.setErrorCode("XPTY0004");
                throw de;
            }
            return calculator;
        }
        private Saxon.Hej.expr.Expression createConversionCode(Saxon.Hej.expr.Expression operand, Saxon.Hej.Configuration config, Saxon.Hej.type.AtomicType type) {
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (Saxon.Hej.value.Cardinality.allowsMany(operand.getCardinality())) {
                Saxon.Hej.expr.Expression fie = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(operand);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, fie);
                operand = fie;
            }
            if (((((th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.DOUBLE) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.DATE)) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.TIME)) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.DATE_TIME)) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.DURATION))) {
                return operand;
            }
            if (((((th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.STRING)) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.FLOAT)) || th.isSubType(type, Saxon.Hej.type.BuiltInAtomicType.DECIMAL))) {
                if ((operand is Saxon.Hej.expr.Literal)) {
                    Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.expr.Literal)(operand)).getGroundedValue();
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.functions.Number_1.convert((Saxon.Hej.value.AtomicValue)(val), config), this);
                } else {
                    return Saxon.Hej.functions.SystemFunction.makeCall("number", getRetainedStaticContext(), operand);
                }
            }
            Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
            let.setRequiredType(Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC);
            let.setVariableQName(new Saxon.Hej.om.StructuredQName("nn", Saxon.Hej.lib.NamespaceConstant.SAXON, ("nn"+let.GetHashCode())));
            let.setSequence(operand);
            Saxon.Hej.expr.LocalVariableReference var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression isDouble = new Saxon.Hej.expr.InstanceOfExpression(var, Saxon.Hej.type.BuiltInAtomicType.DOUBLE.zeroOrOne());
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression isDecimal = new Saxon.Hej.expr.InstanceOfExpression(var, Saxon.Hej.type.BuiltInAtomicType.DECIMAL.zeroOrOne());
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression isFloat = new Saxon.Hej.expr.InstanceOfExpression(var, Saxon.Hej.type.BuiltInAtomicType.FLOAT.zeroOrOne());
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression isString = new Saxon.Hej.expr.InstanceOfExpression(var, Saxon.Hej.type.BuiltInAtomicType.STRING.zeroOrOne());
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression isUntypedAtomic = new Saxon.Hej.expr.InstanceOfExpression(var, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC.zeroOrOne());
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression isBoolean = new Saxon.Hej.expr.InstanceOfExpression(var, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN.zeroOrOne());
            Saxon.Hej.expr.Expression condition = new Saxon.Hej.expr.OrExpression(isDouble, isDecimal);
            condition = new Saxon.Hej.expr.OrExpression(condition, isFloat);
            condition = new Saxon.Hej.expr.OrExpression(condition, isString);
            condition = new Saxon.Hej.expr.OrExpression(condition, isUntypedAtomic);
            condition = new Saxon.Hej.expr.OrExpression(condition, isBoolean);
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("number", getRetainedStaticContext(), var);
            var = new Saxon.Hej.expr.LocalVariableReference(let);
            var.setStaticType(Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, null, 0);
            Saxon.Hej.expr.Expression action = Saxon.Hej.expr.instruct.Choose.makeConditional(condition, fn, var);
            let.setAction(action);
            return let;
        }
        public override Saxon.Hej.type.PlainType getItemType() {
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
                return calculator.getResultType((Saxon.Hej.type.AtomicType)(t1.getPrimitiveItemType()), (Saxon.Hej.type.AtomicType)(t2.getPrimitiveItemType()));
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.compat.ArithmeticExpression10 a2 = new Saxon.Hej.expr.compat.ArithmeticExpression10(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            a2.calculator = calculator;
            return a2;
        }
        protected internal override string tag() {
            return "arith10";
        }
        protected internal override void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.emitAttribute("calc", calculator.code());
        }
        public override Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Calculator calc = calculator;
            Saxon.Hej.value.AtomicValue v1 = (Saxon.Hej.value.AtomicValue)(getLhsExpression().evaluateItem(context));
            if ((v1 == null)) {
                return Saxon.Hej.value.DoubleValue.NaN;
            }
            Saxon.Hej.value.AtomicValue v2 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
            if ((v2 == null)) {
                return Saxon.Hej.value.DoubleValue.NaN;
            }
            if ((calc == null)) {
                calc = assignCalculator(v1.getPrimitiveType(), v2.getPrimitiveType(), true);
            }
            return calc.compute(v1, v2, context);
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.Calculator calc = calculator;
            Saxon.Hej.value.AtomicValue v1 = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((v1 == null)) {
                return Saxon.Hej.value.DoubleValue.NaN;
            }
            Saxon.Hej.value.AtomicValue v2 = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
            if ((v2 == null)) {
                return Saxon.Hej.value.DoubleValue.NaN;
            }
            if ((calc == null)) {
                calc = assignCalculator(v1.getPrimitiveType(), v2.getPrimitiveType(), true);
            }
            return calc.compute(v1, v2, context);
        }
    
    }
}