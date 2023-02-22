using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public abstract class GroupMatchingIterator : Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.expr.sort.GroupIterator {
        protected internal Saxon.Hej.expr.elab.PullEvaluator select;
        protected internal Saxon.Hej.om.FocusIterator population;
        protected internal Saxon.Hej.pattern.Pattern pattern;
        protected internal Saxon.Hej.expr.XPathContext baseContext;
        protected internal Saxon.Hej.expr.XPathContext runningContext;
        protected internal System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers;
        protected internal Saxon.Hej.om.Item nextItem;
        protected internal Saxon.Hej.om.Item current = null;
        protected internal int position = 0;
        protected internal abstract void advance();
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
                if ((nextItem != null)) {
                    current = nextItem;
                    position++;
                    advance();
                    return current;
                } else {
                    current = null;
                    position = -1;
                    return null;
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            population.close();
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.ManualGroupIterator iter = new Saxon.Eej.ee.stream.ManualGroupIterator(current, position);
            iter.setCurrentGroup(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(currentMembers));
            iter.setLengthFinder(runningContext.getLast /* instance method reference */);
            return iter;
        }
    
        /* Creating stub method supportsGetLength to satisfy interface LastPositionFinder */
        public virtual bool supportsGetLength() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getLength to satisfy interface LastPositionFinder */
        public virtual int getLength() {
            throw new System.NotImplementedException();
        }

    }
}