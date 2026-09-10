using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class MappingIterator : Saxon.Hej.om.SequenceIterator {
    // Injecting C# code
        public MappingIterator(Saxon.Hej.om.SequenceIterator first, Saxon.Hej.expr.SequenceMapper.Lambda action) : this(first, Saxon.Hej.expr.SequenceMapper.of(action)) {}
    // End of injected C# code
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private readonly Saxon.Hej.expr.MappingFunction action;
        private Saxon.Hej.om.SequenceIterator results = null;
        public MappingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.MappingFunction action)  {
            this.@base = @base;
            this.action = action;
        }
        public static Saxon.Hej.expr.MappingIterator map(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.SequenceMapper.Lambda mappingExpression) {
            return new Saxon.Hej.expr.MappingIterator(@base, Saxon.Hej.expr.SequenceMapper.of(mappingExpression));
        }
        public Saxon.Hej.om.Item next() {
            try {
                Saxon.Hej.om.Item nextItem;
                while (true) {
                    if ((results != null)) {
                        nextItem = results.next();
                        if ((nextItem != null)) {
                            break ;
                        } else {
                            results = null;
                        }
                    }
                    Saxon.Hej.om.Item nextSource = @base.next();
                    if ((nextSource != null)) {
                        Saxon.Hej.om.SequenceIterator obj = action.map(nextSource);
                        if ((obj != null)) {
                            results = obj;
                            nextItem = results.next();
                            if ((nextItem == null)) {
                                results = null;
                            } else {
                                break ;
                            }
                        }
                    } else {
                        results = null;
                        return null;
                    }
                }
                return nextItem;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            if ((results != null)) {
                results.close();
            }
            @base.close();
        }
    
    }
}