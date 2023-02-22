using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SortedGroupIterator : Saxon.Hej.expr.sort.SortedIterator, Saxon.Hej.expr.sort.GroupIterator {
        public SortedGroupIterator(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.GroupIterator @base, Saxon.Hej.expr.sort.SortKeyEvaluator sortKeyEvaluator, Saxon.Hej.expr.sort.AtomicComparer[] comparators) : base (context, @base, sortKeyEvaluator, comparators, true) {
            setHostLanguage(Saxon.Api.HostLanguage.XSLT);
        }
        protected internal override void buildArray() {
            int allocated = (Saxon.Hej.om.SequenceTool.supportsGetLength(@base) ? Saxon.Hej.om.SequenceTool.getLength(@base) : 100);
            values = new Saxon.Hej.expr.sort.ObjectToBeSorted[allocated];
            count = 0;
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setCurrentIterator((Saxon.Hej.om.FocusIterator)(@base));
            Saxon.Hej.expr.sort.GroupIterator groupIter = (Saxon.Hej.expr.sort.GroupIterator)(((Saxon.Hej.om.FocusTrackingIterator)(@base)).getUnderlyingIterator());
            c2.setCurrentGroupIterator(groupIter);
            Saxon.Hej.om.Item item;
            while (((item = @base.next()) != null)) {
                if ((count == allocated)) {
                    allocated *= 2;
                    values = Saxon.Ejava.util.Arrays.copyOf(values, allocated);
                }
                Saxon.Hej.expr.sort.GroupToBeSorted gtbs = new Saxon.Hej.expr.sort.GroupToBeSorted(comparators.Length);
                values[count] = gtbs;
                gtbs.value = item;
                for (int n = 0; (n<comparators.Length); n++) {
                    gtbs.sortKeyValues[n] = sortKeyEvaluator.evaluateSortKey(n, c2);
                }
                gtbs.originalPosition = count++;
                gtbs.currentGroupingKey = groupIter.getCurrentGroupingKey();
                gtbs.currentGroup = groupIter.currentGroup();
            }
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return ((Saxon.Hej.expr.sort.GroupToBeSorted)(values[(position-1)])).currentGroupingKey;
        }
        public Saxon.Hej.om.GroundedValue currentGroup() {
            return ((Saxon.Hej.expr.sort.GroupToBeSorted)(values[(position-1)])).currentGroup;
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.GroupToBeSorted[] groupArray = (Saxon.Hej.expr.sort.GroupToBeSorted[])(values);
            Saxon.Hej.expr.sort.GroupToBeSorted thisGroup = groupArray[(position-1)];
            Saxon.Eej.ee.stream.ManualGroupIterator iter = new Saxon.Eej.ee.stream.ManualGroupIterator(thisGroup.value, position);
            iter.setCurrentGroup(thisGroup.currentGroup);
            iter.setCurrentGroupingKey(thisGroup.currentGroupingKey);
            iter.setLengthFinder(() => values.Length);
            return iter;
        }
    
    }
}