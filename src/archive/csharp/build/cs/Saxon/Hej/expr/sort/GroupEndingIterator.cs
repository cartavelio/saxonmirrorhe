using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GroupEndingIterator : Saxon.Hej.expr.sort.GroupMatchingIterator, Saxon.Hej.expr.sort.GroupIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        public GroupEndingIterator(Saxon.Hej.expr.Expression select, Saxon.Hej.pattern.Pattern endPattern, Saxon.Hej.expr.XPathContext context)  {
            this.select = select;
            this.pattern = endPattern;
            baseContext = context;
            runningContext = context.newMinorContext();
            this.population = runningContext.trackFocus(select.iterate(context));
            nextItem = population.next();
        }
        public override bool supportsGetLength() {
            return true;
        }
        public override int getLength() {
            try {
                Saxon.Hej.expr.sort.GroupEndingIterator another = new Saxon.Hej.expr.sort.GroupEndingIterator(select, pattern, baseContext);
                return Saxon.Hej.functions.Count.steppingCount(another);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        protected internal override void advance() {
            currentMembers = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
            currentMembers.Add(current);
            nextItem = current;
            while ((nextItem != null)) {
                if (pattern.matches(nextItem, runningContext)) {
                    nextItem = population.next();
                    if ((nextItem != null)) {
                        break ;
                    }
                } else {
                    nextItem = population.next();
                    if ((nextItem != null)) {
                        currentMembers.Add(nextItem);
                    }
                }
            }
        }
    
    }
}