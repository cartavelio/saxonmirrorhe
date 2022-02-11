using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class UnionConstructorFunction : Saxon.Hej.functions.AbstractFunction {
        protected internal Saxon.Hej.type.UnionType targetType;
        protected internal Saxon.Hej.om.NamespaceResolver resolver;
        protected internal bool allowEmpty;
        public UnionConstructorFunction(Saxon.Hej.type.UnionType targetType, Saxon.Hej.om.NamespaceResolver resolver, bool allowEmpty)  {
            this.targetType = targetType;
            this.resolver = resolver;
            this.allowEmpty = allowEmpty;
        }
        protected internal Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC;
        }
        public bool isAllowEmpty() {
            return allowEmpty;
        }
        public Saxon.Hej.type.UnionType getTargetType() {
            return targetType;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return resolver;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            Saxon.Hej.value.SequenceType resultType = targetType.getResultTypeOfCast();
            Saxon.Hej.value.SequenceType argType = (allowEmpty ? Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC : Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
            return new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{argType}, resultType);
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return targetType.getTypeName();
        }
        public override string getDescription() {
            return getFunctionName().getDisplayName();
        }
        public override int getArity() {
            return 1;
        }
        public Saxon.Hej.om.AtomicSequence cast(Saxon.Hej.value.AtomicValue value, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            if ((value == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            if (((value is Saxon.Hej.value.StringValue) && !(((value is Saxon.Hej.value.AnyURIValue))))) {
                try {
                    return targetType.getTypedValue(value.getUnicodeStringValue(), resolver, rules);
                } catch (Saxon.Hej.type.ValidationException e) {
                    e.setErrorCode("FORG0001");
                    throw;
                }
            }
            Saxon.Hej.type.AtomicType label = value.getItemType();
            System.Collections.Generic.IEnumerable<Saxon.Hej.type.PlainType> memberTypes = ((Saxon.Hej.type.UnionType)(targetType)).getPlainMemberTypes();
            if (targetType.isPlainType()) {
                foreach (Saxon.Hej.type.PlainType member in memberTypes) {
                    if (label.Equals(member)) {
                        return value;
                    }
                }
                foreach (Saxon.Hej.type.PlainType member in memberTypes) {
                    Saxon.Hej.type.AtomicType t = label;
                    while ((t != null)) {
                        if (t.Equals(member)) {
                            return value;
                        } else {
                            t = ((t.getBaseType() is Saxon.Hej.type.AtomicType) ? (Saxon.Hej.type.AtomicType)(t.getBaseType()) : null);
                        }
                    }
                }
            }
            foreach (Saxon.Hej.type.PlainType type in memberTypes) {
                if ((type is Saxon.Hej.type.AtomicType)) {
                    Saxon.Hej.type.Converter c = rules.getConverter(value.getItemType(), (Saxon.Hej.type.AtomicType)(type));
                    if ((c != null)) {
                        Saxon.Hej.type.ConversionResult result = c.convert(value);
                        if ((result is Saxon.Hej.value.AtomicValue)) {
                            if (!(targetType.isPlainType())) {
                                Saxon.Hej.type.ValidationFailure vf = targetType.checkAgainstFacets((Saxon.Hej.value.AtomicValue)(result), rules);
                                if ((vf == null)) {
                                    return (Saxon.Hej.value.AtomicValue)(result);
                                }
                            } else {
                                return (Saxon.Hej.value.AtomicValue)(result);
                            }
                        }
                    }
                }
            }
            throw new Saxon.Hej.trans.XPathException(("Cannot convert the supplied value to "+targetType.getDescription()), "FORG0001");
        }
        public override Saxon.Hej.om.AtomicSequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(args[0].head());
            if ((val == null)) {
                if (allowEmpty) {
                    return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
                } else {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Cast expression does not allow an empty sequence to be supplied", "XPTY0004");
                    e.setIsTypeError(true);
                    throw e;
                }
            }
            return cast(val, context);
        }
        public static Saxon.Hej.om.AtomicSequence cast(Saxon.Hej.value.AtomicValue value, Saxon.Hej.type.UnionType targetType, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            if ((value == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            if (((value is Saxon.Hej.value.StringValue) && !(((value is Saxon.Hej.value.AnyURIValue))))) {
                try {
                    return targetType.getTypedValue(value.getUnicodeStringValue(), nsResolver, rules);
                } catch (Saxon.Hej.type.ValidationException e) {
                    e.setErrorCode("FORG0001");
                    throw;
                }
            }
            Saxon.Hej.type.AtomicType label = value.getItemType();
            System.Collections.Generic.IEnumerable<Saxon.Hej.type.PlainType> memberTypes = targetType.getPlainMemberTypes();
            foreach (Saxon.Hej.type.PlainType member in memberTypes) {
                if (label.Equals(member)) {
                    return value;
                }
            }
            foreach (Saxon.Hej.type.PlainType member in memberTypes) {
                Saxon.Hej.type.AtomicType t = label;
                while ((t != null)) {
                    if (t.Equals(member)) {
                        return value;
                    } else {
                        t = ((t.getBaseType() is Saxon.Hej.type.AtomicType) ? (Saxon.Hej.type.AtomicType)(t.getBaseType()) : null);
                    }
                }
            }
            foreach (Saxon.Hej.type.PlainType type in memberTypes) {
                if ((type is Saxon.Hej.type.AtomicType)) {
                    Saxon.Hej.type.Converter c = rules.getConverter(value.getItemType(), (Saxon.Hej.type.AtomicType)(type));
                    if ((c != null)) {
                        Saxon.Hej.type.ConversionResult result = c.convert(value);
                        if ((result is Saxon.Hej.value.AtomicValue)) {
                            return (Saxon.Hej.value.AtomicValue)(result);
                        }
                    }
                }
            }
            throw new Saxon.Hej.trans.XPathException(("Cannot convert the supplied value to "+targetType.getDescription()), "FORG0001");
        }
    
    }
}