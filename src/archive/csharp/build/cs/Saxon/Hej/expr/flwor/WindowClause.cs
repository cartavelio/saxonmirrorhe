using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WindowClause : Saxon.Hej.expr.flwor.Clause {
        private bool sliding;
        private bool includeUnclosedWindows = true;
        private Saxon.Hej.expr.Operand sequenceOp;
        private Saxon.Hej.expr.Operand startConditionOp;
        private Saxon.Hej.expr.Operand endConditionOp;
        private Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.flwor.LocalVariableBinding> windowVars = new Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.flwor.LocalVariableBinding>(10);
        private Saxon.Hej.expr.ItemTypeCheckingFunction itemTypeChecker;
        private bool windowMustBeSingleton;
        public const int WINDOW_VAR = 0;
        public const int START_ITEM = 1;
        public const int START_ITEM_POSITION = 2;
        public const int START_PREVIOUS_ITEM = 3;
        public const int START_NEXT_ITEM = 4;
        public const int END_ITEM = 5;
        public const int END_ITEM_POSITION = 6;
        public const int END_PREVIOUS_ITEM = 7;
        public const int END_NEXT_ITEM = 8;
        public WindowClause()  {
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.WINDOW;
        }
        public void setIsSlidingWindow(bool sliding) {
            this.sliding = sliding;
        }
        public bool isSlidingWindow() {
            return sliding;
        }
        public bool isTumblingWindow() {
            return !(sliding);
        }
        public void setIncludeUnclosedWindows(bool include) {
            this.includeUnclosedWindows = include;
        }
        public bool isIncludeUnclosedWindows() {
            return includeUnclosedWindows;
        }
        public void initSequence(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression sequence) {
            sequenceOp = new Saxon.Hej.expr.Operand(flwor, sequence, Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public void setSequence(Saxon.Hej.expr.Expression sequence) {
            sequenceOp.setChildExpression(sequence);
        }
        public Saxon.Hej.expr.Expression getSequence() {
            return sequenceOp.getChildExpression();
        }
        public void initStartCondition(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression startCondition) {
            startConditionOp = new Saxon.Hej.expr.Operand(flwor, startCondition, Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public void setStartCondition(Saxon.Hej.expr.Expression startCondition) {
            startConditionOp.setChildExpression(startCondition);
        }
        public Saxon.Hej.expr.Expression getStartCondition() {
            return startConditionOp.getChildExpression();
        }
        public void initEndCondition(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression endCondition) {
            endConditionOp = new Saxon.Hej.expr.Operand(flwor, endCondition, Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public void setEndCondition(Saxon.Hej.expr.Expression endCondition) {
            endConditionOp.setChildExpression(endCondition);
        }
        public Saxon.Hej.expr.Expression getEndCondition() {
            return ((endConditionOp == null) ? null : endConditionOp.getChildExpression());
        }
        public void setVariableBinding(int role, Saxon.Hej.expr.flwor.LocalVariableBinding binding) {
            foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in windowVars.valueSet()) {
                if (b.getVariableQName().Equals(binding.getVariableQName())) {
                    throw new Saxon.Hej.trans.XPathException((("Two variables in a window clause cannot have the same name ("+binding.getVariableQName().getDisplayName())+")"), "XQST0103");
                }
            }
            windowVars.put(role, binding);
        }
        public Saxon.Hej.expr.flwor.LocalVariableBinding getVariableBinding(int role) {
            return windowVars.get(role);
        }
        public Saxon.Hej.expr.ItemTypeCheckingFunction getItemTypeChecker() {
            return itemTypeChecker;
        }
        public bool isWindowMustBeSingleton() {
            return windowMustBeSingleton;
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.value.SequenceType requiredType = getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.WINDOW_VAR).getRequiredType();
            Saxon.Hej.type.ItemType required = requiredType.getPrimaryType();
            Saxon.Hej.type.ItemType supplied = getSequence().getItemType();
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.type.Affinity rel = th.relationship(required, supplied);
            switch (rel) {
              case Saxon.Hej.type.Affinity.SAME_TYPE:
              case Saxon.Hej.type.Affinity.SUBSUMES:
                break ;
              case Saxon.Hej.type.Affinity.OVERLAPS:
              case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.WINDOW_VAR).getVariableQName().getDisplayName(), 0);
                itemTypeChecker = new Saxon.Hej.expr.ItemTypeCheckingFunction(required, role, getLocation(), config);
                break ;
              case Saxon.Hej.type.Affinity.DISJOINT:
                string message = ((("The items in the window will always be instances of "+supplied)+", never of the required type ")+required);
                throw new Saxon.Hej.trans.XPathException(message, "XPTY0004", getLocation());
            }
            windowMustBeSingleton = !(Saxon.Hej.value.Cardinality.allowsMany(requiredType.getCardinality()));
            if ((requiredType.getCardinality() == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                string message = "The value of the window variable can never be an empty sequence";
                throw new Saxon.Hej.trans.XPathException(message, "XPTY0004", getLocation());
            }
        }
        protected internal void checkWindowContents(Saxon.Hej.expr.flwor.WindowClause.Window w) {
            if ((windowMustBeSingleton && (w.contents.Count>1))) {
                throw new Saxon.Hej.trans.XPathException(("Required type of window allows only a single item; window has length "+w.contents.Count), "XPTY0004", getLocation());
            }
            Saxon.Hej.expr.ItemTypeCheckingFunction checker = getItemTypeChecker();
            if ((checker != null)) {
                Saxon.Hej.om.SequenceIterator check = new Saxon.Hej.expr.ItemMappingIterator(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(w.contents), checker);
                Saxon.Hej.om.SequenceTool.supply(check, it => {
                } );
            }
        }
        public override Saxon.Hej.expr.flwor.Clause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.WindowClause wc = new Saxon.Hej.expr.flwor.WindowClause();
            wc.setLocation(getLocation());
            wc.setPackageData(getPackageData());
            wc.sliding = sliding;
            wc.includeUnclosedWindows = includeUnclosedWindows;
            wc.initSequence(flwor, getSequence().copy(rebindings));
            wc.initStartCondition(flwor, getStartCondition().copy(rebindings));
            if ((getEndCondition() != null)) {
                wc.initEndCondition(flwor, getEndCondition().copy(rebindings));
            }
            wc.windowVars = windowVars;
            return wc;
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.WindowClausePull(@base, this, context);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.WindowClausePush(output, destination, this);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(sequenceOp);
            /*Delegate.processOperand()*/processor(startConditionOp);
            if ((endConditionOp != null)) {
                /*Delegate.processOperand()*/processor(endConditionOp);
            }
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot use document projection with windowing");
        }
        public override Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            Saxon.Hej.expr.flwor.LocalVariableBinding[] vars = new Saxon.Hej.expr.flwor.LocalVariableBinding[windowVars.size()];
            int i = 0;
            foreach (Saxon.Hej.expr.flwor.LocalVariableBinding binding in windowVars.valueSet()) {
                vars[i++] = binding;
            }
            return vars;
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement((isSlidingWindow() ? "slidingWindow" : "tumblingWindow"));
            @out.startSubsidiaryElement("select");
            getSequence().export(@out);
            @out.endSubsidiaryElement();
            @out.startSubsidiaryElement("start");
            getStartCondition().export(@out);
            @out.endSubsidiaryElement();
            if ((endConditionOp != null)) {
                @out.startSubsidiaryElement("end");
                getEndCondition().export(@out);
                @out.endSubsidiaryElement();
            }
            @out.endElement();
        }
        protected internal bool matchesStart(Saxon.Hej.om.Item previous, Saxon.Hej.om.Item current, Saxon.Hej.om.Item next, int position, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.WindowClause clause = this;
            Saxon.Hej.expr.flwor.LocalVariableBinding binding;
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), current);
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM_POSITION);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(position));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_NEXT_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), makeValue(next));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_PREVIOUS_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), makeValue(previous));
            }
            return clause.getStartCondition().effectiveBooleanValue(context);
        }
        protected internal bool matchesEnd(Saxon.Hej.expr.flwor.WindowClause.Window window, Saxon.Hej.om.Item previous, Saxon.Hej.om.Item current, Saxon.Hej.om.Item next, int position, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.flwor.WindowClause clause = this;
            Saxon.Hej.expr.flwor.LocalVariableBinding binding;
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), window.startItem);
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM_POSITION);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(window.startPosition));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_NEXT_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), makeValue(window.startNextItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_PREVIOUS_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), makeValue(window.startPreviousItem));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), current);
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM_POSITION);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), new Saxon.Hej.value.Int64Value(position));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_NEXT_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), makeValue(next));
            }
            binding = clause.getVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_PREVIOUS_ITEM);
            if ((binding != null)) {
                context.setLocalVariable(binding.getLocalSlotNumber(), makeValue(previous));
            }
            return clause.getEndCondition().effectiveBooleanValue(context);
        }
        protected internal static Saxon.Hej.om.Sequence makeValue(Saxon.Hej.om.Item item) {
            if ((item == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return item;
            }
        }
        protected internal class Window {
            public Saxon.Hej.om.Item startItem;
            public int startPosition;
            public Saxon.Hej.om.Item startPreviousItem;
            public Saxon.Hej.om.Item startNextItem;
            public Saxon.Hej.om.Item endItem;
            public int endPosition = 0;
            public Saxon.Hej.om.Item endPreviousItem;
            public Saxon.Hej.om.Item endNextItem;
            public System.Collections.Generic.IList<Saxon.Hej.om.Item> contents;
            public bool despatched = false;
            public bool isFinished() {
                return (endPosition>0);
            }
            public bool isDespatched() {
                return despatched;
            }
        
        }
    
    }
}