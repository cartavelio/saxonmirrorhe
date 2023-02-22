using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class SortExpression : Saxon.Hej.expr.Expression, Saxon.Hej.expr.sort.SortKeyEvaluator {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand sortOp;
        private Saxon.Hej.expr.sort.AtomicComparer[] comparators = null;
        private Saxon.Hej.expr.elab.ItemEvaluator[] sortKeyEvaluators;
        public SortExpression(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT);
            sortOp = new Saxon.Hej.expr.Operand(this, sortKeys, Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE);
            adoptChildExpression(select);
            adoptChildExpression(sortKeys);
        }
        public override string getExpressionName() {
            return "sort";
        }
        public Saxon.Hej.expr.Operand getBaseOperand() {
            return selectOp;
        }
        public Saxon.Hej.expr.Expression getBaseExpression() {
            return getSelect();
        }
        public Saxon.Hej.expr.sort.AtomicComparer[] getComparators() {
            return comparators;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(selectOp, sortOp);
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = getSelect().addToPathMap(pathMap, pathMapNodeSet);
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition sortKeyDefinition in getSortKeyDefinitionList()) {
                if (sortKeyDefinition.isSetContextForSortKey()) {
                    sortKeyDefinition.getSortKey().addToPathMap(pathMap, target);
                } else {
                    sortKeyDefinition.getSortKey().addToPathMap(pathMap, pathMapNodeSet);
                }
                addSortKeyDetailsToPathMap(pathMap, pathMapNodeSet, sortKeyDefinition);
            }
            return target;
        }
        public static void addSortKeyDetailsToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet, Saxon.Hej.expr.sort.SortKeyDefinition skd) {
            Saxon.Hej.expr.Expression e = skd.getOrder();
            if ((e != null)) {
                e.addToPathMap(pathMap, pathMapNodeSet);
            }
            e = skd.getCaseOrder();
            if ((e != null)) {
                e.addToPathMap(pathMap, pathMapNodeSet);
            }
            e = skd.getDataTypeExpression();
            if ((e != null)) {
                e.addToPathMap(pathMap, pathMapNodeSet);
            }
            e = skd.getLanguage();
            if ((e != null)) {
                e.addToPathMap(pathMap, pathMapNodeSet);
            }
            e = skd.getCollationNameExpression();
            if ((e != null)) {
                e.addToPathMap(pathMap, pathMapNodeSet);
            }
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            selectOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.Expression select2 = getSelect();
            if ((select2 != getSelect())) {
                adoptChildExpression(select2);
                setSelect(select2);
            }
            if (!(Saxon.Hej.value.Cardinality.allowsMany(select2.getCardinality()))) {
                return select2;
            }
            Saxon.Hej.type.ItemType sortedItemType = getSelect().getItemType();
            bool allKeysFixed = true;
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition sortKeyDefinition in getSortKeyDefinitionList()) {
                if (!(sortKeyDefinition.isFixed())) {
                    allKeysFixed = false;
                    break ;
                }
            }
            if (allKeysFixed) {
                comparators = new Saxon.Hej.expr.sort.AtomicComparer[getSortKeyDefinitionList().size()];
            }
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            for (int i = 0; (i<getSortKeyDefinitionList().size()); i++) {
                Saxon.Hej.expr.sort.SortKeyDefinition sortKeyDef = getSortKeyDefinition(i);
                Saxon.Hej.expr.Expression sortKey = sortKeyDef.getSortKey();
                if (sortKeyDef.isSetContextForSortKey()) {
                    Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(sortedItemType, false);
                    sortKey = sortKey.typeCheck(visitor, cit);
                } else {
                    sortKey = sortKey.typeCheck(visitor, contextInfo);
                }
                if (sortKeyDef.isBackwardsCompatible()) {
                    sortKey = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(sortKey);
                } else {
                    Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:sort/select", 0, "XTTE1020");
                    sortKey = tc.staticTypeCheck(sortKey, Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC, role, visitor);
                }
                sortKeyDef.setSortKey(sortKey, sortKeyDef.isSetContextForSortKey());
                sortKeyDef.typeCheck(visitor, contextInfo);
                if (sortKeyDef.isFixed()) {
                    Saxon.Hej.expr.sort.AtomicComparer comp = sortKeyDef.makeComparator(visitor.getStaticContext().makeEarlyEvaluationContext());
                    sortKeyDef.setFinalComparator(comp);
                    if (allKeysFixed) {
                        comparators[i] = comp;
                    }
                }
                if ((sortKeyDef.isSetContextForSortKey() && !(Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(sortKey)))) {
                    visitor.getStaticContext().issueWarning("Sort key will have no effect because its value does not depend on the context item", Saxon.Hej.trans.SaxonErrorCode.SXWN9033, sortKey.getLocation());
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            selectOp.optimize(visitor, contextItemType);
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit;
            if (getSortKeyDefinition(0).isSetContextForSortKey()) {
                Saxon.Hej.type.ItemType sortedItemType = getSelect().getItemType();
                cit = visitor.getConfiguration().makeContextItemStaticInfo(sortedItemType, false);
            } else {
                cit = contextItemType;
            }
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition sortKeyDefinition in getSortKeyDefinitionList()) {
                Saxon.Hej.expr.Expression sortKey = sortKeyDefinition.getSortKey();
                sortKey = sortKey.optimize(visitor, cit);
                sortKeyDefinition.setSortKey(sortKey, true);
            }
            if (Saxon.Hej.value.Cardinality.allowsMany(getSelect().getCardinality())) {
                return this;
            } else {
                return getSelect();
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            int len = getSortKeyDefinitionList().size();
            Saxon.Hej.expr.sort.SortKeyDefinition[] sk2 = new Saxon.Hej.expr.sort.SortKeyDefinition[len];
            for (int i = 0; (i<len); i++) {
                sk2[i] = getSortKeyDefinition(i).copy(rebindings);
            }
            Saxon.Hej.expr.sort.SortExpression se2 = new Saxon.Hej.expr.sort.SortExpression(getSelect().copy(rebindings), new Saxon.Hej.expr.sort.SortKeyDefinitionList(sk2));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, se2);
            se2.comparators = comparators;
            return se2;
        }
        public bool isSortKey(Saxon.Hej.expr.Expression child) {
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition sortKeyDefinition in getSortKeyDefinitionList()) {
                Saxon.Hej.expr.Expression exp = sortKeyDefinition.getSortKey();
                if ((exp == child)) {
                    return true;
                }
            }
            return false;
        }
        protected internal override int computeCardinality() {
            return getSelect().getCardinality();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getSelect().getItemType();
        }
        protected internal override int computeSpecialProperties() {
            int props = 0;
            if (getSelect().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) {
                props |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            if (getSelect().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)) {
                props |= Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET;
            }
            if (getSelect().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) {
                props |= Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
            }
            return props;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator iter = getSelect().iterate(context);
            if ((iter is Saxon.Hej.tree.iter.EmptyIterator)) {
                return iter;
            }
            return iterateSorted(iter, context);
        }
        public Saxon.Hej.om.SequenceIterator iterateSorted(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.AtomicComparer[] comps = comparators;
            if ((comparators == null)) {
                int len = getSortKeyDefinitionList().size();
                comps = new Saxon.Hej.expr.sort.AtomicComparer[len];
                for (int s = 0; (s<len); s++) {
                    Saxon.Hej.expr.sort.AtomicComparer comp = getSortKeyDefinition(s).getFinalComparator();
                    if ((comp == null)) {
                        comp = getSortKeyDefinition(s).makeComparator(context);
                    }
                    comps[s] = comp;
                }
            }
            makeSortKeyEvaluators();
            iter = new Saxon.Hej.expr.sort.SortedIterator(context, iter, this, comps, getSortKeyDefinition(0).isSetContextForSortKey());
            ((Saxon.Hej.expr.sort.SortedIterator)(iter)).setHostLanguage(getPackageData().getHostLanguage());
            return iter;
        }
        public void makeSortKeyEvaluators() { lock(this) {
            if ((sortKeyEvaluators == null)) {
                int len = getSortKeyDefinitionList().size();
                sortKeyEvaluators = new Saxon.Hej.expr.elab.ItemEvaluator[len];
                for (int s = 0; (s<len); s++) {
                    sortKeyEvaluators[s] = getSortKeyDefinition(s).getSortKey().makeElaborator().elaborateForItem();
                }
            }
        }}
        public Saxon.Hej.value.AtomicValue evaluateSortKey(int n, Saxon.Hej.expr.XPathContext c) {
            return (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/sortKeyEvaluators[n](c));
        }
        public override string toShortString() {
            return (("sort("+getBaseExpression().toShortString())+")");
        }
        public override string getStreamerName() {
            return "SortExpression";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("sort", this);
            @out.setChildRole("select");
            getSelect().export(@out);
            getSortKeyDefinitionList().export(@out);
            @out.endElement();
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.sort.SortKeyDefinitionList getSortKeyDefinitionList() {
            return (Saxon.Hej.expr.sort.SortKeyDefinitionList)(sortOp.getChildExpression());
        }
        public Saxon.Hej.expr.sort.SortKeyDefinition getSortKeyDefinition(int i) {
            return getSortKeyDefinitionList().getSortKeyDefinition(i);
        }
        public void setSortKeyDefinitionList(Saxon.Hej.expr.sort.SortKeyDefinitionList skd) {
            sortOp.setChildExpression(skd);
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.sort.SortExpression.SortExprElaborator();
        }
        public class SortExprElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.sort.SortExpression expr = (Saxon.Hej.expr.sort.SortExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBaseExpression().makeElaborator().elaborateForPull();
                return context => expr.iterateSorted(/*Delegate.iterate()*/baseEval(context), context);
            }
        
        }
    
    }
}