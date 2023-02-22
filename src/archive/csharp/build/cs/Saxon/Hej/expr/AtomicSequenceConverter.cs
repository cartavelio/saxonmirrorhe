using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class AtomicSequenceConverter : Saxon.Hej.expr.UnaryExpression {
        public static Saxon.Hej.expr.AtomicSequenceConverter.ToStringMappingFunction TO_STRING_MAPPER = new Saxon.Hej.expr.AtomicSequenceConverter.ToStringMappingFunction();
        protected internal Saxon.Hej.type.PlainType requiredItemType;
        protected internal Saxon.Hej.type.Converter converter;
        private Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier;
        public AtomicSequenceConverter(Saxon.Hej.expr.Expression sequence, Saxon.Hej.type.PlainType requiredItemType) : base (sequence) {
            this.requiredItemType = requiredItemType;
        }
        public static Saxon.Hej.expr.AtomicSequenceConverter makeDownCaster(Saxon.Hej.expr.Expression sequence, Saxon.Hej.type.AtomicType requiredItemType, Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.AtomicSequenceConverter asc = new Saxon.Hej.expr.AtomicSequenceConverter(sequence, requiredItemType);
            asc.setConverter(new Saxon.Hej.type.Converter.DownCastingConverter(requiredItemType, config.getConversionRules(), "XPTY0004"));
            return asc;
        }
        public void allocateConverterStatically(Saxon.Hej.Configuration config, bool allowNull) {
            converter = allocateConverter(config, allowNull, getBaseExpression().getItemType());
        }
        public Saxon.Hej.type.Converter allocateConverter(Saxon.Hej.Configuration config, bool allowNull) {
            return allocateConverter(config, allowNull, getBaseExpression().getItemType());
        }
        protected internal Saxon.Hej.type.Converter getConverterDynamically(Saxon.Hej.expr.XPathContext context) {
            if ((converter != null)) {
                return converter;
            }
            return allocateConverter(context.getConfiguration(), false);
        }
        public Saxon.Hej.type.Converter allocateConverter(Saxon.Hej.Configuration config, bool allowNull, Saxon.Hej.type.ItemType sourceType) {
            Saxon.Hej.lib.ConversionRules rules = config.getConversionRules();
            Saxon.Hej.type.Converter converter = null;
            if ((sourceType is Saxon.Hej.type.ErrorType)) {
                converter = Saxon.Hej.type.StringConverter.IdentityConverter.INSTANCE;
            } else {
                if (!(((sourceType is Saxon.Hej.type.AtomicType)))) {
                    converter = null;
                } else {
                    if ((requiredItemType is Saxon.Hej.type.AtomicType)) {
                        converter = rules.getConverter((Saxon.Hej.type.AtomicType)(sourceType), (Saxon.Hej.type.AtomicType)(requiredItemType));
                    } else {
                        if (((Saxon.Hej.type.SimpleType)(requiredItemType)).isUnionType()) {
                            converter = new Saxon.Hej.type.StringConverter.StringToUnionConverter(requiredItemType, rules);
                        }
                    }
                }
            }
            if (((converter == null) && !(allowNull))) {
                converter = new Inner_Converter_7(this, rules);
            }
            return converter;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE;
        }
        public Saxon.Hej.type.PlainType getRequiredItemType() {
            return requiredItemType;
        }
        public Saxon.Hej.type.Converter getConverter() {
            return converter;
        }
        public void setConverter(Saxon.Hej.type.Converter converter) {
            this.converter = converter;
        }
        public void setRoleDiagnostic(Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier) {
            if ((roleSupplier != null)) {
                this.roleSupplier = roleSupplier;
            }
        }
        public Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> getRoleSupplier() {
            return this.roleSupplier;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.Expression operand = getBaseExpression().simplify();
            setBaseExpression(operand);
            if (((operand is Saxon.Hej.expr.Literal) && (requiredItemType is Saxon.Hej.type.AtomicType))) {
                if (Saxon.Hej.expr.Literal.isEmptySequence(operand)) {
                    return operand;
                }
                Saxon.Hej.Configuration config = getConfiguration();
                if ((converter == null)) {
                    allocateConverterStatically(config, true);
                }
                if ((converter != null)) {
                    try {
                        Saxon.Hej.om.GroundedValue val = Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(new Saxon.Hej.expr.EarlyEvaluationContext(config)));
                        return Saxon.Hej.expr.Literal.makeLiteral(val, operand);
                    } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                        throw e.getXPathException();
                    }
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            if (th.isSubType(operand.getItemType(), requiredItemType)) {
                return operand;
            } else {
                if ((converter == null)) {
                    allocateConverterStatically(config, true);
                }
                return this;
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            if ((getBaseExpression() is Saxon.Hej.expr.UntypedSequenceConverter)) {
                Saxon.Hej.expr.UntypedSequenceConverter asc = (Saxon.Hej.expr.UntypedSequenceConverter)(getBaseExpression());
                Saxon.Hej.type.ItemType ascType = asc.getItemType();
                if ((ascType == requiredItemType)) {
                    return getBaseExpression();
                } else {
                    if (((((requiredItemType == Saxon.Hej.type.BuiltInAtomicType.STRING) || (requiredItemType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) && (((ascType == Saxon.Hej.type.BuiltInAtomicType.STRING) || (ascType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))))) {
                        Saxon.Hej.expr.UntypedSequenceConverter old = (Saxon.Hej.expr.UntypedSequenceConverter)(getBaseExpression());
                        Saxon.Hej.expr.UntypedSequenceConverter asc2 = new Saxon.Hej.expr.UntypedSequenceConverter(old.getBaseExpression(), requiredItemType);
                        return asc2.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                    }
                }
            } else {
                if ((getBaseExpression() is Saxon.Hej.expr.AtomicSequenceConverter)) {
                    Saxon.Hej.expr.AtomicSequenceConverter asc = (Saxon.Hej.expr.AtomicSequenceConverter)(getBaseExpression());
                    Saxon.Hej.type.ItemType ascType = asc.getItemType();
                    if ((ascType == requiredItemType)) {
                        return getBaseExpression();
                    } else {
                        if (((((requiredItemType == Saxon.Hej.type.BuiltInAtomicType.STRING) || (requiredItemType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) && (((ascType == Saxon.Hej.type.BuiltInAtomicType.STRING) || (ascType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))))) {
                            Saxon.Hej.expr.AtomicSequenceConverter old = (Saxon.Hej.expr.AtomicSequenceConverter)(getBaseExpression());
                            Saxon.Hej.expr.AtomicSequenceConverter asc2 = new Saxon.Hej.expr.AtomicSequenceConverter(old.getBaseExpression(), requiredItemType);
                            return asc2.typeCheck(visitor, contextInfo).optimize(visitor, contextInfo);
                        }
                    }
                }
            }
            return this;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        protected internal override int computeSpecialProperties() {
            int p = (base.computeSpecialProperties()|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            if ((requiredItemType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                p &= ~(Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC);
            } else {
                p |= Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC;
            }
            return p;
        }
        public override string getStreamerName() {
            return "AtomicSequenceConverter";
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.AtomicSequenceConverter atomicConverter = new Saxon.Hej.expr.AtomicSequenceConverter(getBaseExpression().copy(rebindings), requiredItemType);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, atomicConverter);
            atomicConverter.setConverter(converter);
            atomicConverter.setRoleDiagnostic(getRoleSupplier());
            return atomicConverter;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator @base = getBaseExpression().iterate(context);
            return getConvertingIterator(context, @base);
        }
        public Saxon.Hej.expr.ItemMappingIterator getConvertingIterator(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.SequenceIterator @base) {
            Saxon.Hej.type.Converter conv = getConverterDynamically(context);
            if ((conv == Saxon.Hej.type.Converter.ToStringConverter.INSTANCE)) {
                return new Saxon.Hej.expr.ItemMappingIterator(@base, TO_STRING_MAPPER, true);
            } else {
                Saxon.Hej.expr.AtomicSequenceConverter.AtomicSequenceMappingFunction mapper = new Saxon.Hej.expr.AtomicSequenceConverter.AtomicSequenceMappingFunction();
                mapper.setConverter(conv);
                if ((roleSupplier != null)) {
                    string errorCode = roleSupplier().getErrorCode();
                    if (!("XPTY0004".Equals(errorCode))) {
                        mapper.setErrorCode(errorCode);
                    }
                }
                return new Saxon.Hej.expr.ItemMappingIterator(@base, mapper, true);
            }
        }
        public class AtomicSequenceMappingFunction : Saxon.Hej.expr.ItemMappingFunction {
            private Saxon.Hej.type.Converter converter;
            private string errorCode;
            public void setConverter(Saxon.Hej.type.Converter converter) {
                this.converter = converter;
            }
            public void setErrorCode(string code) {
                this.errorCode = code;
            }
            public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
                Saxon.Hej.type.ConversionResult result = converter.convert((Saxon.Hej.value.AtomicValue)(item));
                if (((errorCode != null) && (result is Saxon.Hej.type.ValidationFailure))) {
                    ((Saxon.Hej.type.ValidationFailure)(result)).setErrorCode(errorCode);
                }
                return result.asAtomic();
            }
        
        }
        public class ToStringMappingFunction : Saxon.Hej.expr.ItemMappingFunction {
            public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
                return new Saxon.Hej.value.StringValue(item.getUnicodeStringValue());
            }
        
        }
        public override Saxon.Hej.value.AtomicValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/makeElaborator().elaborateForItem()(context));
        }
        public Saxon.Hej.value.AtomicValue convertItem(Saxon.Hej.value.AtomicValue item, Saxon.Hej.expr.XPathContext context) {
            if ((item == null)) {
                return null;
            }
            Saxon.Hej.type.Converter conv = getConverterDynamically(context);
            Saxon.Hej.type.ConversionResult result = conv.convert(item);
            if (((result is Saxon.Hej.type.ValidationFailure) && (roleSupplier != null))) {
                ((Saxon.Hej.type.ValidationFailure)(result)).setErrorCode(roleSupplier().getErrorCode());
            }
            return result.asAtomic();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return requiredItemType;
        }
        protected internal override int computeCardinality() {
            return getBaseExpression().getCardinality();
        }
        public override string ToString() {
            return (((("convertTo_"+getRequiredItemType().ToString())+"(")+getBaseExpression().ToString())+")");
        }
        public override bool Equals(System.Object other) {
            return (base.Equals(other) && requiredItemType.Equals(((Saxon.Hej.expr.AtomicSequenceConverter)(other)).requiredItemType));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^requiredItemType.GetHashCode());
        }
        public override string getExpressionName() {
            return "convert";
        }
        protected internal override string displayOperator(Saxon.Hej.Configuration config) {
            return "convert";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("convert", this);
            destination.emitAttribute("from", Saxon.Hej.type.AlphaCode.fromItemType(getBaseExpression().getItemType()));
            destination.emitAttribute("to", Saxon.Hej.type.AlphaCode.fromItemType(requiredItemType));
            if (((converter is Saxon.Hej.type.Converter.PromoterToDouble) || (converter is Saxon.Hej.type.Converter.PromoterToFloat))) {
                destination.emitAttribute("flags", "p");
            }
            if ((getRoleSupplier() != null)) {
                destination.emitAttribute("diag", getRoleSupplier()().save());
            }
            getBaseExpression().export(destination);
            destination.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.AtomicSequenceConverter.AtomicSequenceConverterElaborator();
        }
        public class AtomicSequenceConverterElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.AtomicSequenceConverter expr = (Saxon.Hej.expr.AtomicSequenceConverter)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBaseExpression().makeElaborator().elaborateForPull();
                return context => {
                    Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/baseEval(context);
                    return expr.getConvertingIterator(context, @base);
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.AtomicSequenceConverter expr = (Saxon.Hej.expr.AtomicSequenceConverter)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator baseEval = expr.getBaseExpression().makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.value.AtomicValue @base = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/baseEval(context));
                    return expr.convertItem(@base, context);
                } ;
            }
        
        }
    
        private class Inner_Converter_7 : Saxon.Hej.type.Converter {
            private readonly AtomicSequenceConverter outerInstance;
            public Inner_Converter_7(AtomicSequenceConverter outerInstance, /*AA*/Saxon.Hej.lib.ConversionRules arg1) : base(arg1) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ Saxon.Hej.type.ConversionResult convert(Saxon.Hej.value.AtomicValue input) {
                Saxon.Hej.type.Converter converter = /*superMethod*/getConversionRules().getConverter(/*localparam*/input.getPrimitiveType(), (Saxon.Hej.type.AtomicType)(outerInstance.requiredItemType));
                if ((/*localvar*/converter == null)) {
                    return new Saxon.Hej.type.ValidationFailure(((("Cannot convert value from "+/*localparam*/input.getPrimitiveType())+" to ")+outerInstance.requiredItemType));
                } else {
                    return /*localvar*/converter.convert(/*localparam*/input);
                }
            }
        }

    }
}