using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class GroupByClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.flwor.TuplePush destination;
        private readonly Saxon.Hej.expr.flwor.GroupByClause groupByClause;
        private readonly System.Collections.Generic.Dictionary<System.Object,System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>> map = new System.Collections.Generic.Dictionary<System.Object, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>>();
        private readonly Saxon.Hej.expr.XPathContext context;
        private readonly Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers;
        public GroupByClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.GroupByClause groupBy, Saxon.Hej.expr.XPathContext context) : base (outputter) {
            this.destination = destination;
            this.groupByClause = groupBy;
            this.context = context;
            comparers = new Saxon.Hej.expr.sort.GenericAtomicComparer[groupBy.comparers.Length];
            for (int i = 0; (i<comparers.Length); i++) {
                comparers[i] = ((/*1430*/Saxon.Hej.expr.sort.GenericAtomicComparer)groupBy.comparers[i].provideContext(context));
            }
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.TupleExpression groupingTupleExpr = groupByClause.getGroupingTupleExpression();
            Saxon.Hej.expr.flwor.TupleExpression retainedTupleExpr = groupByClause.getRetainedTupleExpression();
            Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped otbg = new Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped();
            Saxon.Hej.om.Sequence[] groupingValues = groupingTupleExpr.evaluateItem(context).getMembers();
            checkGroupingValues(groupingValues);
            otbg.groupingValues = new Saxon.Hej.expr.flwor.Tuple(groupingValues);
            otbg.retainedValues = retainedTupleExpr.evaluateItem(context);
            System.Object key = groupByClause.getComparisonKey(otbg.groupingValues, comparers);
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> group = Saxon.Impl.Helpers.MapUtils.Get(map, key);
            addToGroup(key, otbg, group, map);
        }
        protected internal static void addToGroup(System.Object key, Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped objectToBeGrouped, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> group, System.Collections.Generic.Dictionary<System.Object,System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>> map) {
            if ((group != null)) {
                group.Add(objectToBeGrouped);
                Saxon.Impl.Helpers.MapUtils.Put(map, key, group);
            } else {
                System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> list = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped>();
                list.Add(objectToBeGrouped);
                Saxon.Impl.Helpers.MapUtils.Put(map, key, list);
            }
        }
        protected internal static void checkGroupingValues(Saxon.Hej.om.Sequence[] groupingValues) {
            for (int i = 0; (i<groupingValues.Length); i++) {
                Saxon.Hej.om.Sequence v = groupingValues[i];
                if (!((((v is Saxon.Hej.value.EmptySequence) || (v is Saxon.Hej.value.AtomicValue))))) {
                    v = Saxon.Hej.om.SequenceTool.toGroundedValue(Saxon.Hej.expr.Atomizer.getAtomizingIterator(v.iterate(), false));
                    if ((Saxon.Hej.om.SequenceTool.getLength(v)>1)) {
                        throw new Saxon.Hej.trans.XPathException("Grouping key value cannot be a sequence of more than one item", "XPTY0004");
                    }
                    groupingValues[i] = v;
                }
            }
        }
        public override void close() {
            foreach (System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> group in map.Values) {
                groupByClause.processGroup(group, context);
                destination.processTuple(context);
            }
            destination.close();
        }
    
    }
}