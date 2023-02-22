using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class CharactersFn : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue s = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((s == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.om.SequenceIterator codepoints = s.iterateCharacters();
            Saxon.Hej.expr.ItemMappingIterator mapper = new Saxon.Hej.expr.ItemMappingIterator(codepoints, Saxon.Hej.expr.ItemMapper.of(cp => new Saxon.Hej.value.StringValue(new Saxon.Hej.str.UnicodeChar((int)(((Saxon.Hej.value.IntegerValue)(cp)).longValue())))));
            return new Saxon.Hej.om.LazySequence(mapper);
        }
    
    }
}