using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class CastExpression : Saxon.Hej.expr.CastingExpression, Saxon.Hej.expr.Callable {
        public CastExpression(Saxon.Hej.expr.Expression source, Saxon.Hej.type.AtomicType target, bool allowEmpty) : base (source, target, allowEmpty) {
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.value.SequenceType atomicType = Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, getCardinality());
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.TYPE_OP, "cast as", 0);
            Saxon.Hej.type.ItemType sourceItemType;
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.expr.Expression operand = tc.staticTypeCheck(getBaseExpression(), atomicType, role, visitor);
            setBaseExpression(operand);
            sourceItemType = operand.getItemType();
            if ((sourceItemType is Saxon.Hej.type.ErrorType)) {
                if (allowsEmpty()) {
                    return Saxon.Hej.expr.Literal.makeEmptySequence();
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cast does not allow an empty sequence as input");
                    err.setErrorCode("XPTY0004");
                    err.setLocation(getLocation());
                    err.setIsTypeError(true);
                    throw err;
                }
            }
            Saxon.Hej.type.PlainType sourceType = (Saxon.Hej.type.PlainType)(sourceItemType);
            Saxon.Hej.type.Affinity r = th.relationship(sourceType, getTargetType());
            if ((r == Saxon.Hej.type.Affinity.SAME_TYPE)) {
                return operand;
            } else {
                if ((r == Saxon.Hej.type.Affinity.SUBSUMED_BY)) {
                    converter = new Saxon.Hej.type.Converter.UpCastingConverter(getTargetType());
                } else {
                    Saxon.Hej.lib.ConversionRules rules = visitor.getConfiguration().getConversionRules();
                    if ((sourceType.isAtomicType() && (sourceType != Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))) {
                        converter = rules.getConverter((Saxon.Hej.type.AtomicType)(sourceType), getTargetType());
                        if ((converter == null)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Casting from "+sourceType)+" to ")+getTargetType())+" can never succeed"));
                            err.setErrorCode("XPTY0004");
                            err.setLocation(getLocation());
                            err.setIsTypeError(true);
                            throw err;
                        } else {
                            if (getTargetType().isNamespaceSensitive()) {
                                converter = converter.setNamespaceResolver(getRetainedStaticContext());
                            }
                        }
                    }
                }
            }
            if ((operand is Saxon.Hej.expr.Literal)) {
                return preEvaluate();
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.Expression e2 = base.optimize(visitor, contextInfo);
            if ((e2 != this)) {
                return e2;
            }
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            if ((getTargetType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                if (operand.isCallOn(typeof(Saxon.Hej.functions.String_1))) {
                    Saxon.Hej.expr.Expression e = ((Saxon.Hej.expr.SystemFunctionCall)(operand)).getArg(0);
                    if (((e.getItemType() is Saxon.Hej.type.AtomicType) && (e.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                        return new Saxon.Hej.expr.CastExpression(e, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, allowsEmpty());
                    }
                } else {
                    if ((operand is Saxon.Hej.expr.CastExpression)) {
                        if ((((Saxon.Hej.expr.CastExpression)(operand)).getTargetType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                            return operand;
                        } else {
                            if ((((Saxon.Hej.expr.CastExpression)(operand)).getTargetType() == Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                                ((Saxon.Hej.expr.CastExpression)(operand)).setTargetType(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                                return operand;
                            }
                        }
                    } else {
                        if ((operand is Saxon.Hej.expr.AtomicSequenceConverter)) {
                            if ((operand.getItemType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                return operand;
                            } else {
                                if ((operand.getItemType() == Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                                    Saxon.Hej.expr.AtomicSequenceConverter old = (Saxon.Hej.expr.AtomicSequenceConverter)(operand);
                                    Saxon.Hej.expr.AtomicSequenceConverter asc = new Saxon.Hej.expr.AtomicSequenceConverter(old.getBaseExpression(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                                    return asc.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                                }
                            }
                        }
                    }
                }
            }
            if (operand.isCallOn(typeof(Saxon.Hej.functions.String_1))) {
                Saxon.Hej.expr.Expression e = ((Saxon.Hej.expr.SystemFunctionCall)(operand)).getArg(0);
                Saxon.Hej.type.ItemType et = e.getItemType();
                if ((((et is Saxon.Hej.type.AtomicType) && (e.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && th.isSubType(et, getTargetType()))) {
                    return e;
                }
            }
            if ((operand is Saxon.Hej.expr.CastExpression)) {
                Saxon.Hej.type.ItemType it = ((Saxon.Hej.expr.CastExpression)(operand)).getTargetType();
                if ((th.isSubType(it, Saxon.Hej.type.BuiltInAtomicType.STRING) || th.isSubType(it, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                    Saxon.Hej.expr.Expression e = ((Saxon.Hej.expr.CastExpression)(operand)).getBaseExpression();
                    Saxon.Hej.type.ItemType et = e.getItemType();
                    if ((((et is Saxon.Hej.type.AtomicType) && (e.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && th.isSubType(et, getTargetType()))) {
                        return e;
                    }
                }
            }
            if ((operand is Saxon.Hej.expr.AtomicSequenceConverter)) {
                Saxon.Hej.type.ItemType it = operand.getItemType();
                if ((th.isSubType(it, Saxon.Hej.type.BuiltInAtomicType.STRING) || th.isSubType(it, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                    Saxon.Hej.expr.Expression e = ((Saxon.Hej.expr.AtomicSequenceConverter)(operand)).getBaseExpression();
                    Saxon.Hej.type.ItemType et = e.getItemType();
                    if ((((et is Saxon.Hej.type.AtomicType) && (e.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && th.isSubType(et, getTargetType()))) {
                        return e;
                    }
                }
            }
            if (!(Saxon.Hej.value.Cardinality.allowsZero(operand.getCardinality()))) {
                setAllowEmpty(false);
                resetLocalStaticProperties();
            }
            if ((operand is Saxon.Hej.expr.Literal)) {
                return preEvaluate();
            }
            return this;
        }
        protected internal Saxon.Hej.expr.Expression preEvaluate() {
            Saxon.Hej.om.GroundedValue literalOperand = ((Saxon.Hej.expr.Literal)(getBaseExpression())).getGroundedValue();
            if (((literalOperand is Saxon.Hej.value.AtomicValue) && (converter != null))) {
                Saxon.Hej.type.ConversionResult result = converter.convert((Saxon.Hej.value.AtomicValue)(literalOperand));
                if ((result is Saxon.Hej.type.ValidationFailure)) {
                    Saxon.Hej.type.ValidationFailure err = (Saxon.Hej.type.ValidationFailure)(result);
                    string code = err.getErrorCode();
                    if ((code == null)) {
                        code = "FORG0001";
                    }
                    throw new Saxon.Hej.trans.XPathException(err.getMessage(), code, this.getLocation());
                } else {
                    return Saxon.Hej.expr.Literal.makeLiteral((Saxon.Hej.value.AtomicValue)(result), this);
                }
            }
            if ((literalOperand.getLength() == 0)) {
                if (allowsEmpty()) {
                    return getBaseExpression();
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cast can never succeed: the operand must not be an empty sequence", "XPTY0004", this.getLocation());
                    err.setIsTypeError(true);
                    throw err;
                }
            }
            return this;
        }
        protected internal override int computeCardinality() {
            return ((allowsEmpty() && Saxon.Hej.value.Cardinality.allowsZero(getBaseExpression().getCardinality())) ? Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE : Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getTargetType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getTargetType().getUType();
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            if ((getTargetType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                p = (p&~(Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC));
            }
            return p;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            if ((converter == Saxon.Hej.type.Converter.BooleanToInteger.INSTANCE)) {
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.ZERO, Saxon.Hej.value.Int64Value.PLUS_ONE};
            } else {
                return null;
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.CastExpression c2 = new Saxon.Hej.expr.CastExpression(getBaseExpression().copy(rebindings), getTargetType(), allowsEmpty());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c2);
            c2.converter = converter;
            c2.setRetainedStaticContext(getRetainedStaticContext());
            c2.setOperandIsStringLiteral(isOperandIsStringLiteral());
            return c2;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue result = doCast((Saxon.Hej.value.AtomicValue)(arguments[0].head()), context);
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(result);
        }
        private Saxon.Hej.value.AtomicValue doCast(Saxon.Hej.value.AtomicValue value, Saxon.Hej.expr.XPathContext context) {
            if ((value == null)) {
                if (allowsEmpty()) {
                    return null;
                } else {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Cast does not allow an empty sequence");
                    e.setXPathContext(context);
                    e.setLocation(getLocation());
                    e.setErrorCode("XPTY0004");
                    throw e;
                }
            }
            Saxon.Hej.type.Converter converter = this.converter;
            if ((converter == null)) {
                Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
                converter = rules.getConverter(value.getPrimitiveType(), getTargetType());
                if ((converter == null)) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((((("Casting from "+value.getPrimitiveType())+" to ")+getTargetType())+" is not permitted"));
                    e.setXPathContext(context);
                    e.setLocation(getLocation());
                    e.setErrorCode("XPTY0004");
                    throw e;
                }
                if (getTargetType().isNamespaceSensitive()) {
                    converter = converter.setNamespaceResolver(getRetainedStaticContext());
                }
            }
            Saxon.Hej.type.ConversionResult result = converter.convert(value);
            if ((result is Saxon.Hej.type.ValidationFailure)) {
                Saxon.Hej.type.ValidationFailure err = (Saxon.Hej.type.ValidationFailure)(result);
                Saxon.Hej.trans.XPathException xe = err.makeException();
                xe.maybeSetErrorCode("FORG0001");
                xe.maybeSetLocation(getLocation());
                throw xe;
            }
            return (Saxon.Hej.value.AtomicValue)(result);
        }
        public override Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(getBaseExpression().evaluateItem(context));
                return doCast(value, context);
            } catch (System.InvalidCastException e) {
                System.Console.Error.WriteLine(e);
                throw;
            }
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.expr.CastExpression) && getBaseExpression().isEqual(((Saxon.Hej.expr.CastExpression)(other)).getBaseExpression())) && (getTargetType() == ((Saxon.Hej.expr.CastExpression)(other)).getTargetType())) && (allowsEmpty() == ((Saxon.Hej.expr.CastExpression)(other)).allowsEmpty()));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^getTargetType().GetHashCode());
        }
        public override string ToString() {
            return (((getTargetType().getEQName()+"(")+getBaseExpression().ToString())+")");
        }
        public override string toShortString() {
            return (((getTargetType().getDisplayName()+"(")+getBaseExpression().toShortString())+")");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            export(@out, "cast");
        }
        public override string getExpressionName() {
            return "cast";
        }
    
    }
}