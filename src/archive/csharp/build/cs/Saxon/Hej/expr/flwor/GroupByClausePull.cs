using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class GroupByClausePull : Saxon.Hej.expr.flwor.TuplePull {
        private readonly Saxon.Hej.expr.flwor.TuplePull @base;
        private readonly Saxon.Hej.expr.flwor.GroupByClause groupByClause;
        internal System.Collections.Generic.IEnumerator<System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>> groupIterator;
        private readonly Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers;
        public GroupByClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.GroupByClause groupBy, Saxon.Hej.expr.XPathContext context)  {
            this.@base = @base;
            this.groupByClause = groupBy;
            comparers = new Saxon.Hej.expr.sort.GenericAtomicComparer[groupBy.comparers.Length];
            for (int i = 0; (i<comparers.Length); i++) {
                comparers[i] = ((/*1430*/Saxon.Hej.expr.sort.GenericAtomicComparer)groupBy.comparers[i].provideContext(context));
            }
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if ((groupIterator == null)) {
                Saxon.Hej.expr.flwor.TupleExpression groupingTupleExpr = groupByClause.getGroupingTupleExpression();
                Saxon.Hej.expr.flwor.TupleExpression retainedTupleExpr = groupByClause.getRetainedTupleExpression();
                System.Collections.Generic.Dictionary<System.Object,System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>> map = new System.Collections.Generic.Dictionary<System.Object, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>>();
                while (@base.nextTuple(context)) {
                    Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped otbg = new Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped();
                    Saxon.Hej.om.Sequence[] groupingValues = groupingTupleExpr.evaluateItem(context).getMembers();
                    Saxon.Hej.expr.flwor.GroupByClausePush.checkGroupingValues(groupingValues);
                    otbg.groupingValues = new Saxon.Hej.expr.flwor.Tuple(groupingValues);
                    otbg.retainedValues = retainedTupleExpr.evaluateItem(context);
                    System.Object key = groupByClause.getComparisonKey(otbg.groupingValues, comparers);
                    System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> group = Saxon.Impl.Helpers.MapUtils.Get(map, key);
                    Saxon.Hej.expr.flwor.GroupByClausePush.addToGroup(key, otbg, group, map);
                }
                groupIterator = map.Values.GetEnumerator();
            }
            if (groupIterator.MoveNext()) {
                System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> group = groupIterator.Current;
                groupByClause.processGroup(group, context);
                return true;
            } else {
                return false;
            }
        }
        public override void close() {
            @base.close();
            groupIterator = null;
        }
    
    }
}