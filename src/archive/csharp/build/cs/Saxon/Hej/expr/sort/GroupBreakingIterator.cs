using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GroupBreakingIterator : Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.sort.GroupIterator {
        private readonly Saxon.Hej.expr.elab.PullEvaluator select;
        private readonly Saxon.Hej.om.FocusIterator population;
        private readonly Saxon.Hej.om.FunctionItem breakWhen;
        private readonly Saxon.Hej.expr.XPathContext baseContext;
        private readonly Saxon.Hej.expr.XPathContext runningContext;
        private System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers;
        private Saxon.Hej.om.Item nextItem;
        private Saxon.Hej.om.Item current = null;
        private int position = 0;
        public GroupBreakingIterator(Saxon.Hej.expr.elab.PullEvaluator select, Saxon.Hej.om.FunctionItem breakWhen, Saxon.Hej.expr.XPathContext baseContext)  {
            this.select = select;
            this.breakWhen = breakWhen;
            this.baseContext = baseContext;
            this.runningContext = baseContext.newMinorContext();
            this.population = runningContext.trackFocus(/*Delegate.iterate()*/select(baseContext));
            nextItem = population.next();
        }
        private void advance() {
            currentMembers = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
            currentMembers.Add(current);
            while (true) {
                Saxon.Hej.om.Item nextCandidate = population.next();
                if ((nextCandidate == null)) {
                    break ;
                }
                Saxon.Hej.value.BooleanValue result = (Saxon.Hej.value.BooleanValue)(breakWhen.call(runningContext, new Saxon.Hej.om.Sequence[]{Saxon.Hej.value.SequenceExtent.makeSequenceExtent(currentMembers), nextCandidate}).head());
                try {
                    if (!(result.getBooleanValue())) {
                        currentMembers.Add(nextCandidate);
                    } else {
                        nextItem = nextCandidate;
                        return ;
                    }
                } catch (System.InvalidCastException e) {
                    string message = "Grouping key values are of non-comparable types";
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                    err.setIsTypeError(true);
                    err.setXPathContext(runningContext);
                    throw err;
                }
            }
            nextItem = null;
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return null;
        }
        public Saxon.Hej.om.GroundedValue currentGroup() {
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(currentMembers);
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (nextItem != null);
        }
        public Saxon.Hej.om.Item next() {
            try {
                if ((nextItem == null)) {
                    current = null;
                    position = -1;
                    return null;
                }
                current = nextItem;
                position++;
                advance();
                return current;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            population.close();
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}