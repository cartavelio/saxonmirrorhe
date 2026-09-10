using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class FLWORExpression : Saxon.Hej.expr.Expression {
        public System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauses;
        public Saxon.Hej.expr.Operand returnClauseOp;
        public FLWORExpression()  {
        }
        public void init(System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauses, Saxon.Hej.expr.Expression returnClause) {
            this.clauses = clauses;
            bool looping = false;
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                if (isLoopingClause(c)) {
                    looping = true;
                    break ;
                }
            }
            this.returnClauseOp = new Saxon.Hej.expr.Operand(this, returnClause, (looping ? REPEATED_RETURN : SINGLE_RETURN));
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> getClauseList() {
            return clauses;
        }
        public static bool isLoopingClause(Saxon.Hej.expr.flwor.Clause c) {
            return (((c.getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.FOR) || (c.getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.GROUP_BY)) || (c.getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.WINDOW));
        }
        public Saxon.Hej.expr.Expression getReturnClause() {
            return returnClauseOp.getChildExpression();
        }
        public override bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                if (clauseHasBinding(c, binding)) {
                    return true;
                }
            }
            return false;
        }
        private bool clauseHasBinding(Saxon.Hej.expr.flwor.Clause c, Saxon.Hej.expr.Binding binding) {
            foreach (Saxon.Hej.expr.Binding b in c.getRangeVariables()) {
                if ((b == binding)) {
                    return true;
                }
            }
            return false;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.flwor.OperandProcessor simplifier = op => op.setChildExpression(op.getChildExpression().simplify());
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                c.processOperands(simplifier);
            }
            returnClauseOp.setChildExpression(getReturnClause().simplify());
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.flwor.OperandProcessor typeChecker = op => op.typeCheck(visitor, contextInfo);
            for (int i = 0; (i<clauses.Count); i++) {
                clauses[i].processOperands(typeChecker);
                clauses[i].typeCheck(visitor, contextInfo);
                Saxon.Hej.expr.flwor.LocalVariableBinding[] bindings = clauses[i].getRangeVariables();
                foreach (Saxon.Hej.expr.Binding b in bindings) {
                    System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references = new System.Collections.Generic.List<Saxon.Hej.expr.VariableReference>();
                    for (int j = i; (j<clauses.Count); j++) {
                        clauses[j].gatherVariableReferences(visitor, b, references);
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.gatherVariableReferences(getReturnClause(), b, references);
                    clauses[i].refineVariableType(visitor, references, getReturnClause());
                }
            }
            returnClauseOp.typeCheck(visitor, contextInfo);
            return this;
        }
        public override bool implementsStaticTypeCheck() {
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                switch (c.getClauseKey()) {
                  case Saxon.Hej.expr.flwor.Clause.ClauseName.LET:
                  case Saxon.Hej.expr.flwor.Clause.ClauseName.WHERE:
                    continue ;
                  default:
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.value.SequenceType req, bool backwardsCompatible, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(backwardsCompatible);
            returnClauseOp.setChildExpression(tc.staticTypeCheck(getReturnClause(), req, role, visitor));
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getReturnClause().getItemType();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(5);
            try {
                foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                    c.processOperands(op => list.Add(op));
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e);
            }
            list.Add(returnClauseOp);
            return list;
        }
        private static readonly Saxon.Hej.expr.OperandRole SINGLE_RETURN = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        private static readonly Saxon.Hej.expr.OperandRole REPEATED_RETURN = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.HIGHER_ORDER, Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public override void checkForUpdatingSubexpressions() {
            Saxon.Hej.expr.flwor.OperandProcessor processor = op => {
                op.getChildExpression().checkForUpdatingSubexpressions();
                if (op.getChildExpression().isUpdatingExpression()) {
                    throw new Saxon.Hej.trans.XPathException("An updating expression cannot be used in a clause of a FLWOR expression", "XUST0001");
                }
            } ;
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                c.processOperands(processor);
            }
            getReturnClause().checkForUpdatingSubexpressions();
        }
        public override bool isUpdatingExpression() {
            return getReturnClause().isUpdatingExpression();
        }
        public override int getImplementationMethod() {
            return (ITERATE_METHOD|PROCESS_METHOD);
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                c.addToPathMap(pathMap, pathMapNodeSet);
            }
            return getReturnClause().addToPathMap(pathMap, pathMapNodeSet);
        }
        public void injectCode(Saxon.Hej.expr.parser.CodeInjector injector) {
            if ((injector != null)) {
                foreach (Saxon.Hej.expr.flwor.Clause clause in clauses) {
                    if ((clause is Saxon.Hej.expr.flwor.TraceClause)) {
                        return ;
                    }
                }
                System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> expandedList = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.Clause>((clauses.Count*2));
                expandedList.Add(clauses[0]);
                for (int i = 1; (i<clauses.Count); i++) {
                    Saxon.Hej.expr.flwor.Clause extraClause = injector.injectClause(this, clauses[(i-1)]);
                    if ((extraClause != null)) {
                        expandedList.Add(extraClause);
                    }
                    expandedList.Add(clauses[i]);
                }
                Saxon.Hej.expr.flwor.Clause extra = injector.injectClause(this, clauses[(clauses.Count-1)]);
                if ((extra != null)) {
                    expandedList.Add(extra);
                }
                clauses = expandedList;
                returnClauseOp.setChildExpression(Saxon.Hej.expr.parser.ExpressionTool.injectCode(returnClauseOp.getChildExpression(), injector));
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("FLWOR", this);
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                c.explain(@out);
            }
            @out.startSubsidiaryElement("return");
            getReturnClause().export(@out);
            @out.endSubsidiaryElement();
            @out.endElement();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> newClauses = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.Clause>();
            Saxon.Hej.expr.flwor.FLWORExpression f2 = new Saxon.Hej.expr.flwor.FLWORExpression();
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                Saxon.Hej.expr.flwor.Clause c2 = c.copy(f2, rebindings);
                c2.setLocation(c.getLocation());
                c2.setRepeated(c.isRepeated());
                Saxon.Hej.expr.flwor.LocalVariableBinding[] oldBindings = c.getRangeVariables();
                Saxon.Hej.expr.flwor.LocalVariableBinding[] newBindings = c2.getRangeVariables();
                System.Diagnostics.Debug.Assert((oldBindings.Length == newBindings.Length));
                for (int i = 0; (i<oldBindings.Length); i++) {
                    rebindings.put(oldBindings[i], newBindings[i]);
                }
                newClauses.Add(c2);
            }
            f2.init(newClauses, getReturnClause().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f2);
            return f2;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                if (((c is Saxon.Hej.expr.flwor.ForClause) && (((Saxon.Hej.expr.flwor.ForClause)(c)).getPositionVariable() == null))) {
                    ((Saxon.Hej.expr.flwor.ForClause)(c)).setSequence(((Saxon.Hej.expr.flwor.ForClause)(c)).getSequence().unordered(retainAllNodes, forStreaming));
                }
            }
            returnClauseOp.setChildExpression(getReturnClause().unordered(retainAllNodes, forStreaming));
            return this;
        }
        private Saxon.Hej.expr.Binding[] extendBindingList(Saxon.Hej.expr.Binding[] bindings, Saxon.Hej.expr.flwor.LocalVariableBinding[] moreBindings) {
            if ((bindings == null)) {
                bindings = new Saxon.Hej.expr.Binding[0];
            }
            if (((moreBindings == null) || (moreBindings.Length == 0))) {
                return bindings;
            } else {
                Saxon.Hej.expr.Binding[] b2 = new Saxon.Hej.expr.Binding[(bindings.Length+moreBindings.Length)];
                Saxon.Ejava.lang.System.arraycopy(bindings, 0, b2, 0, bindings.Length);
                Saxon.Ejava.lang.System.arraycopy(moreBindings, 0, b2, bindings.Length, moreBindings.Length);
                return b2;
            }
        }
        public override int getEvaluationMethod() {
            return Saxon.Hej.expr.Expression.PROCESS_METHOD;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                c.processOperands(op => op.optimize(visitor, contextItemType));
                c.optimize(visitor, contextItemType);
            }
            returnClauseOp.setChildExpression(getReturnClause().optimize(visitor, contextItemType));
            if ((clauses.Count == 1)) {
                Saxon.Hej.expr.flwor.Clause c = clauses[0];
                if (((c is Saxon.Hej.expr.flwor.LetClause) || ((((c is Saxon.Hej.expr.flwor.ForClause) && !(((c is Saxon.Hej.expr.flwor.ForMemberClause)))) && (((Saxon.Hej.expr.flwor.ForClause)(c)).getPositionVariable() == null))))) {
                    return rewriteForOrLet(visitor, contextItemType);
                }
            }
            bool tryAgain;
            bool changed = false;
            do {
                tryAgain = false;
                foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                    if ((c.getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.LET)) {
                        Saxon.Hej.expr.flwor.LetClause lc = (Saxon.Hej.expr.flwor.LetClause)(c);
                        if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(this, new Saxon.Hej.expr.Binding[]{lc.getRangeVariable()}))) {
                            Saxon.Impl.Helpers.ListUtils.Remove(clauses, c);
                            opt.trace(("Removed unused variable "+lc.getRangeVariable().getVariableQName().getDisplayName()), this);
                            tryAgain = true;
                            break ;
                        }
                        bool suppressInlining = false;
                        foreach (Saxon.Hej.expr.flwor.Clause c2 in clauses) {
                            if (c2.containsNonInlineableVariableReference(lc.getRangeVariable())) {
                                suppressInlining = true;
                                break ;
                            }
                        }
                        if (!(suppressInlining)) {
                            bool oneRef = (lc.getRangeVariable().getNominalReferenceCount() == 1);
                            bool simpleSeq = ((lc.getSequence() is Saxon.Hej.expr.VariableReference) || (lc.getSequence() is Saxon.Hej.expr.Literal));
                            if ((oneRef || simpleSeq)) {
                                Saxon.Hej.expr.parser.ExpressionTool.replaceVariableReferences(this, lc.getRangeVariable(), lc.getSequence(), true);
                                Saxon.Impl.Helpers.ListUtils.Remove(clauses, c);
                                opt.trace(("Inlined variable "+lc.getRangeVariable().getVariableQName().getDisplayName()), this);
                                if (clauses.Count==0) {
                                    return getReturnClause();
                                }
                                tryAgain = true;
                                break ;
                            }
                        }
                    }
                }
                changed |= tryAgain;
            } while (tryAgain);
            if (changed) {
                for (int i = (clauses.Count-1); (i>=1); i--) {
                    if (((clauses[i].getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.TRACE) && (clauses[(i-1)].getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.TRACE))) {
                        Saxon.Impl.Helpers.ListUtils.Remove(clauses, i);
                    }
                }
            }
            bool depends = false;
            foreach (Saxon.Hej.expr.flwor.Clause w in clauses) {
                if (((w is Saxon.Hej.expr.flwor.WhereClause) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(((Saxon.Hej.expr.flwor.WhereClause)(w)).getPredicate()))) {
                    depends = true;
                    break ;
                }
            }
            if ((depends && (contextItemType != null))) {
                Saxon.Hej.expr.Expression expr1 = Saxon.Hej.expr.parser.ExpressionTool.tryToFactorOutDot(this, contextItemType.getItemType());
                if (((expr1 == null) || (expr1 == this))) {
                    return this;
                }
                resetLocalStaticProperties();
                return expr1.optimize(visitor, contextItemType);
            }
            Saxon.Hej.expr.Expression expr2 = rewriteWhereClause(visitor, contextItemType);
            if (((expr2 != null) && (expr2 != this))) {
                return expr2.optimize(visitor, contextItemType);
            }
            bool allForOrLetExpr = true;
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                if (((c is Saxon.Hej.expr.flwor.ForClause) && !(((c is Saxon.Hej.expr.flwor.ForMemberClause))))) {
                    if ((((Saxon.Hej.expr.flwor.ForClause)(c)).getPositionVariable() != null)) {
                        allForOrLetExpr = false;
                        break ;
                    }
                } else {
                    if (!(((c is Saxon.Hej.expr.flwor.LetClause)))) {
                        allForOrLetExpr = false;
                        break ;
                    }
                }
            }
            if (allForOrLetExpr) {
                return rewriteForOrLet(visitor, contextItemType);
            }
            return this;
        }
        private class WhereClauseStruct {
            internal int whereIndex = 0;
            internal Saxon.Hej.expr.flwor.WhereClause whereClause;
        
        }
        private Saxon.Hej.expr.Expression rewriteWhereClause(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.flwor.WhereClause whereClause;
            int whereIndex = 0;
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.FLWORExpression.WhereClauseStruct> whereList = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.FLWORExpression.WhereClauseStruct>();
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                if ((c is Saxon.Hej.expr.flwor.WhereClause)) {
                    Saxon.Hej.expr.flwor.FLWORExpression.WhereClauseStruct wStruct = new Saxon.Hej.expr.flwor.FLWORExpression.WhereClauseStruct();
                    wStruct.whereClause = (Saxon.Hej.expr.flwor.WhereClause)(c);
                    wStruct.whereIndex = (clauses.Count-whereIndex);
                    whereList.Add(wStruct);
                }
                whereIndex++;
            }
            if ((whereList.Count == 0)) {
                return null;
            }
            while (!(whereList.Count==0)) {
                whereClause = whereList[0].whereClause;
                whereIndex = whereList[0].whereIndex;
                Saxon.Hej.expr.Expression condition = whereClause.getPredicate();
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(5);
                Saxon.Hej.expr.BooleanExpression.listAndComponents(condition, list);
                for (int i = (list.Count-1); (i>=0); i--) {
                    Saxon.Hej.expr.Expression term = list[i];
                    for (int c = ((clauses.Count-whereIndex)-1); (c>=0); c--) {
                        Saxon.Hej.expr.flwor.Clause clause = clauses[c];
                        Saxon.Hej.expr.Binding[] bindingList = clause.getRangeVariables();
                        if ((Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(term, bindingList) || (clause.getClauseKey() == Saxon.Hej.expr.flwor.Clause.ClauseName.COUNT))) {
                            Saxon.Hej.expr.Expression removedExpr = Saxon.Impl.Helpers.ListUtils.Remove(list, i);
                            if (list.Count==0) {
                                Saxon.Impl.Helpers.ListUtils.Remove(clauses, (clauses.Count-whereIndex));
                            } else {
                                whereClause.setPredicate(makeAndCondition(list));
                            }
                            if ((((clause is Saxon.Hej.expr.flwor.ForClause)) && !(((Saxon.Hej.expr.flwor.ForClause)(clause)).isAllowingEmpty()))) {
                                bool added = ((Saxon.Hej.expr.flwor.ForClause)(clause)).addPredicate(this, visitor, contextItemType, term);
                                if (!(added)) {
                                    Saxon.Hej.expr.flwor.WhereClause newWhere = new Saxon.Hej.expr.flwor.WhereClause(this, removedExpr);
                                    newWhere.setLocation(clause.getLocation());
                                    clauses.Insert((c+1), newWhere);
                                }
                            } else {
                                Saxon.Hej.expr.flwor.WhereClause newWhere = new Saxon.Hej.expr.flwor.WhereClause(this, term);
                                newWhere.setLocation(clause.getLocation());
                                clauses.Insert((c+1), newWhere);
                            }
                            break ;
                        }
                    }
                    if (((list.Count-1) == i)) {
                        Saxon.Impl.Helpers.ListUtils.Remove(list, i);
                        if (list.Count==0) {
                            Saxon.Impl.Helpers.ListUtils.Remove(clauses, (clauses.Count-whereIndex));
                        } else {
                            whereClause.setPredicate(makeAndCondition(list));
                        }
                        Saxon.Hej.expr.flwor.WhereClause newWhere = new Saxon.Hej.expr.flwor.WhereClause(this, term);
                        newWhere.setLocation(condition.getLocation());
                        clauses.Insert(0, newWhere);
                    }
                }
                Saxon.Impl.Helpers.ListUtils.Remove(whereList, 0);
            }
            return this;
        }
        private Saxon.Hej.expr.Expression makeAndCondition(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if ((list.Count == 1)) {
                return list[0];
            } else {
                return new Saxon.Hej.expr.AndExpression(list[0], makeAndCondition(Saxon.Impl.Helpers.ListUtils.SubList(list, 1, list.Count)));
            }
        }
        private Saxon.Hej.expr.Expression rewriteForOrLet(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression action = getReturnClause();
            Saxon.Hej.expr.parser.CodeInjector injector = null;
            if ((visitor.getStaticContext() is Saxon.Hej.query.QueryModule)) {
                injector = ((Saxon.Hej.query.QueryModule)(visitor.getStaticContext())).getCodeInjector();
            }
            for (int i = (clauses.Count-1); (i>=0); i--) {
                if ((clauses[i] is Saxon.Hej.expr.flwor.ForClause)) {
                    Saxon.Hej.expr.flwor.ForClause forClause = (Saxon.Hej.expr.flwor.ForClause)(clauses[i]);
                    Saxon.Hej.expr.ForExpression forExpr;
                    if (forClause.isAllowingEmpty()) {
                        forExpr = new Saxon.Hej.expr.flwor.OuterForExpression();
                    } else {
                        forExpr = new Saxon.Hej.expr.ForExpression();
                    }
                    forExpr.setLocation(forClause.getLocation());
                    forExpr.setRetainedStaticContext(getRetainedStaticContext());
                    forExpr.setAction(action);
                    forExpr.setSequence(forClause.getSequence());
                    forExpr.setVariableQName(forClause.getRangeVariable().getVariableQName());
                    forExpr.setRequiredType(forClause.getRangeVariable().getRequiredType());
                    Saxon.Hej.expr.parser.ExpressionTool.rebindVariableReferences(action, forClause.getRangeVariable(), forExpr);
                    action = forExpr;
                } else {
                    Saxon.Hej.expr.flwor.LetClause letClause = (Saxon.Hej.expr.flwor.LetClause)(clauses[i]);
                    Saxon.Hej.expr.LetExpression letExpr = new Saxon.Hej.expr.LetExpression();
                    letExpr.setLocation(letClause.getLocation());
                    letExpr.setRetainedStaticContext(getRetainedStaticContext());
                    letExpr.setAction(action);
                    letExpr.setSequence(letClause.getSequence());
                    letExpr.setVariableQName(letClause.getRangeVariable().getVariableQName());
                    letExpr.setRequiredType(letClause.getRangeVariable().getRequiredType());
                    if (letClause.getRangeVariable().isIndexedVariable()) {
                        letExpr.setIndexedVariable();
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.rebindVariableReferences(action, letClause.getRangeVariable(), letExpr);
                    action = letExpr;
                }
            }
            action = action.typeCheck(visitor, contextItemType);
            action = action.optimize(visitor, contextItemType);
            return action;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.TuplePull stream = new Saxon.Hej.expr.flwor.SingularityPull();
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                stream = c.getPullStream(stream, context);
            }
            return new Saxon.Hej.expr.flwor.ReturnClauseIterator(stream, this, context);
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.TuplePush destination = new Saxon.Hej.expr.flwor.ReturnClausePush(output, getReturnClause());
            for (int i = (clauses.Count-1); (i>=0); i--) {
                Saxon.Hej.expr.flwor.Clause c = clauses[i];
                destination = c.getPushStream(destination, output, context);
            }
            destination.processTuple(context);
            destination.close();
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            Saxon.Hej.expr.flwor.TuplePull stream = new Saxon.Hej.expr.flwor.SingularityPull();
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                stream = c.getPullStream(stream, context);
            }
            while (stream.nextTuple(context)) {
                getReturnClause().evaluatePendingUpdates(context, pul);
            }
        }
        public override string getExpressionName() {
            return "FLWOR";
        }
        public override string toShortString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            sb.append(clauses[0].toShortString());
            sb.append(" ... return ");
            sb.append(getReturnClause().toShortString());
            return sb.ToString();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            foreach (Saxon.Hej.expr.flwor.Clause c in clauses) {
                sb.append(c.ToString());
                sb.append(' ');
            }
            sb.append(" return ");
            sb.append(getReturnClause().ToString());
            return sb.ToString();
        }
        public bool hasLoopingVariableReference(Saxon.Hej.expr.Binding binding) {
            int bindingClause = -1;
            for (int i = 0; (i<clauses.Count); i++) {
                if (clauseHasBinding(clauses[i], binding)) {
                    bindingClause = i;
                    break ;
                }
            }
            bool boundOutside = (bindingClause<0);
            if (boundOutside) {
                bindingClause = 0;
            }
            int lastReferencingClause = clauses.Count;
            if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(getReturnClause(), new Saxon.Hej.expr.Binding[]{binding}))) {
                System.Collections.Generic.IList<bool> response = new System.Collections.Generic.List<bool>();
                Saxon.Hej.expr.flwor.OperandProcessor checker = op => {
                    if ((response.Count==0 && Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(op.getChildExpression(), new Saxon.Hej.expr.Binding[]{binding}))) {
                        response.Add(true);
                    }
                } ;
                for (int i = (clauses.Count-1); (i>=0); i--) {
                    try {
                        clauses[i].processOperands(checker);
                        if (!(response.Count==0)) {
                            lastReferencingClause = i;
                            break ;
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
            }
            for (int i = (lastReferencingClause-1); (i>=bindingClause); i--) {
                if (isLoopingClause(clauses[i])) {
                    return true;
                }
            }
            return false;
        }
    
    }
}