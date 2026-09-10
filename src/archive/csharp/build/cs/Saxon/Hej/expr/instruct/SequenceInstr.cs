using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class SequenceInstr : Saxon.Hej.expr.UnaryExpression {
        public SequenceInstr(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        public override bool isInstruction() {
            return true;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.TRANSMISSION);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return new Saxon.Hej.expr.instruct.SequenceInstr(getBaseExpression().copy(rebindings));
        }
        public override int getImplementationMethod() {
            return getBaseExpression().getImplementationMethod();
        }
        public override string getExpressionName() {
            return "sequence";
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            dispatchTailCall(/*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context));
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return getBaseExpression().iterate(context);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            getBaseExpression().export(@out);
        }
        public override string getStreamerName() {
            return "SequenceInstr";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.SequenceInstr.SequenceInstrElaborator();
        }
        public class SequenceInstrElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.UnaryExpression expr = (Saxon.Hej.expr.UnaryExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator basePull = expr.getBaseExpression().makeElaborator().elaborateForPull();
                return context => /*Delegate.iterate()*/basePull(context);
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.UnaryExpression expr = (Saxon.Hej.expr.UnaryExpression)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator basePush = expr.getBaseExpression().makeElaborator().elaborateForPush();
                return (output, context) => /*Delegate.processLeavingTail()*/basePush(output, context);
            }
        
        }
    
    }
}