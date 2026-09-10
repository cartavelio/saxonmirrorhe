using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ForEachGroup : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.sort.SortKeyEvaluator, Saxon.Hej.expr.ContextSwitchingExpression {
        public const int GROUP_BY = 0;
        public const int GROUP_ADJACENT = 1;
        public const int GROUP_STARTING = 2;
        public const int GROUP_ENDING = 3;
        public const int GROUP_BREAKING_WHEN = 4;
        private readonly byte algorithm;
        private int keyItemType;
        private Saxon.Hej.lib.StringCollator collator = null;
        private Saxon.Hej.expr.sort.AtomicComparer[] sortComparators = null;
        private bool composite = false;
        private bool inFork = false;
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand actionOp;
        private readonly Saxon.Hej.expr.Operand keyOp;
        private Saxon.Hej.expr.Operand collationOp;
        private Saxon.Hej.expr.Operand sortKeysOp;
        public ForEachGroup(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression action, byte algorithm, Saxon.Hej.expr.Expression key, Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.Expression collationNameExpression, Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT);
            actionOp = new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
            Saxon.Hej.expr.OperandRole keyRole = ((((algorithm == GROUP_ENDING) || (algorithm == GROUP_STARTING))) ? Saxon.Hej.expr.OperandRole.PATTERN : Saxon.Hej.expr.OperandRole.NEW_FOCUS_ATOMIC);
            keyOp = new Saxon.Hej.expr.Operand(this, key, keyRole);
            if ((collationNameExpression != null)) {
                collationOp = new Saxon.Hej.expr.Operand(this, collationNameExpression, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((sortKeys != null)) {
                sortKeysOp = new Saxon.Hej.expr.Operand(this, sortKeys, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            this.algorithm = algorithm;
            this.collator = collator;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                adoptChildExpression(o.getChildExpression());
            }
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_FOR_EACH_GROUP;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(selectOp, actionOp, keyOp, collationOp, sortKeysOp);
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return actionOp.getChildExpression();
        }
        public byte getAlgorithm() {
            return algorithm;
        }
        public Saxon.Hej.expr.Expression getGroupingKey() {
            return keyOp.getChildExpression();
        }
        public int getKeyItemType() {
            return keyItemType;
        }
        public Saxon.Hej.expr.sort.SortKeyDefinitionList getSortKeyDefinitions() {
            return ((sortKeysOp == null) ? null : (Saxon.Hej.expr.sort.SortKeyDefinitionList)(sortKeysOp.getChildExpression()));
        }
        public Saxon.Hej.expr.sort.AtomicComparer[] getSortKeyComparators() {
            return sortComparators;
        }
        public Saxon.Hej.lib.StringCollator getCollation() {
            return collator;
        }
        public System.Uri getBaseURI() {
            try {
                return getRetainedStaticContext().getStaticBaseUri();
            } catch (Saxon.Hej.trans.XPathException err) {
                return null;
            }
        }
        public bool isComposite() {
            return composite;
        }
        public void setComposite(bool composite) {
            this.composite = composite;
        }
        public bool isInFork() {
            return inFork;
        }
        public void setIsInFork(bool inFork) {
            this.inFork = inFork;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            selectOp.typeCheck(visitor, contextInfo);
            if ((collationOp != null)) {
                collationOp.typeCheck(visitor, contextInfo);
            }
            Saxon.Hej.type.ItemType selectedItemType = getSelectExpression().getItemType();
            if ((selectedItemType == Saxon.Hej.type.ErrorType.getInstance())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                fixupGroupReferences(this, this, selectedItemType, false);
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(selectedItemType, false);
            cit.setContextSettingExpression(getSelectExpression());
            actionOp.typeCheck(visitor, cit);
            keyOp.typeCheck(visitor, cit);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSelectExpression())) {
                return getSelectExpression();
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getActionExpression())) {
                return getActionExpression();
            }
            if ((getSortKeyDefinitions() != null)) {
                bool allFixed = true;
                foreach (Saxon.Hej.expr.sort.SortKeyDefinition sk in getSortKeyDefinitions()) {
                    Saxon.Hej.expr.Expression sortKey = sk.getSortKey();
                    sortKey = sortKey.typeCheck(visitor, cit);
                    if (sk.isBackwardsCompatible()) {
                        sortKey = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(sortKey);
                    } else {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:sort/select", 0);
                        role.setErrorCode("XTTE1020");
                        sortKey = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(sortKey, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE, role);
                    }
                    sk.setSortKey(sortKey, true);
                    sk.typeCheck(visitor, contextInfo);
                    if (sk.isFixed()) {
                        Saxon.Hej.expr.sort.AtomicComparer comp = sk.makeComparator(visitor.getStaticContext().makeEarlyEvaluationContext());
                        sk.setFinalComparator(comp);
                    } else {
                        allFixed = false;
                    }
                }
                if (allFixed) {
                    sortComparators = new Saxon.Hej.expr.sort.AtomicComparer[getSortKeyDefinitions().size()];
                    for (int i = 0; (i<getSortKeyDefinitions().size()); i++) {
                        sortComparators[i] = getSortKeyDefinitions().getSortKeyDefinition(i).getFinalComparator();
                    }
                }
            }
            keyItemType = getGroupingKey().getItemType().getPrimitiveType();
            return this;
        }
        private static void fixupGroupReferences(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.instruct.ForEachGroup feg, Saxon.Hej.type.ItemType selectedItemType, bool isInLoop) {
            if ((exp == null)) {
            } else {
                if ((exp is Saxon.Hej.functions.CurrentGroupCall)) {
                    ((Saxon.Hej.functions.CurrentGroupCall)(exp)).setControllingInstruction(feg, selectedItemType, isInLoop);
                } else {
                    if ((exp is Saxon.Hej.expr.instruct.ForEachGroup)) {
                        Saxon.Hej.expr.instruct.ForEachGroup feg2 = (Saxon.Hej.expr.instruct.ForEachGroup)(exp);
                        if ((feg2 == feg)) {
                            fixupGroupReferences(feg2.getActionExpression(), feg, selectedItemType, false);
                        } else {
                            fixupGroupReferences(feg2.getSelectExpression(), feg, selectedItemType, isInLoop);
                            fixupGroupReferences(feg2.getGroupingKey(), feg, selectedItemType, isInLoop);
                            if ((feg2.getSortKeyDefinitions() != null)) {
                                foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in feg2.getSortKeyDefinitions()) {
                                    fixupGroupReferences(skd.getOrder(), feg, selectedItemType, isInLoop);
                                    fixupGroupReferences(skd.getCaseOrder(), feg, selectedItemType, isInLoop);
                                    fixupGroupReferences(skd.getDataTypeExpression(), feg, selectedItemType, isInLoop);
                                    fixupGroupReferences(skd.getLanguage(), feg, selectedItemType, isInLoop);
                                    fixupGroupReferences(skd.getCollationNameExpression(), feg, selectedItemType, isInLoop);
                                    fixupGroupReferences(skd.getOrder(), feg, selectedItemType, isInLoop);
                                }
                            }
                        }
                    } else {
                        foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                            fixupGroupReferences(o.getChildExpression(), feg, selectedItemType, (isInLoop || o.isHigherOrder()));
                        }
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            selectOp.optimize(visitor, contextItemType);
            Saxon.Hej.type.ItemType selectedItemType = getSelectExpression().getItemType();
            Saxon.Hej.expr.parser.ContextItemStaticInfo sit = visitor.getConfiguration().makeContextItemStaticInfo(selectedItemType, false);
            sit.setContextSettingExpression(getSelectExpression());
            actionOp.optimize(visitor, sit);
            keyOp.optimize(visitor, sit);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSelectExpression())) {
                return getSelectExpression();
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getActionExpression())) {
                return getActionExpression();
            }
            if ((getSortKeyDefinitions() != null)) {
                foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in getSortKeyDefinitions()) {
                    Saxon.Hej.expr.Expression sortKey = skd.getSortKey();
                    sortKey = sortKey.optimize(visitor, sit);
                    skd.setSortKey(sortKey, true);
                }
            }
            if ((collationOp != null)) {
                collationOp.optimize(visitor, contextItemType);
            }
            if (((collator == null) && ((getCollationNameExpression() is Saxon.Hej.expr.StringLiteral)))) {
                string collation = ((Saxon.Hej.expr.StringLiteral)(getCollationNameExpression())).stringify();
                System.Uri collationURI;
                try {
                    collationURI = new System.Uri(collation, System.UriKind.RelativeOrAbsolute);
                    if (!(collationURI.IsAbsoluteUri)) {
                        collationURI = new System.Uri(getStaticBaseURI(), collationURI);
                        string collationNameString = collationURI.ToString();
                        setCollationNameExpression(new Saxon.Hej.expr.StringLiteral(collationNameString));
                        collator = visitor.getConfiguration().getCollation(collationNameString);
                        if ((collator == null)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unknown collation "+Saxon.Hej.trans.Err.wrap(collationURI.ToString(), Saxon.Hej.trans.Err.URI)));
                            err.setErrorCode("XTDE1110");
                            err.setLocation(getLocation());
                            throw err;
                        }
                    }
                } catch (System.UriFormatException err) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Collation name '"+getCollationNameExpression())+"' is not a valid URI"));
                    e.setErrorCode("XTDE1110");
                    e.setLocation(getLocation());
                    throw e;
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.sort.SortKeyDefinition[] newKeyDef = null;
            if ((getSortKeyDefinitions() != null)) {
                newKeyDef = new Saxon.Hej.expr.sort.SortKeyDefinition[getSortKeyDefinitions().size()];
                for (int i = 0; (i<getSortKeyDefinitions().size()); i++) {
                    newKeyDef[i] = getSortKeyDefinitions().getSortKeyDefinition(i).copy(rebindings);
                }
            }
            Saxon.Hej.expr.instruct.ForEachGroup feg = new Saxon.Hej.expr.instruct.ForEachGroup(getSelectExpression().copy(rebindings), getActionExpression().copy(rebindings), algorithm, getGroupingKey().copy(rebindings), collator, getCollationNameExpression().copy(rebindings), ((newKeyDef == null) ? null : new Saxon.Hej.expr.sort.SortKeyDefinitionList(newKeyDef)));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, feg);
            feg.setComposite(isComposite());
            fixupGroupReferences(feg, feg, getSelectExpression().getItemType(), false);
            return feg;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getActionExpression().getItemType();
        }
        public override int computeDependencies() {
            int dependencies = 0;
            dependencies |= getSelectExpression().getDependencies();
            dependencies |= (getGroupingKey().getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS));
            dependencies |= (getActionExpression().getDependencies()&~(((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_GROUP))));
            if ((getSortKeyDefinitions() != null)) {
                foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in getSortKeyDefinitions()) {
                    dependencies |= (skd.getSortKey().getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS));
                    Saxon.Hej.expr.Expression e = skd.getCaseOrder();
                    if (((e != null) && !(((e is Saxon.Hej.expr.Literal))))) {
                        dependencies |= e.getDependencies();
                    }
                    e = skd.getDataTypeExpression();
                    if (((e != null) && !(((e is Saxon.Hej.expr.Literal))))) {
                        dependencies |= e.getDependencies();
                    }
                    e = skd.getLanguage();
                    if (((e != null) && !(((e is Saxon.Hej.expr.Literal))))) {
                        dependencies |= e.getDependencies();
                    }
                }
            }
            if ((getCollationNameExpression() != null)) {
                dependencies |= getCollationNameExpression().getDependencies();
            }
            return dependencies;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            p |= (getActionExpression().getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED);
            return p;
        }
        public override bool mayCreateNewNodes() {
            int props = getActionExpression().getSpecialProperties();
            return (((props&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0);
        }
        public override string getStreamerName() {
            return "ForEachGroup";
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = getSelectExpression().addToPathMap(pathMap, pathMapNodeSet);
            if ((getCollationNameExpression() != null)) {
                getCollationNameExpression().addToPathMap(pathMap, pathMapNodeSet);
            }
            if ((getSortKeyDefinitions() != null)) {
                foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in getSortKeyDefinitions()) {
                    skd.getSortKey().addToPathMap(pathMap, target);
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
            }
            return getActionExpression().addToPathMap(pathMap, target);
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getActionExpression().checkPermittedContents(parentType, false);
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.@event.PipelineConfiguration pipe = output.getPipelineConfiguration();
            Saxon.Hej.expr.sort.GroupIterator groupIterator = getGroupIterator(context);
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setOrigin(this);
            Saxon.Hej.om.FocusIterator focusIterator = c2.trackFocus(groupIterator);
            c2.setCurrentGroupIterator(groupIterator);
            c2.setCurrentTemplateRule(null);
            pipe.setXPathContext(c2);
            if (controller.isTracing()) {
                Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                System.Diagnostics.Debug.Assert((listener != null));
                Saxon.Hej.om.Item item;
                while (((item = focusIterator.next()) != null)) {
                    listener.startCurrentItem(item);
                    getActionExpression().process(output, c2);
                    listener.endCurrentItem(item);
                }
            } else {
                while ((focusIterator.next() != null)) {
                    getActionExpression().process(output, c2);
                }
            }
            pipe.setXPathContext(context);
            return null;
        }
        public Saxon.Hej.expr.Expression getCollationNameExpression() {
            return ((collationOp == null) ? null : collationOp.getChildExpression());
        }
        private Saxon.Hej.lib.StringCollator getCollator(Saxon.Hej.expr.XPathContext context) {
            if ((getCollationNameExpression() != null)) {
                Saxon.Hej.value.StringValue collationValue = (Saxon.Hej.value.StringValue)(getCollationNameExpression().evaluateItem(context));
                System.Diagnostics.Debug.Assert((collationValue != null));
                string cname = collationValue.getStringValue();
                try {
                    return context.getConfiguration().getCollation(cname, getStaticBaseURIString(), "FOCH0002");
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.setLocation(getLocation());
                    throw;
                }
            } else {
                return Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
        }
        private Saxon.Hej.expr.sort.GroupIterator getGroupIterator(Saxon.Hej.expr.XPathContext context) {
            return getGroupIterator(getSelectExpression(), context);
        }
        public Saxon.Hej.expr.sort.GroupIterator getGroupIterator(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.GroupIterator groupIterator;
            switch (algorithm) {
              case GROUP_BY:
                {
                    Saxon.Hej.lib.StringCollator coll = collator;
                    if ((coll == null)) {
                        coll = getCollator(context);
                    }
                    Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
                    Saxon.Hej.om.FocusIterator population = c2.trackFocus(select.iterate(context));
                    groupIterator = new Saxon.Hej.expr.sort.GroupByIterator(population, getGroupingKey(), c2, coll, composite);
                    break ;
                }
              case GROUP_ADJACENT:
                {
                    Saxon.Hej.lib.StringCollator coll = collator;
                    if ((coll == null)) {
                        coll = getCollator(context);
                    }
                    groupIterator = new Saxon.Hej.expr.sort.GroupAdjacentIterator(select, getGroupingKey(), context, coll, composite);
                    break ;
                }
              case GROUP_STARTING:
                groupIterator = new Saxon.Hej.expr.sort.GroupStartingIterator(select, (Saxon.Hej.pattern.Pattern)(getGroupingKey()), context);
                break ;
              case GROUP_ENDING:
                groupIterator = new Saxon.Hej.expr.sort.GroupEndingIterator(select, (Saxon.Hej.pattern.Pattern)(getGroupingKey()), context);
                break ;
              case GROUP_BREAKING_WHEN:
                Saxon.Hej.om.Function breakWhen = (Saxon.Hej.om.Function)(getGroupingKey().evaluateItem(context));
                groupIterator = new Saxon.Hej.expr.sort.GroupBreakingIterator(select, breakWhen, context);
                break ;
              default:
                throw new Saxon.Ejava.lang.AssertionError("Unknown grouping algorithm");
            }
            if ((getSortKeyDefinitions() != null)) {
                Saxon.Hej.expr.sort.AtomicComparer[] comps = sortComparators;
                Saxon.Hej.expr.XPathContext xpc = context.newMinorContext();
                if ((comps == null)) {
                    comps = new Saxon.Hej.expr.sort.AtomicComparer[getSortKeyDefinitions().size()];
                    for (int s = 0; (s<getSortKeyDefinitions().size()); s++) {
                        comps[s] = getSortKeyDefinitions().getSortKeyDefinition(s).makeComparator(xpc);
                    }
                }
                groupIterator = new Saxon.Hej.expr.sort.SortedGroupIterator(xpc, groupIterator, this, comps);
            }
            return groupIterator;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.GroupIterator master = getGroupIterator(context);
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setOrigin(this);
            c2.trackFocus(master);
            c2.setCurrentGroupIterator(master);
            c2.setCurrentTemplateRule(null);
            return new Saxon.Hej.expr.ContextMappingIterator(cc => getActionExpression().iterate(cc), c2);
        }
        public Saxon.Hej.value.AtomicValue evaluateSortKey(int n, Saxon.Hej.expr.XPathContext c) {
            return (Saxon.Hej.value.AtomicValue)(getSortKeyDefinitions().getSortKeyDefinition(n).getSortKey().evaluateItem(c));
        }
        public Saxon.Hej.expr.sort.SortKeyDefinitionList getSortKeyDefinitionList() {
            if ((sortKeysOp == null)) {
                return null;
            }
            return (Saxon.Hej.expr.sort.SortKeyDefinitionList)(sortKeysOp.getChildExpression());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("forEachGroup", this);
            @out.emitAttribute("algorithm", getAlgorithmName(algorithm));
            string flags = "";
            if (composite) {
                flags = "c";
            }
            if (isInFork()) {
                flags += "k";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            @out.setChildRole("select");
            getSelectExpression().export(@out);
            if (((algorithm == GROUP_BY) || (algorithm == GROUP_ADJACENT))) {
                @out.setChildRole("key");
                getGroupingKey().export(@out);
            } else {
                @out.setChildRole("match");
                getGroupingKey().export(@out);
            }
            if ((getSortKeyDefinitions() != null)) {
                @out.setChildRole("sort");
                getSortKeyDefinitionList().export(@out);
            }
            if ((getCollationNameExpression() != null)) {
                @out.setChildRole("collation");
                getCollationNameExpression().export(@out);
            }
            @out.setChildRole("content");
            getActionExpression().export(@out);
            @out.endElement();
        }
        private static string getAlgorithmName(byte algorithm) {
            switch (algorithm) {
              case GROUP_BY:
                return "by";
              case GROUP_ADJACENT:
                return "adjacent";
              case GROUP_STARTING:
                return "starting";
              case GROUP_ENDING:
                return "ending";
              default:
                return "** unknown algorithm **";
            }
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public void setAction(Saxon.Hej.expr.Expression action) {
            actionOp.setChildExpression(action);
        }
        public void setKey(Saxon.Hej.expr.Expression key) {
            keyOp.setChildExpression(key);
        }
        public void setCollationNameExpression(Saxon.Hej.expr.Expression collationNameExpression) {
            if ((collationOp == null)) {
                collationOp = new Saxon.Hej.expr.Operand(this, collationNameExpression, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                collationOp.setChildExpression(collationNameExpression);
            }
        }
        public void setSortKeyDefinitions(Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeyDefinitions) {
            if ((sortKeysOp == null)) {
                sortKeysOp = new Saxon.Hej.expr.Operand(this, sortKeyDefinitions, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            } else {
                sortKeysOp.setChildExpression(sortKeyDefinitions);
            }
        }
    
    }
}