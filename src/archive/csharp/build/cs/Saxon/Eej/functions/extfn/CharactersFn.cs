using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class CharactersFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
            Saxon.Hej.om.SequenceIterator codepoints = s.iterateCharacters();
            Saxon.Hej.expr.ItemMappingIterator mapper = new Saxon.Hej.expr.ItemMappingIterator(codepoints, Saxon.Hej.expr.ItemMapper.of((cp => {
                Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
                ub.append((int)(((Saxon.Hej.value.IntegerValue)(cp)).longValue()));
                return new Saxon.Hej.value.StringValue(ub.toUnicodeString());
            } )));
            return new Saxon.Hej.om.LazySequence(mapper);
        }
    
    }
}