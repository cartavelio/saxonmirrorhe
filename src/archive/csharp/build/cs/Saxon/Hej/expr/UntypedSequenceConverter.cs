using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class UntypedSequenceConverter : Saxon.Hej.expr.AtomicSequenceConverter {
        public UntypedSequenceConverter(Saxon.Hej.expr.Expression sequence, Saxon.Hej.type.PlainType requiredItemType) : base (sequence, requiredItemType) {
        }
        public static Saxon.Hej.expr.UntypedSequenceConverter makeUntypedSequenceConverter(Saxon.Hej.Configuration config, Saxon.Hej.expr.Expression operand, Saxon.Hej.type.PlainType requiredItemType) {
            Saxon.Hej.expr.UntypedSequenceConverter atomicSeqConverter = new Saxon.Hej.expr.UntypedSequenceConverter(operand, requiredItemType);
            Saxon.Hej.lib.ConversionRules rules = config.getConversionRules();
            Saxon.Hej.type.Converter untypedConverter;
            if (requiredItemType.isNamespaceSensitive()) {
                throw new Saxon.Hej.trans.XPathException("Cannot convert untyped atomic values to a namespace-sensitive type", "XPTY0117");
            }
            if (requiredItemType.isAtomicType()) {
                untypedConverter = rules.getConverter(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, (Saxon.Hej.type.AtomicType)(requiredItemType));
            } else {
                if ((requiredItemType == Saxon.Hej.type.NumericType.getInstance())) {
                    untypedConverter = rules.getConverter(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, Saxon.Hej.type.BuiltInAtomicType.DOUBLE);
                    atomicSeqConverter.requiredItemType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                } else {
                    untypedConverter = new Saxon.Hej.type.StringConverter.StringToUnionConverter(requiredItemType, rules);
                }
            }
            Saxon.Hej.type.Converter converter = new Saxon.Hej.expr.UntypedSequenceConverter.UntypedConverter(rules, untypedConverter);
            atomicSeqConverter.setConverter(converter);
            return atomicSeqConverter;
        }
        public class UntypedConverter : Saxon.Hej.type.Converter {
            internal Saxon.Hej.type.Converter untypedConverter = null;
            public UntypedConverter(Saxon.Hej.lib.ConversionRules rules, Saxon.Hej.type.Converter converter) : base (rules) {
                untypedConverter = converter;
            }
            public override Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                if (input.isUntypedAtomic()) {
                    return untypedConverter.convert(input);
                } else {
                    return input;
                }
            }
        
        }
        public static Saxon.Hej.expr.UntypedSequenceConverter makeUntypedSequenceRejector(Saxon.Hej.Configuration config, Saxon.Hej.expr.Expression operand, Saxon.Hej.type.PlainType requiredItemType) {
            Saxon.Hej.expr.UntypedSequenceConverter atomicSeqConverter = new Saxon.Hej.expr.UntypedSequenceConverter(operand, requiredItemType);
            Saxon.Hej.lib.ConversionRules rules = config.getConversionRules();
            Saxon.Hej.type.Converter untypedConverter = new Inner_Converter_7(requiredItemType, operand);
            Saxon.Hej.type.Converter converter = new Saxon.Hej.expr.UntypedSequenceConverter.UntypedConverter(rules, untypedConverter);
            atomicSeqConverter.setConverter(converter);
            return atomicSeqConverter;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e2 = base.typeCheck(visitor, contextInfo);
            if ((e2 != this)) {
                return e2;
            }
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.Expression @base = getBaseExpression();
            if (((th.relationship(@base.getItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) == Saxon.Hej.type.Affinity.DISJOINT) || @base.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC))) {
                return getBaseExpression();
            }
            return this;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return ((p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)|Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.UntypedSequenceConverter atomicConverter = new Saxon.Hej.expr.UntypedSequenceConverter(getBaseExpression().copy(rebindings), getRequiredItemType());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, atomicConverter);
            atomicConverter.setConverter(converter);
            atomicConverter.setRoleDiagnostic(getRoleDiagnostic());
            return atomicConverter;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((getBaseExpression().getItemType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                return getRequiredItemType();
            } else {
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                return Saxon.Hej.type.Type.getCommonSuperType(getRequiredItemType(), getBaseExpression().getItemType(), th);
            }
        }
        protected internal override int computeCardinality() {
            return getBaseExpression().getCardinality();
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.expr.UntypedSequenceConverter) && getBaseExpression().isEqual(((Saxon.Hej.expr.UntypedSequenceConverter)(other)).getBaseExpression()));
        }
        protected internal override int computeHashCode() {
            return base.computeHashCode();
        }
        protected internal override string displayOperator(Saxon.Hej.Configuration config) {
            return "convertUntyped";
        }
        public override string getExpressionName() {
            return "convertUntyped";
        }
        public override string toShortString() {
            return getBaseExpression().toShortString();
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("cvUntyped", this);
            destination.emitAttribute("to", Saxon.Hej.type.AlphaCode.fromItemType(getRequiredItemType()));
            if ((getRoleDiagnostic() != null)) {
                destination.emitAttribute("diag", getRoleDiagnostic().save());
            }
            getBaseExpression().export(destination);
            destination.endElement();
        }
    
        private class Inner_Converter_7 : Saxon.Hej.type.Converter {
            public Inner_Converter_7(Saxon.Hej.type.PlainType requiredItemType, Saxon.Hej.expr.Expression operand) {
                this.requiredItemType = requiredItemType;
                this.operand = operand;
            }
            private Saxon.Hej.type.PlainType requiredItemType;
            private Saxon.Hej.expr.Expression operand;
            public override  /* REPLACING public */ Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure((("Implicit conversion of untypedAtomic value to "+/*extraParam*/requiredItemType.ToString())+" is not allowed"));
                /*localvar*/vf.setErrorCode("XPTY0117");
                /*localvar*/vf.setLocator(/*extraParam*/operand.getLocation());
                return /*localvar*/vf;
            }
        }

    }
}