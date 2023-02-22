using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class GroupByClause : Saxon.Hej.expr.flwor.Clause {
        internal Saxon.Hej.Configuration config;
        internal Saxon.Hej.expr.flwor.LocalVariableBinding[] bindings;
        internal Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers;
        internal Saxon.Hej.expr.Operand retainedTupleOp;
        internal Saxon.Hej.expr.Operand groupingTupleOp;
        public GroupByClause(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.GROUP_BY;
        }
        public override bool containsNonInlineableVariableReference(Saxon.Hej.expr.Binding binding) {
            return (getRetainedTupleExpression().includesBinding(binding) || getGroupingTupleExpression().includesBinding(binding));
        }
        public override Saxon.Hej.expr.flwor.GroupByClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.GroupByClause g2 = new Saxon.Hej.expr.flwor.GroupByClause(config);
            g2.setLocation(getLocation());
            g2.setPackageData(getPackageData());
            g2.bindings = new Saxon.Hej.expr.flwor.LocalVariableBinding[bindings.Length];
            for (int i = 0; (i<bindings.Length); i++) {
                g2.bindings[i] = bindings[i].copy();
            }
            g2.comparers = comparers;
            g2.initRetainedTupleExpression(flwor, (Saxon.Hej.expr.flwor.TupleExpression)(getRetainedTupleExpression().copy(rebindings)));
            g2.initGroupingTupleExpression(flwor, (Saxon.Hej.expr.flwor.TupleExpression)(getGroupingTupleExpression().copy(rebindings)));
            return g2;
        }
        public void initRetainedTupleExpression(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.flwor.TupleExpression expr) {
            retainedTupleOp = new Saxon.Hej.expr.Operand(flwor, expr, Saxon.Hej.expr.OperandRole.FLWOR_TUPLE_CONSTRAINED);
        }
        public void setRetainedTupleExpression(Saxon.Hej.expr.flwor.TupleExpression expr) {
            retainedTupleOp.setChildExpression(expr);
        }
        public Saxon.Hej.expr.flwor.TupleExpression getRetainedTupleExpression() {
            return (Saxon.Hej.expr.flwor.TupleExpression)(retainedTupleOp.getChildExpression());
        }
        public override void optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.LocalVariableBinding> list = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.LocalVariableBinding>(Saxon.Ejava.util.Arrays.asList(bindings));
            System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> retainingExpr = new System.Collections.Generic.List<Saxon.Hej.expr.LocalVariableReference>();
            foreach (Saxon.Hej.expr.Operand o in getRetainedTupleExpression().operands()) {
                retainingExpr.Add((Saxon.Hej.expr.LocalVariableReference)(o.getChildExpression()));
            }
            int groupingSize = getGroupingTupleExpression().getSize();
            for (int i = (list.Count-1); (i>=groupingSize); i--) {
                if ((list[i].getNominalReferenceCount() == 0)) {
                    Saxon.Impl.Helpers.ListUtils.Remove(list, i);
                    Saxon.Impl.Helpers.ListUtils.Remove(retainingExpr, (i-groupingSize));
                }
            }
            bindings = list.ToArray();
            getRetainedTupleExpression().setVariables(retainingExpr);
        }
        public void initGroupingTupleExpression(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.flwor.TupleExpression expr) {
            groupingTupleOp = new Saxon.Hej.expr.Operand(flwor, expr, Saxon.Hej.expr.OperandRole.FLWOR_TUPLE_CONSTRAINED);
        }
        public void setGroupingTupleExpression(Saxon.Hej.expr.flwor.TupleExpression expr) {
            groupingTupleOp.setChildExpression(expr);
        }
        public Saxon.Hej.expr.flwor.TupleExpression getGroupingTupleExpression() {
            return (Saxon.Hej.expr.flwor.TupleExpression)(groupingTupleOp.getChildExpression());
        }
        public void setVariableBindings(Saxon.Hej.expr.flwor.LocalVariableBinding[] bindings) {
            this.bindings = bindings;
        }
        public override Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            return bindings;
        }
        public void setComparers(Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers) {
            this.comparers = comparers;
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.GroupByClausePull(@base, this, context);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.GroupByClausePush(output, destination, this, context);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(groupingTupleOp);
            /*Delegate.processOperand()*/processor(retainedTupleOp);
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("group-by");
            foreach (Saxon.Hej.expr.Operand o in getRetainedTupleExpression().operands()) {
                Saxon.Hej.expr.LocalVariableReference @ref = (Saxon.Hej.expr.LocalVariableReference)(o.getChildExpression());
                @out.startSubsidiaryElement("by");
                @out.emitAttribute("var", @ref.getDisplayName());
                @out.emitAttribute("slot", (@ref.getBinding().getLocalSlotNumber()+""));
                @out.endSubsidiaryElement();
            }
            @out.endElement();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("group by ... ");
            return fsb.ToString();
        }
        public class ObjectToBeGrouped {
            public Saxon.Hej.expr.flwor.Tuple groupingValues;
            public Saxon.Hej.expr.flwor.Tuple retainedValues;
        
        }
        public void processGroup(System.Collections.Generic.IList<Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped> group, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.LocalVariableBinding[] bindings = getRangeVariables();
            Saxon.Hej.om.Sequence[] groupingValues = group[0].groupingValues.getMembers();
            for (int j = 0; (j<groupingValues.Length); j++) {
                Saxon.Hej.om.Sequence v = groupingValues[j];
                context.setLocalVariable(bindings[j].getLocalSlotNumber(), v);
            }
            for (int j = groupingValues.Length; (j<bindings.Length); j++) {
                System.Collections.Generic.IList<Saxon.Hej.om.Item> concatenatedValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                foreach (Saxon.Hej.expr.flwor.GroupByClause.ObjectToBeGrouped otbg in group) {
                    Saxon.Hej.om.Sequence val = otbg.retainedValues.getMembers()[(j-groupingValues.Length)];
                    Saxon.Hej.om.SequenceIterator si = val.iterate();
                    Saxon.Hej.om.Item it;
                    while (((it = si.next()) != null)) {
                        concatenatedValue.Add(it);
                    }
                }
                Saxon.Hej.value.SequenceExtent se = new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(concatenatedValue);
                context.setLocalVariable(bindings[j].getLocalSlotNumber(), se);
            }
        }
        public Saxon.Hej.expr.flwor.GroupByClause.TupleComparisonKey getComparisonKey(Saxon.Hej.expr.flwor.Tuple t, Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers) {
            return new Saxon.Hej.expr.flwor.GroupByClause.TupleComparisonKey(t.getMembers(), comparers);
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot use document projection with group-by");
        }
        public class TupleComparisonKey {
            private readonly Saxon.Hej.om.Sequence[] groupingValues;
            private readonly Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers;
            public TupleComparisonKey(Saxon.Hej.om.Sequence[] groupingValues, Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers)  {
                this.groupingValues = groupingValues;
                this.comparers = comparers;
            }
            public override int GetHashCode() {
                int h = (0x77557755^groupingValues.Length);
                for (int i = 0; (i<groupingValues.Length); i++) {
                    Saxon.Hej.expr.sort.GenericAtomicComparer comparer = comparers[i];
                    int implicitTimezone = comparer.getContext().getImplicitTimezone();
                    try {
                        Saxon.Hej.om.SequenceIterator atoms = groupingValues[i].iterate();
                        while (true) {
                            Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(atoms.next());
                            if ((val == null)) {
                                break ;
                            }
                            h ^= (i+val.getXPathMatchKey(comparer.getCollator(), implicitTimezone).GetHashCode());
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                    }
                }
                return h;
            }
            public override bool Equals(System.Object other) {
                if (!(((other is Saxon.Hej.expr.flwor.GroupByClause.TupleComparisonKey)))) {
                    return false;
                }
                if ((groupingValues.Length != ((Saxon.Hej.expr.flwor.GroupByClause.TupleComparisonKey)(other)).groupingValues.Length)) {
                    return false;
                }
                for (int i = 0; (i<groupingValues.Length); i++) {
                    try {
                        if (!(Saxon.Hej.functions.DeepEqual.deepEqual(groupingValues[i].iterate(), ((Saxon.Hej.expr.flwor.GroupByClause.TupleComparisonKey)(other)).groupingValues[i].iterate(), comparers[i], comparers[i].getContext(), 0))) {
                            return false;
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        return false;
                    }
                }
                return true;
            }
        
        }
    
    }
}