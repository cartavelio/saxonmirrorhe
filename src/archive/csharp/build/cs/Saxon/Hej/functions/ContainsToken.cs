using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ContainsToken : Saxon.Hej.functions.CollatingFunctionFixed {
        public override bool isSubstringMatchingFunction() {
            return true;
        }
        private static bool containsToken(Saxon.Hej.om.SequenceIterator arg0, Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.lib.StringCollator collator) {
            if ((arg1 == null)) {
                return false;
            }
            Saxon.Hej.str.UnicodeString search = Saxon.Hej.value.Whitespace.trim(arg1);
            if (search.isEmpty()) {
                return false;
            }
            Saxon.Hej.om.Item item;
            while (((item = arg0.next()) != null)) {
                Saxon.Hej.om.SequenceIterator tokens = new Saxon.Hej.value.Whitespace.Tokenizer(item.getUnicodeStringValue());
                Saxon.Hej.om.Item token;
                while (((token = tokens.next()) != null)) {
                    if (collator.comparesEqual(search, token.getUnicodeStringValue())) {
                        tokens.close();
                        arg0.close();
                        return true;
                    }
                }
            }
            return false;
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.BooleanValue.get(containsToken(arguments[0].iterate(), arguments[1].head().getUnicodeStringValue(), getStringCollator()));
        }
    
    }
}