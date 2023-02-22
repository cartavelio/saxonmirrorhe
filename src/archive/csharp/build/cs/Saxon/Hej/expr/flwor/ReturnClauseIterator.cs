using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ReturnClauseIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.expr.flwor.TuplePull @base;
        private readonly Saxon.Hej.expr.elab.PullEvaluator action;
        private readonly Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.om.SequenceIterator results = null;
        public ReturnClauseIterator(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.elab.PullEvaluator returnAction, Saxon.Hej.expr.XPathContext context)  {
            this.@base = @base;
            this.action = returnAction;
            this.context = context;
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item nextItem;
            while (true) {
                try {
                    if ((results != null)) {
                        nextItem = results.next();
                        if ((nextItem != null)) {
                            break ;
                        } else {
                            results = null;
                        }
                    }
                    if (@base.nextTuple(context)) {
                        results = /*Delegate.iterate()*/action(context);
                        nextItem = results.next();
                        if ((nextItem == null)) {
                            results = null;
                        } else {
                            break ;
                        }
                    } else {
                        results = null;
                        return null;
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            return nextItem;
        }
        public void close() {
            if ((results != null)) {
                results.close();
            }
            @base.close();
        }
    
    }
}