using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class ContextMappingIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.om.FocusIterator @base;
        private readonly Saxon.Hej.expr.ContextMappingFunction action;
        private readonly Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.om.SequenceIterator stepIterator = null;
        private bool discharged;
        public ContextMappingIterator(Saxon.Hej.expr.ContextMappingFunction action, Saxon.Hej.expr.XPathContext context)  {
            @base = context.getCurrentIterator();
            this.action = action;
            this.context = context;
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item nextItem;
            while (true) {
                if ((stepIterator != null)) {
                    nextItem = stepIterator.next();
                    if ((nextItem != null)) {
                        break ;
                    } else {
                        stepIterator = null;
                    }
                }
                if ((@base.next() != null)) {
                    try {
                        stepIterator = /*Delegate.map()*/action(context);
                        if (discharged) {
                            stepIterator.discharge();
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                    nextItem = stepIterator.next();
                    if ((nextItem == null)) {
                        stepIterator = null;
                    } else {
                        break ;
                    }
                } else {
                    stepIterator = null;
                    return null;
                }
            }
            return nextItem;
        }
        public void close() {
            @base.close();
            if ((stepIterator != null)) {
                stepIterator.close();
            }
        }
        public void discharge() {
            @base.discharge();
            if ((stepIterator != null)) {
                stepIterator.discharge();
            }
            discharged = true;
        }
    
    }
}