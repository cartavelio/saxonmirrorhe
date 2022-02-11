using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ListCastableFunction : Saxon.Hej.expr.ListConstructorFunction {
        public ListCastableFunction(Saxon.Hej.type.ListType targetType, Saxon.Hej.om.NamespaceResolver resolver, bool allowEmpty) : base (targetType, resolver, allowEmpty) {
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return null;
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.om.SequenceIterator iter = args[0].iterate();
            Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(iter.next());
            if ((val == null)) {
                return Saxon.Hej.value.BooleanValue.get(allowEmpty);
            }
            if ((iter.next() != null)) {
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
            if ((!(((val is Saxon.Hej.value.StringValue))) || (val is Saxon.Hej.value.AnyURIValue))) {
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.str.UnicodeString cs = val.getUnicodeStringValue();
            Saxon.Hej.type.ValidationFailure failure = targetType.validateContent(cs, nsResolver, rules);
            return Saxon.Hej.value.BooleanValue.get((failure == null));
        }
    
    }
}