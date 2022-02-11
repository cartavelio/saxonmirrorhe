using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ListConstructorFunction : Saxon.Hej.functions.AbstractFunction {
        protected internal Saxon.Hej.type.ListType targetType;
        protected internal Saxon.Hej.om.NamespaceResolver nsResolver;
        protected internal bool allowEmpty;
        protected internal Saxon.Hej.type.SimpleType memberType;
        public ListConstructorFunction(Saxon.Hej.type.ListType targetType, Saxon.Hej.om.NamespaceResolver resolver, bool allowEmpty)  {
            this.targetType = targetType;
            this.nsResolver = resolver;
            this.allowEmpty = allowEmpty;
            this.memberType = targetType.getItemType();
        }
        public Saxon.Hej.type.ListType getTargetType() {
            return targetType;
        }
        public Saxon.Hej.type.SimpleType getMemberType() {
            return memberType;
        }
        public bool isAllowEmpty() {
            return allowEmpty;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            Saxon.Hej.type.AtomicType resultType = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            if (memberType.isAtomicType()) {
                resultType = (Saxon.Hej.type.AtomicType)(memberType);
            }
            Saxon.Hej.value.SequenceType argType = (allowEmpty ? Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC : Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
            return new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{argType}, Saxon.Hej.value.SequenceType.makeSequenceType(resultType, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE));
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return targetType.getStructuredQName();
        }
        public override string getDescription() {
            return getFunctionName().getDisplayName();
        }
        public override int getArity() {
            return 1;
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
            if ((!(((val is Saxon.Hej.value.StringValue))) || (val is Saxon.Hej.value.AnyURIValue))) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Only xs:string and xs:untypedAtomic can be cast to a list type", "XPTY0004");
                e.setIsTypeError(true);
                throw e;
            }
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.str.UnicodeString cs = val.getUnicodeStringValue();
            Saxon.Hej.type.ValidationFailure failure = targetType.validateContent(cs, nsResolver, rules);
            if ((failure != null)) {
                throw failure.makeException();
            }
            return targetType.getTypedValue(cs, nsResolver, rules);
        }
    
    }
}