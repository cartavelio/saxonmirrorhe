using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class IterateInstr : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.ContextSwitchingExpression {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand actionOp;
        private readonly Saxon.Hej.expr.Operand initiallyOp;
        private readonly Saxon.Hej.expr.Operand onCompletionOp;
        public IterateInstr(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.instruct.LocalParamBlock initiallyExp, Saxon.Hej.expr.Expression action, Saxon.Hej.expr.Expression onCompletion)  {
            if ((onCompletion == null)) {
                onCompletion = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT);
            actionOp = new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
            initiallyOp = new Saxon.Hej.expr.Operand(this, initiallyExp, new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.CONSTRAINED_CLASS, Saxon.Hej.expr.OperandUsage.NAVIGATION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE, expr => (expr is Saxon.Hej.expr.instruct.LocalParamBlock)));
            onCompletionOp = new Saxon.Hej.expr.Operand(this, onCompletion, new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS, Saxon.Hej.expr.OperandUsage.TRANSMISSION));
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.instruct.LocalParamBlock getInitiallyExp() {
            return (Saxon.Hej.expr.instruct.LocalParamBlock)(initiallyOp.getChildExpression());
        }
        public void setInitiallyExp(Saxon.Hej.expr.instruct.LocalParamBlock initiallyExp) {
            initiallyOp.setChildExpression(initiallyExp);
        }
        public void setAction(Saxon.Hej.expr.Expression action) {
            actionOp.setChildExpression(action);
        }
        public Saxon.Hej.expr.Expression getOnCompletion() {
            return onCompletionOp.getChildExpression();
        }
        public void setOnCompletion(Saxon.Hej.expr.Expression onCompletion) {
            onCompletionOp.setChildExpression(onCompletion);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(selectOp, actionOp, initiallyOp, onCompletionOp);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_ITERATE;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return actionOp.getChildExpression();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            selectOp.typeCheck(visitor, contextInfo);
            initiallyOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.type.ItemType selectType = getSelectExpression().getItemType();
            if ((selectType == Saxon.Hej.type.ErrorType.getInstance())) {
                selectType = Saxon.Hej.type.AnyItemType.getInstance();
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(selectType, false);
            cit.setContextSettingExpression(getSelectExpression());
            actionOp.typeCheck(visitor, cit);
            onCompletionOp.typeCheck(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getOnCompletion())) {
                if ((Saxon.Hej.expr.Literal.isEmptySequence(getSelectExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getActionExpression()))) {
                    return getOnCompletion();
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            selectOp.optimize(visitor, contextInfo);
            initiallyOp.optimize(visitor, contextInfo);
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit2 = visitor.getConfiguration().makeContextItemStaticInfo(getSelectExpression().getItemType(), false);
            cit2.setContextSettingExpression(getSelectExpression());
            actionOp.optimize(visitor, cit2);
            onCompletionOp.optimize(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getOnCompletion())) {
                if ((Saxon.Hej.expr.Literal.isEmptySequence(getSelectExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getActionExpression()))) {
                    return getOnCompletion();
                }
            }
            return this;
        }
        public bool isCompilable() {
            return !(containsBreakOrNextIterationWithinTryCatch(this, false));
        }
        private static bool containsBreakOrNextIterationWithinTryCatch(Saxon.Hej.expr.Expression exp, bool withinTryCatch) {
            if (((exp is Saxon.Hej.expr.instruct.BreakInstr) || (exp is Saxon.Hej.expr.instruct.NextIteration))) {
                return withinTryCatch;
            } else {
                bool found = false;
                bool inTryCatch = (withinTryCatch || (exp is Saxon.Hej.expr.TryCatch));
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    if (containsBreakOrNextIterationWithinTryCatch(o.getChildExpression(), inTryCatch)) {
                        found = true;
                        break ;
                    }
                }
                return found;
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if (Saxon.Hej.expr.Literal.isEmptySequence(getOnCompletion())) {
                return getActionExpression().getItemType();
            } else {
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                return Saxon.Hej.type.Type.getCommonSuperType(getActionExpression().getItemType(), getOnCompletion().getItemType(), th);
            }
        }
        public override bool mayCreateNewNodes() {
            return ((((getActionExpression().getSpecialProperties()&getOnCompletion().getSpecialProperties())&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0);
        }
        public override bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            Saxon.Hej.expr.instruct.LocalParamBlock paramBlock = getInitiallyExp();
            foreach (Saxon.Hej.expr.Operand o in paramBlock.operands()) {
                Saxon.Hej.expr.instruct.LocalParam setter = (Saxon.Hej.expr.instruct.LocalParam)(o.getChildExpression());
                if ((setter == binding)) {
                    return true;
                }
            }
            return false;
        }
        public override string getStreamerName() {
            return "Iterate";
        }
        public override int getImplementationMethod() {
            return PROCESS_METHOD;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getActionExpression().checkPermittedContents(parentType, false);
            getOnCompletion().checkPermittedContents(parentType, false);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.IterateInstr exp = new Saxon.Hej.expr.instruct.IterateInstr(getSelectExpression().copy(rebindings), (Saxon.Hej.expr.instruct.LocalParamBlock)(getInitiallyExp().copy(rebindings)), getActionExpression().copy(rebindings), getOnCompletion().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("iterate", this);
            @out.setChildRole("select");
            getSelectExpression().export(@out);
            @out.setChildRole("params");
            getInitiallyExp().export(@out);
            if (!(Saxon.Hej.expr.Literal.isEmptySequence(getOnCompletion()))) {
                @out.setChildRole("on-completion");
                getOnCompletion().export(@out);
            }
            @out.setChildRole("action");
            getActionExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.IterateInstr.IterateElaborator();
        }
        public class IterateElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.IterateInstr expr = (Saxon.Hej.expr.instruct.IterateInstr)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator select = expr.getSelectExpression().makeElaborator().elaborateForPull();
                Saxon.Hej.expr.elab.PushEvaluator initial = expr.getInitiallyExp().makeElaborator().elaborateForPush();
                Saxon.Hej.expr.elab.PushEvaluator action = expr.getActionExpression().makeElaborator().elaborateForPush();
                Saxon.Hej.expr.elab.PushEvaluator completion = expr.getOnCompletion().makeElaborator().elaborateForPush();
                return (output, context) => {
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                    c2.setOrigin(expr);
                    Saxon.Hej.om.FocusIterator iter = c2.trackFocus(/*Delegate.iterate()*/select(context));
                    c2.setCurrentTemplateRule(null);
                    Saxon.Hej.@event.PipelineConfiguration pipe = output.getPipelineConfiguration();
                    pipe.setXPathContext(c2);
                    bool tracing = context.getController().isTracing();
                    Saxon.Hej.lib.TraceListener listener = (tracing ? context.getController().getTraceListener() : null);
                    Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/initial(output, context);
                    Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                    while (true) {
                        Saxon.Hej.om.Item item = iter.next();
                        if ((item != null)) {
                            if (tracing) {
                                listener.startCurrentItem(item);
                            }
                            tc = /*Delegate.processLeavingTail()*/action(output, c2);
                            Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                            if (tracing) {
                                listener.endCurrentItem(item);
                            }
                            Saxon.Hej.expr.TailCallLoop.TailCallInfo comp = c2.getTailCallInfo();
                            if ((comp == null)) {
                            } else {
                                if ((comp is Saxon.Hej.expr.instruct.BreakInstr)) {
                                    iter.close();
                                    return null;
                                } else {
                                }
                            }
                        } else {
                            Saxon.Hej.expr.XPathContextMinor c3 = context.newMinorContext();
                            c3.setCurrentIterator(null);
                            tc = /*Delegate.processLeavingTail()*/completion(output, c3);
                            Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                            break ;
                        }
                    }
                    pipe.setXPathContext(context);
                    return null;
                } ;
            }
        
        }
    
    }
}