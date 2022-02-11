using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class XQueryFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string query = arguments[0].head().getStringValue();
            Saxon.Hej.query.StaticQueryContext sqc = context.getConfiguration().newStaticQueryContext();
            Saxon.Hej.query.XQueryExpression exp = sqc.compileQuery(query);
            Saxon.Hej.expr.Callable callable = new Saxon.Hej.expr.CallableDelegate((cxt, args) => {
                Saxon.Hej.query.DynamicQueryContext dqc = new Saxon.Hej.query.DynamicQueryContext(cxt.getConfiguration());
                Saxon.Hej.om.Item arg0 = args[0].head();
                if ((arg0 != null)) {
                    dqc.setContextItem(arg0);
                }
                Saxon.Hej.ma.map.MapItem paramMap = (Saxon.Hej.ma.map.MapItem)(args[1].head());
                foreach (Saxon.Hej.ma.map.KeyValuePair kvp in paramMap.keyValuePairs()) {
                    Saxon.Hej.value.QNameValue key = (Saxon.Hej.value.QNameValue)(kvp.key);
                    Saxon.Hej.om.GroundedValue value = kvp.value;
                    dqc.setParameter(key.getStructuredQName(), value);
                }
                Saxon.Hej.om.SequenceIterator iter = exp.iterator(dqc);
                return new Saxon.Hej.om.LazySequence(iter);
            } );
            return new Saxon.Hej.functions.CallableFunction(2, callable, functionType);
        }
        private static Saxon.Hej.ma.map.MapType paramMap = new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.QNAME, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public static Saxon.Hej.type.FunctionItemType functionType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.OPTIONAL_ITEM, new Saxon.Hej.value.SequenceType(paramMap, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
    
    }
}