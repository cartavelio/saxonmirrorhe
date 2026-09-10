using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class AtomicConstructorFunction : Saxon.Hej.functions.AbstractFunction {
        private readonly Saxon.Hej.type.AtomicType targetType;
        private readonly Saxon.Hej.om.NamespaceResolver nsResolver;
        public AtomicConstructorFunction(Saxon.Hej.type.AtomicType targetType, Saxon.Hej.om.NamespaceResolver resolver)  {
            this.targetType = targetType;
            this.nsResolver = resolver;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC}, Saxon.Hej.value.SequenceType.makeSequenceType(targetType, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE));
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
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(args[0].head());
            if ((val == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.type.Converter converter = config.getConversionRules().getConverter(val.getItemType(), targetType);
            if ((converter == null)) {
                Saxon.Hej.trans.XPathException ex = new Saxon.Hej.trans.XPathException(((("Cannot convert "+val.getItemType())+" to ")+targetType), "XPTY0004");
                ex.setIsTypeError(true);
                throw ex;
            }
            converter = converter.setNamespaceResolver(nsResolver);
            return converter.convert(val).asAtomic();
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("acFnRef");
            @out.emitAttribute("name", targetType.getTypeName());
            @out.endElement();
        }
        public override bool isTrustedResultType() {
            return true;
        }
    
    }
}