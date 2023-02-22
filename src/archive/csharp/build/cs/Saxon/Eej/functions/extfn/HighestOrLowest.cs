using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class HighestOrLowest : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.om.FunctionItem function;
            if ((arguments.Length == 2)) {
                function = (Saxon.Hej.om.FunctionItem)(arguments[1].head());
            } else {
                function = config.makeSystemFunction("data", 1);
            }
            Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[1];
            Saxon.Hej.value.AtomicValue maxSoFar = null;
            System.Collections.Generic.IList<Saxon.Hej.om.Item> highestSoFar = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            Saxon.Hej.lib.StringCollator collator = config.getCollation(getRetainedStaticContext().getDefaultCollationName());
            if ((collator == null)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("Unknown collation "+getRetainedStaticContext().getDefaultCollationName()));
                e.setErrorCode("FOCH0002");
                throw e;
            }
            Saxon.Hej.expr.sort.AtomicComparer comparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, context);
            if (getFunctionName().getLocalPart().Equals("lowest")) {
                comparer = new Saxon.Hej.expr.sort.DescendingComparer(comparer);
            }
            Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
            Saxon.Hej.om.Item next;
            while (((next = iter.next()) != null)) {
                args[0] = next;
                Saxon.Hej.om.Sequence key = dynamicCall(function, context, args);
                Saxon.Hej.om.Item head = key.head();
                if ((head != null)) {
                    if (!(((head is Saxon.Hej.value.AtomicValue)))) {
                        throw new Saxon.Hej.trans.XPathException((("Result of function supplied as second argument to "+getFunctionName().getDisplayName())+" must be an atomic value"), "XPTY0004");
                    }
                    Saxon.Hej.value.AtomicValue keyVal = (Saxon.Hej.value.AtomicValue)(key.head());
                    if (keyVal.isUntypedAtomic()) {
                        Saxon.Hej.type.StringToDouble converter = context.getConfiguration().getConversionRules().getStringToDoubleConverter();
                        try {
                            keyVal = new Saxon.Hej.value.DoubleValue(converter.stringToNumber(keyVal.getUnicodeStringValue()));
                        } catch (Saxon.Ejava.lang.NumberFormatException e) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(("Failed to convert key to xs:double: "+e.Message), "FORG0001");
                            de.setXPathContext(context);
                            throw de;
                        }
                    }
                    if (keyVal.isNaN()) {
                        continue ;
                    }
                    if (!(keyVal.getPrimitiveType().isOrdered(false))) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Type "+keyVal.getPrimitiveType())+" is not an ordered type"));
                        de.setErrorCode("FORG0006");
                        de.setIsTypeError(true);
                        de.setXPathContext(context);
                        throw de;
                    }
                    if ((maxSoFar == null)) {
                        highestSoFar.Add(next);
                        maxSoFar = keyVal;
                    } else {
                        int result;
                        try {
                            result = comparer.compareAtomicValues(keyVal, maxSoFar);
                        } catch (System.InvalidCastException e) {
                            throw new Saxon.Hej.trans.XPathException((("Keys computed for "+getFunctionName().getDisplayName())+" are not comparable"), "FORG0006");
                        }
                        if ((result>0)) {
                            highestSoFar.Clear();
                            highestSoFar.Add(next);
                            maxSoFar = keyVal;
                        } else {
                            if ((result == 0)) {
                                highestSoFar.Add(next);
                            }
                        }
                    }
                }
            }
            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(highestSoFar);
        }
    
    }
}