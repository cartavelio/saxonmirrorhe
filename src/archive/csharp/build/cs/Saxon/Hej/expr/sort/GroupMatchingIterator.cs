using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public abstract class GroupMatchingIterator : Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.expr.sort.GroupIterator {
        protected internal Saxon.Hej.expr.Expression select;
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
        public Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentMembers);
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
            return new Saxon.Hej.expr.sort.GroupMatchingIterator.ManualGroupMatchingIterator(current, position, currentMembers, runningContext.newMinorContext());
        }
        private class ManualGroupMatchingIterator : Saxon.Eej.ee.stream.ManualGroupIterator {
            private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> cMembers;
            private readonly Saxon.Hej.expr.XPathContext savedcontext;
            public ManualGroupMatchingIterator(Saxon.Hej.om.Item current, int position, System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers, Saxon.Hej.expr.XPathContext savedContext) : base (current, position) {
                this.cMembers = currentMembers;
                this.savedcontext = savedContext;
                setLengthFinder(() => this.savedcontext.getLast());
            }
            public override Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
                return null;
            }
            public override Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(cMembers);
            }
        
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