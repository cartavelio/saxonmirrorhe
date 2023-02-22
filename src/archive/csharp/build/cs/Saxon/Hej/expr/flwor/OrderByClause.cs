using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class OrderByClause : Saxon.Hej.expr.flwor.Clause {
        public readonly static Saxon.Hej.expr.OperandRole SORT_KEYS_ROLE = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.HIGHER_ORDER|Saxon.Hej.expr.OperandRole.CONSTRAINED_CLASS), Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE, expr => (expr is Saxon.Hej.expr.sort.SortKeyDefinitionList));
        internal Saxon.Hej.expr.Operand sortKeysOp;
        internal Saxon.Hej.expr.sort.AtomicComparer[] comparators;
        internal Saxon.Hej.expr.Operand tupleOp;
        public OrderByClause(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.sort.SortKeyDefinition[] sortKeys, Saxon.Hej.expr.flwor.TupleExpression tupleExpression)  {
            this.sortKeysOp = new Saxon.Hej.expr.Operand(flwor, new Saxon.Hej.expr.sort.SortKeyDefinitionList(sortKeys), SORT_KEYS_ROLE);
            this.tupleOp = new Saxon.Hej.expr.Operand(flwor, tupleExpression, Saxon.Hej.expr.OperandRole.FLWOR_TUPLE_CONSTRAINED);
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.ORDER_BY;
        }
        public override bool containsNonInlineableVariableReference(Saxon.Hej.expr.Binding binding) {
            return getTupleExpression().includesBinding(binding);
        }
        public override Saxon.Hej.expr.flwor.OrderByClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = getSortKeyDefinitions();
            Saxon.Hej.expr.sort.SortKeyDefinition[] sk2 = new Saxon.Hej.expr.sort.SortKeyDefinition[sortKeys.size()];
            for (int i = 0; (i<sortKeys.size()); i++) {
                sk2[i] = sortKeys.getSortKeyDefinition(i).copy(rebindings);
            }
            Saxon.Hej.expr.flwor.OrderByClause obc = new Saxon.Hej.expr.flwor.OrderByClause(flwor, sk2, (Saxon.Hej.expr.flwor.TupleExpression)(getTupleExpression().copy(rebindings)));
            obc.setLocation(getLocation());
            obc.setPackageData(getPackageData());
            obc.comparators = comparators;
            return obc;
        }
        public Saxon.Hej.expr.sort.SortKeyDefinitionList getSortKeyDefinitions() {
            return (Saxon.Hej.expr.sort.SortKeyDefinitionList)(sortKeysOp.getChildExpression());
        }
        public Saxon.Hej.expr.sort.AtomicComparer[] getAtomicComparers() {
            return comparators;
        }
        public Saxon.Hej.expr.flwor.TupleExpression getTupleExpression() {
            return (Saxon.Hej.expr.flwor.TupleExpression)(tupleOp.getChildExpression());
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.OrderByClausePull(@base, getTupleExpression(), this, context);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.OrderByClausePush(output, destination, getTupleExpression(), this, context);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(tupleOp);
            /*Delegate.processOperand()*/processor(sortKeysOp);
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            bool allKeysFixed = true;
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = getSortKeyDefinitions();
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition sk in sortKeys) {
                if (!(sk.isFixed())) {
                    allKeysFixed = false;
                    break ;
                }
            }
            if (allKeysFixed) {
                comparators = new Saxon.Hej.expr.sort.AtomicComparer[sortKeys.size()];
            }
            int i = 0;
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in sortKeys) {
                Saxon.Hej.expr.Expression sortKey = skd.getSortKey();
                int pos = i;
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => {
                    Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.ORDER_BY, "", pos);
                    role0.setErrorCode("XPTY0004");
                    return role0;
                } ;
                sortKey = tc.staticTypeCheck(sortKey, Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC, role, visitor);
                skd.setSortKey(sortKey, false);
                skd.typeCheck(visitor, contextInfo);
                if (skd.isFixed()) {
                    Saxon.Hej.expr.sort.AtomicComparer comp = skd.makeComparator(visitor.getStaticContext().makeEarlyEvaluationContext());
                    skd.setFinalComparator(comp);
                    if (allKeysFixed) {
                        comparators[i] = comp;
                    }
                }
                i++;
            }
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = getSortKeyDefinitions();
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in sortKeys) {
                Saxon.Hej.expr.Expression sortKey = skd.getSortKey();
                sortKey.addToPathMap(pathMap, pathMapNodeSet);
            }
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("order-by");
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition k in getSortKeyDefinitions()) {
                @out.startSubsidiaryElement("key");
                k.getSortKey().export(@out);
                @out.endSubsidiaryElement();
            }
            @out.endElement();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("order by ... ");
            return fsb.ToString();
        }
        public Saxon.Hej.value.AtomicValue evaluateSortKey(int n, Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = getSortKeyDefinitions();
            return (Saxon.Hej.value.AtomicValue)(sortKeys.getSortKeyDefinition(n).getSortKey().evaluateItem(c));
        }
    
    }
}