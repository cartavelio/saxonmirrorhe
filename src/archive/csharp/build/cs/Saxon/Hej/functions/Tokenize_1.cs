using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Tokenize_1 : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item item = arguments[0].head();
            if ((item == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Hej.value.Whitespace.Tokenizer(item.getUnicodeStringValue()));
        }
    
    }
}