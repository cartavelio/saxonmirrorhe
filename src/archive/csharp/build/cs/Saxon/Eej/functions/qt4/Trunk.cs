using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class Trunk : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.tree.iter.LookaheadIterator spy = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(arguments[0].iterate());
            Saxon.Hej.expr.ItemMappingIterator mapper = new Saxon.Hej.expr.ItemMappingIterator(spy, item => (spy.hasNext() ? item : null));
            return new Saxon.Hej.om.LazySequence(mapper);
        }
    
    }
}