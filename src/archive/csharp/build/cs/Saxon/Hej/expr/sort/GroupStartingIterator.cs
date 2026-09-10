using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GroupStartingIterator : Saxon.Hej.expr.sort.GroupMatchingIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.sort.GroupIterator {
        public GroupStartingIterator(Saxon.Hej.expr.Expression select, Saxon.Hej.pattern.Pattern startPattern, Saxon.Hej.expr.XPathContext context)  {
            this.select = select;
            this.pattern = startPattern;
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
                Saxon.Hej.expr.sort.GroupStartingIterator another = new Saxon.Hej.expr.sort.GroupStartingIterator(select, pattern, baseContext);
                return Saxon.Hej.functions.Count.steppingCount(another);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        protected internal override void advance() {
            currentMembers = new System.Collections.Generic.List<Saxon.Hej.om.Item>(10);
            currentMembers.Add(current);
            while (true) {
                Saxon.Hej.om.Item nextCandidate = population.next();
                if ((nextCandidate == null)) {
                    break ;
                }
                if (pattern.matches(nextCandidate, runningContext)) {
                    nextItem = nextCandidate;
                    return ;
                } else {
                    currentMembers.Add(nextCandidate);
                }
            }
            nextItem = null;
        }
    
    }
}