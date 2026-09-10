using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class GroupAdjacentTester : Saxon.Eej.ee.stream.adjunct.GroupBoundaryTester {
        private Saxon.Hej.expr.sort.CompositeAtomicKey currentGroupingComparisonKey;
        private readonly Saxon.Hej.lib.StringCollator collator;
        private Saxon.Hej.om.AtomicSequence currentGroupingKey;
        private readonly int implicitTimezone;
        public GroupAdjacentTester(Saxon.Hej.expr.instruct.ForEachGroup expression, Saxon.Hej.expr.XPathContext context) : base (expression, context) {
            collator = expression.getCollation();
            implicitTimezone = context.getImplicitTimezone();
        }
        public override bool notifyItem() {
            Saxon.Hej.expr.Expression groupingKey = getControllingExpression().getGroupingKey();
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> comparisonKeyList = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>();
            Saxon.Hej.om.SequenceIterator iter = groupingKey.iterate(getLocalContext());
            Saxon.Hej.om.AtomicArray key = new Saxon.Hej.om.AtomicArray(iter);
            foreach (Saxon.Hej.value.AtomicValue val in key) {
                comparisonKeyList.Add(val.getXPathMatchKey(collator, implicitTimezone));
            }
            Saxon.Hej.expr.sort.CompositeAtomicKey cak = new Saxon.Hej.expr.sort.CompositeAtomicKey(comparisonKeyList);
            bool isNewGroup = ((currentGroupingComparisonKey == null) || !(currentGroupingComparisonKey.Equals(cak)));
            currentGroupingComparisonKey = cak;
            if (isNewGroup) {
                currentGroupingKey = key;
            }
            return isNewGroup;
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return currentGroupingKey;
        }
    
    }
}