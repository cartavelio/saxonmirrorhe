using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CollationKeyFn : Saxon.Hej.functions.CollatingFunctionFixed {
        public static Saxon.Hej.value.Base64BinaryValue getCollationKey(Saxon.Hej.str.UnicodeString s, Saxon.Hej.lib.StringCollator collator) {
            Saxon.Hej.value.AtomicValue val = collator.getCollationKey(s).asAtomic();
            if ((val is Saxon.Hej.value.Base64BinaryValue)) {
                return (Saxon.Hej.value.Base64BinaryValue)(val);
            } else {
                if ((val is Saxon.Hej.value.StringValue)) {
                    return ((Saxon.Hej.value.StringValue)(val)).getCodepointCollationKey();
                } else {
                    throw new Saxon.Ejava.lang.IllegalStateException("Collation key must be Base64Binary");
                }
            }
        }
        public override Saxon.Hej.value.Base64BinaryValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue @in = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.lib.StringCollator collator = getStringCollator();
            return getCollationKey(@in.getUnicodeStringValue(), collator);
        }
    
    }
}