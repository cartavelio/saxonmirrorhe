using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ForEach : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.ContextSwitchingExpression {
        protected internal bool containsTailCall;
        protected internal Saxon.Hej.expr.Operand selectOp;
        protected internal Saxon.Hej.expr.Operand actionOp;
        protected internal Saxon.Hej.expr.Operand separatorOp;
        protected internal Saxon.Hej.expr.Operand threadsOp;
        protected internal bool _isInstruction;
        public ForEach(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression action) : this (select, action, false, null) {
        }
        public ForEach(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression action, bool containsTailCall, Saxon.Hej.expr.Expression threads)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT);
            actionOp = new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
            if ((threads != null)) {
                threadsOp = new Saxon.Hej.expr.Operand(this, threads, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            this.containsTailCall = (containsTailCall && (action is Saxon.Hej.expr.instruct.TailCallReturner));
        }
        public void setSeparatorExpression(Saxon.Hej.expr.Expression separator) {
            separatorOp = new Saxon.Hej.expr.Operand(this, separator, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.Expression getSeparatorExpression() {
            return ((separatorOp == null) ? null : separatorOp.getChildExpression());
        }
        public void setInstruction(bool inst) {
            _isInstruction = inst;
        }
        public override bool isInstruction() {
            return _isInstruction;
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.Expression getAction() {
            return actionOp.getChildExpression();
        }
        public void setAction(Saxon.Hej.expr.Expression action) {
            actionOp.setChildExpression(action);
        }
        public Saxon.Hej.expr.Expression getThreads() {
            return ((threadsOp == null) ? null : threadsOp.getChildExpression());
        }
        public void setThreads(Saxon.Hej.expr.Expression threads) {
            if ((threads != null)) {
                if ((threadsOp == null)) {
                    threadsOp = new Saxon.Hej.expr.Operand(this, threads, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                } else {
                    threadsOp.setChildExpression(threads);
                }
            }
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(selectOp, actionOp, separatorOp, threadsOp);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_FOR_EACH;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return getSelect();
        }
        public void setSelectExpression(Saxon.Hej.expr.Expression select) {
            this.setSelect(select);
        }
        public void setActionExpression(Saxon.Hej.expr.Expression action) {
            this.setAction(action);
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return getAction();
        }
        public Saxon.Hej.expr.Expression getNumberOfThreadsExpression() {
            return getThreads();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getAction().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if (isInstruction()) {
                return base.getStaticUType(contextItemType);
            } else {
                return getAction().getStaticUType(getSelect().getStaticUType(contextItemType));
            }
        }
        public override bool mayCreateNewNodes() {
            int props = getAction().getSpecialProperties();
            return (((props&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            selectOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.type.ItemType selectType = getSelect().getItemType();
            if ((selectType == Saxon.Hej.type.ErrorType.getInstance())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(getSelect().getItemType(), false);
            cit.setContextSettingExpression(getSelect());
            actionOp.typeCheck(visitor, cit);
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getSelect().getCardinality()))) {
                actionOp.setOperandRole(actionOp.getOperandRole().modifyProperty(Saxon.Hej.expr.OperandRole.SINGLETON, true));
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            selectOp.optimize(visitor, contextInfo);
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(getSelect().getItemType(), false);
            cit.setContextSettingExpression(getSelect());
            actionOp.optimize(visitor, cit);
            if (!(visitor.isOptimizeForStreaming())) {
                if (Saxon.Hej.expr.Literal.isEmptySequence(getSelect())) {
                    return getSelect();
                }
                if (Saxon.Hej.expr.Literal.isEmptySequence(getAction())) {
                    return getAction();
                }
            }
            if (((getSelect().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && (getAction() is Saxon.Hej.expr.AxisExpression))) {
                return new Saxon.Hej.expr.SimpleStepExpression(getSelect(), getAction());
            }
            if (((threadsOp != null) && !(Saxon.Hej.expr.Literal.isEmptySequence(getThreads())))) {
                return visitor.obtainOptimizer().generateMultithreadedInstruction(this);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            setSelect(getSelect().unordered(retainAllNodes, forStreaming));
            setAction(getAction().unordered(retainAllNodes, forStreaming));
            return this;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = getSelect().addToPathMap(pathMap, pathMapNodeSet);
            return getAction().addToPathMap(pathMap, target);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ForEach f2 = new Saxon.Hej.expr.instruct.ForEach(getSelect().copy(rebindings), getAction().copy(rebindings), containsTailCall, getThreads());
            if ((separatorOp != null)) {
                f2.setSeparatorExpression(getSeparatorExpression().copy(rebindings));
            }
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f2);
            f2.setInstruction(isInstruction());
            return f2;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            if ((getSelect().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                p |= getAction().getSpecialProperties();
            } else {
                p |= (getAction().getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED);
            }
            return p;
        }
        public override bool alwaysCreatesNewNodes() {
            return (((getAction() is Saxon.Hej.expr.instruct.Instruction)) && ((Saxon.Hej.expr.instruct.Instruction)(getAction())).alwaysCreatesNewNodes());
        }
        public override bool isUpdatingExpression() {
            return getAction().isUpdatingExpression();
        }
        public override void checkForUpdatingSubexpressions() {
            if (getSelect().isUpdatingExpression()) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Updating expression appears in a context where it is not permitted", "XUST0001");
                err.setLocation(getSelect().getLocation());
                throw err;
            }
        }
        public override int getImplementationMethod() {
            return (((ITERATE_METHOD|PROCESS_METHOD)|Saxon.Hej.expr.Expression.WATCH_METHOD)|Saxon.Hej.expr.Expression.ITEM_FEED_METHOD);
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getAction().checkPermittedContents(parentType, false);
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setOrigin(this);
            Saxon.Hej.om.FocusIterator iter = c2.trackFocus(getSelect().iterate(context));
            c2.setCurrentTemplateRule(null);
            Saxon.Hej.expr.Expression action = getAction();
            if (containsTailCall) {
                if (controller.isTracing()) {
                    Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                    System.Diagnostics.Debug.Assert((listener != null));
                    Saxon.Hej.om.Item item = iter.next();
                    if ((item == null)) {
                        return null;
                    }
                    listener.startCurrentItem(item);
                    Saxon.Hej.expr.instruct.TailCall tc = ((Saxon.Hej.expr.instruct.TailCallReturner)(action)).processLeavingTail(output, c2);
                    listener.endCurrentItem(item);
                    return tc;
                } else {
                    Saxon.Hej.om.Item item = iter.next();
                    if ((item == null)) {
                        return null;
                    }
                    return ((Saxon.Hej.expr.instruct.TailCallReturner)(action)).processLeavingTail(output, c2);
                }
            } else {
                Saxon.Hej.@event.PipelineConfiguration pipe = output.getPipelineConfiguration();
                pipe.setXPathContext(c2);
                Saxon.Hej.om.NodeInfo separator = null;
                if ((separatorOp != null)) {
                    separator = makeSeparator(context);
                }
                if ((controller.isTracing() || (separator != null))) {
                    Saxon.Hej.lib.TraceListener listener = controller.getTraceListener();
                    Saxon.Hej.om.Item item;
                    bool first = true;
                    while (((item = iter.next()) != null)) {
                        if (controller.isTracing()) {
                            System.Diagnostics.Debug.Assert((listener != null));
                            listener.startCurrentItem(item);
                        }
                        if ((separator != null)) {
                            if (first) {
                                first = false;
                            } else {
                                output.append(separator);
                            }
                        }
                        action.process(output, c2);
                        if (controller.isTracing()) {
                            listener.endCurrentItem(item);
                        }
                    }
                } else {
                    try {
                        Saxon.Hej.om.SequenceTool.supply(iter, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => action.process(output, c2))));
                    } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                        throw err.getXPathException();
                    }
                }
                pipe.setXPathContext(context);
            }
            return null;
        }
        protected internal Saxon.Hej.om.NodeInfo makeSeparator(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo separator;
            Saxon.Hej.str.UnicodeString sepValue = separatorOp.getChildExpression().evaluateAsString(context);
            Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            orphan.setNodeKind(Saxon.Hej.type.Type.TEXT);
            orphan.setStringValue(sepValue);
            separator = orphan;
            return separator;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContextMinor c2 = context.newMinorContext();
            c2.trackFocus(getSelect().iterate(context));
            if ((separatorOp == null)) {
                return new Saxon.Hej.expr.ContextMappingIterator(c3 => getAction().iterate(c3), c2);
            } else {
                Saxon.Hej.om.NodeInfo separator = makeSeparator(context);
                Saxon.Hej.expr.ContextMappingFunction mapper = cxt => {
                    if ((cxt.getCurrentIterator().position() == 1)) {
                        return getAction().iterate(cxt);
                    } else {
                        return new Saxon.Hej.tree.iter.PrependSequenceIterator(separator, getAction().iterate(cxt));
                    }
                } ;
                return new Saxon.Hej.expr.ContextMappingIterator(mapper, c2);
            }
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            Saxon.Hej.expr.XPathContextMinor c2 = context.newMinorContext();
            c2.trackFocus(getSelect().iterate(context));
            Saxon.Hej.om.SequenceIterator iter = c2.getCurrentIterator();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                getAction().evaluatePendingUpdates(c2, pul);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("forEach", this);
            getSelect().export(@out);
            getAction().export(@out);
            if ((separatorOp != null)) {
                @out.setChildRole("separator");
                separatorOp.getChildExpression().export(@out);
            }
            explainThreads(@out);
            @out.endElement();
        }
        protected internal virtual void explainThreads(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        public override string ToString() {
            return ((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getSelect())+" ! ")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getAction()));
        }
        public override string toShortString() {
            return ((getSelect().toShortString()+"!")+getAction().toShortString());
        }
        public override string getExpressionName() {
            return "forEach";
        }
        public override string getStreamerName() {
            return "ForEach";
        }
    
    }
}