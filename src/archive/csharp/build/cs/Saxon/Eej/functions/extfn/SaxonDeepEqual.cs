using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class SaxonDeepEqual : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string flags = arguments[3].head().getStringValue();
            if ((flags.IndexOf('!')>=0)) {
                Saxon.Hej.lib.Logger err = context.getConfiguration().getLogger();
                Saxon.Ejava.util.Properties indent = new Saxon.Ejava.util.Properties();
                indent.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, "xml");
                indent.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT, "yes");
                err.info("DeepEqual: first argument:");
                Saxon.Hej.query.QueryResult.serialize(Saxon.Hej.query.QueryResult.wrap(arguments[0].iterate(), context.getConfiguration()), err.asStreamResult(), indent);
                err.info("DeepEqual: second argument:");
                Saxon.Hej.query.QueryResult.serialize(Saxon.Hej.query.QueryResult.wrap(arguments[1].iterate(), context.getConfiguration()), err.asStreamResult(), indent);
            }
            Saxon.Hej.om.Item collationValue = arguments[2].head();
            Saxon.Hej.Configuration config = context.getConfiguration();
            string collation = ((collationValue == null) ? config.getDefaultCollationName() : collationValue.getStringValue());
            Saxon.Hej.lib.StringCollator collator = config.getCollation(collation);
            if ((collator == null)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("Unknown collation "+collation));
                e.setErrorCode("FOCH0002");
                throw e;
            }
            Saxon.Hej.expr.sort.GenericAtomicComparer comparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, context);
            int flag = 0;
            if (flags.Contains("N")) {
                flag |= Saxon.Hej.functions.DeepEqual.INCLUDE_NAMESPACES;
            }
            if (flags.Contains("J")) {
                flag |= Saxon.Hej.functions.DeepEqual.JOIN_ADJACENT_TEXT_NODES;
            }
            if (flags.Contains("C")) {
                flag |= Saxon.Hej.functions.DeepEqual.INCLUDE_COMMENTS;
            }
            if (flags.Contains("P")) {
                flag |= Saxon.Hej.functions.DeepEqual.INCLUDE_PROCESSING_INSTRUCTIONS;
            }
            if (flags.Contains("F")) {
                flag |= Saxon.Hej.functions.DeepEqual.INCLUDE_PREFIXES;
            }
            if (flags.Contains("S")) {
                flag |= Saxon.Hej.functions.DeepEqual.COMPARE_STRING_VALUES;
            }
            if (flags.Contains("A")) {
                flag |= Saxon.Hej.functions.DeepEqual.COMPARE_ANNOTATIONS;
            }
            if (flags.Contains("I")) {
                flag |= Saxon.Hej.functions.DeepEqual.COMPARE_ID_FLAGS;
            }
            if (flags.Contains("v")) {
                flag |= Saxon.Hej.functions.DeepEqual.EXCLUDE_VARIETY;
            }
            if (flags.Contains("w")) {
                flag |= Saxon.Hej.functions.DeepEqual.EXCLUDE_WHITESPACE_TEXT_NODES;
            }
            if (flags.Contains("?")) {
                flag |= Saxon.Hej.functions.DeepEqual.WARNING_IF_FALSE;
            }
            bool result = Saxon.Hej.functions.DeepEqual.deepEqual(arguments[0].iterate(), arguments[1].iterate(), comparer, context, flag);
            return Saxon.Hej.value.BooleanValue.get(result);
        }
    
    }
}