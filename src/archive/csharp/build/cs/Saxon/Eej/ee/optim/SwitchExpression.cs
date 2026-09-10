using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class SwitchExpression : Saxon.Hej.expr.Expression, Saxon.Hej.expr.instruct.TailCallReturner, Saxon.Hej.expr.instruct.ConditionalInstruction {
        private readonly Saxon.Hej.expr.Operand subjectOp;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.Operand> actions;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,int> map;
        private System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.value.AtomicValue> originalValues;
        private readonly Saxon.Hej.expr.Operand defaultActionOp;
        private readonly Saxon.Hej.lib.StringCollator collation;
        private bool _isInstruction;
        public readonly static Saxon.Hej.expr.OperandRole SUBJECT = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
        public SwitchExpression(Saxon.Eej.ee.optim.SwitchExpression.SwitchCaseInfo switchCaseInfo)  {
            this.originalValues = switchCaseInfo.originalValues;
            this.subjectOp = new Saxon.Hej.expr.Operand(this, switchCaseInfo.commonlhs, SUBJECT);
            this.map = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, int>(switchCaseInfo.index.Count);
            this.actions = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(switchCaseInfo.index.Count);
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.expr.Expression> entry in switchCaseInfo.index) {
                Saxon.Hej.expr.Expression action = entry.Value;
                int actionNumber = -1;
                for (int i = 0; (i<actions.Count); i++) {
                    if ((actions[i].getChildExpression() == action)) {
                        actionNumber = i;
                    }
                }
                if ((actionNumber == -1)) {
                    actionNumber = actions.Count;
                    actions.Add(new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.instruct.Choose.CHOICE_ACTION));
                }
                Saxon.Impl.Helpers.MapUtils.Put(this.map, entry.Key, actionNumber);
            }
            this.defaultActionOp = new Saxon.Hej.expr.Operand(this, switchCaseInfo.defaultAction, Saxon.Hej.expr.instruct.Choose.CHOICE_ACTION);
            this.collation = switchCaseInfo.collation;
        }
        private SwitchExpression(Saxon.Hej.expr.Expression commonlhs, System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,int> cases, System.Collections.Generic.IList<Saxon.Hej.expr.Operand> actions, Saxon.Hej.expr.Expression otherwise, Saxon.Hej.lib.StringCollator collation)  {
            this.subjectOp = new Saxon.Hej.expr.Operand(this, commonlhs, SUBJECT);
            this.map = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, int>(cases);
            this.actions = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(actions.Count);
            foreach (Saxon.Hej.expr.Operand o in actions) {
                this.actions.Add(new Saxon.Hej.expr.Operand(this, o.getChildExpression().copy(new Saxon.Hej.expr.parser.RebindingMap()), Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION));
            }
            this.defaultActionOp = new Saxon.Hej.expr.Operand(this, otherwise, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            this.collation = collation;
        }
        public void setInstruction(bool inst) {
            _isInstruction = inst;
        }
        public override bool isInstruction() {
            return _isInstruction;
        }
        public Saxon.Hej.expr.Expression getSubjectExpression() {
            return subjectOp.getChildExpression();
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,int> getCaseMap() {
            return map;
        }
        public Saxon.Hej.expr.Expression getOtherwiseExpression() {
            return defaultActionOp.getChildExpression();
        }
        public Saxon.Hej.expr.Operand getAction(int index) {
            return actions[index];
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.Operand> getActions() {
            return actions;
        }
        public Saxon.Hej.expr.Operand getOtherwiseOperand() {
            return defaultActionOp;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            list.Add(subjectOp);
            list.Add(defaultActionOp);
            Saxon.Impl.Helpers.ListUtils.AddAll(list, actions);
            return list;
        }
        public Saxon.Hej.lib.StringCollator getCollation() {
            return collation;
        }
        protected internal override int computeCardinality() {
            int card = getOtherwiseExpression().getCardinality();
            foreach (Saxon.Hej.expr.Operand action in actions) {
                card = Saxon.Hej.value.Cardinality.union(card, action.getChildExpression().getCardinality());
                if ((card == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                    return card;
                }
            }
            return card;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> newActions = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            foreach (Saxon.Hej.expr.Operand o in actions) {
                newActions.Add(new Saxon.Hej.expr.Operand(this, o.getChildExpression().copy(rebindings), Saxon.Hej.expr.instruct.Choose.CHOICE_ACTION));
            }
            Saxon.Eej.ee.optim.SwitchExpression s2 = new Saxon.Eej.ee.optim.SwitchExpression(getSubjectExpression().copy(rebindings), map, newActions, getOtherwiseExpression().copy(rebindings), collation);
            s2.originalValues = originalValues;
            return s2;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("switch", this);
            if ((collation != Saxon.Hej.expr.sort.CodepointCollator.getInstance())) {
                @out.emitAttribute("coll", collation.getCollationURI());
            }
            @out.setChildRole("value");
            getSubjectExpression().export(@out);
            foreach (Saxon.Hej.expr.sort.AtomicMatchKey key in Saxon.Impl.Helpers.MapUtils.KeySet(map)) {
                @out.startElement("case", this);
                Saxon.Hej.expr.Literal.exportAtomicValue(Saxon.Impl.Helpers.MapUtils.Get(originalValues, key), @out);
                int selection = Saxon.Impl.Helpers.MapUtils.Get(map, key);
                actions[selection].getChildExpression().export(@out);
                @out.endElement();
            }
            @out.setChildRole("default");
            getOtherwiseExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType type = getOtherwiseExpression().getItemType();
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            foreach (Saxon.Hej.expr.Operand operand in actions) {
                Saxon.Hej.expr.Expression action = operand.getChildExpression();
                type = Saxon.Hej.type.Type.getCommonSuperType(type, action.getItemType(), th);
            }
            return type;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            foreach (Saxon.Hej.expr.Operand o in actions) {
                Saxon.Hej.expr.Expression e2 = o.getChildExpression().unordered(retainAllNodes, forStreaming);
                if ((e2 != o.getChildExpression())) {
                    o.setChildExpression(e2);
                }
            }
            defaultActionOp.setChildExpression(getOtherwiseExpression().unordered(retainAllNodes, forStreaming));
            return this;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue subjectValue = (Saxon.Hej.value.AtomicValue)(getSubjectExpression().evaluateItem(context));
            if ((subjectValue == null)) {
                return getOtherwiseExpression().evaluateItem(context);
            }
            Saxon.Hej.expr.sort.AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
            int selection = getIndex(key);
            if ((selection == -1)) {
                return getOtherwiseExpression().evaluateItem(context);
            } else {
                return actions[selection].getChildExpression().evaluateItem(context);
            }
        }
        private int getIndex(Saxon.Hej.expr.sort.AtomicMatchKey key) {
            return Saxon.Impl.Helpers.MapUtils.GetOrDefault(map, key, -1);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue subjectValue = (Saxon.Hej.value.AtomicValue)(getSubjectExpression().evaluateItem(context));
            if ((subjectValue == null)) {
                return getOtherwiseExpression().iterate(context);
            }
            Saxon.Hej.expr.sort.AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
            int selection = getIndex(key);
            if ((selection == -1)) {
                return getOtherwiseExpression().iterate(context);
            } else {
                return actions[selection].getChildExpression().iterate(context);
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue subjectValue = (Saxon.Hej.value.AtomicValue)(getSubjectExpression().evaluateItem(context));
            if ((subjectValue == null)) {
                getOtherwiseExpression().process(output, context);
                return ;
            }
            Saxon.Hej.expr.sort.AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
            int selection = getIndex(key);
            if ((selection == -1)) {
                getOtherwiseExpression().process(output, context);
            } else {
                actions[selection].getChildExpression().process(output, context);
            }
        }
        public Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue subjectValue = (Saxon.Hej.value.AtomicValue)(getSubjectExpression().evaluateItem(context));
            if ((subjectValue == null)) {
                return doTailCall(getOtherwiseExpression(), output, context);
            }
            Saxon.Hej.expr.sort.AtomicMatchKey key = subjectValue.getXPathMatchKey(collation, context.getImplicitTimezone());
            int selection = Saxon.Impl.Helpers.MapUtils.GetOrDefault(map, key, -1);
            if ((selection == -1)) {
                return doTailCall(getOtherwiseExpression(), output, context);
            } else {
                return doTailCall(actions[selection].getChildExpression(), output, context);
            }
        }
        private Saxon.Hej.expr.instruct.TailCall doTailCall(Saxon.Hej.expr.Expression o, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            if ((o is Saxon.Hej.expr.instruct.TailCallReturner)) {
                return ((Saxon.Hej.expr.instruct.TailCallReturner)(o)).processLeavingTail(output, context);
            } else {
                o.process(output, context);
                return null;
            }
        }
        public override int getImplementationMethod() {
            return ((PROCESS_METHOD|ITERATE_METHOD)|EVALUATE_METHOD);
        }
        public override string getExpressionName() {
            return "switch";
        }
        public class SwitchCaseInfo {
            public System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.expr.Expression> index = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.expr.Expression>();
            public System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.value.AtomicValue> originalValues = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.value.AtomicValue>();
            public Saxon.Hej.expr.Expression commonlhs = null;
            public Saxon.Hej.type.BuiltInAtomicType commontype = null;
            public Saxon.Hej.lib.StringCollator collation = null;
            public System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> values = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
            public System.Collections.Generic.IList<Saxon.Hej.expr.Expression> actions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            public Saxon.Hej.expr.Expression defaultAction = null;
            public bool makeIndex() {
                for (int i = 0; (i<values.Count); i++) {
                    Saxon.Hej.value.AtomicValue value = values[i];
                    Saxon.Hej.expr.Expression action = actions[i];
                    try {
                        Saxon.Hej.expr.sort.AtomicMatchKey key = value.getXPathMatchKey(collation, Saxon.Hej.value.CalendarValue.MISSING_TIMEZONE);
                        if ((key == null)) {
                            return false;
                        } else {
                            if ((Saxon.Impl.Helpers.MapUtils.Get(index, key) == null)) {
                                Saxon.Impl.Helpers.MapUtils.Put(index, key, action);
                                Saxon.Impl.Helpers.MapUtils.Put(originalValues, key, value);
                            }
                        }
                    } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                        return false;
                    }
                }
                return true;
            }
        
        }
        public override string getStreamerName() {
            return "SwitchExpression";
        }
    
    }
}