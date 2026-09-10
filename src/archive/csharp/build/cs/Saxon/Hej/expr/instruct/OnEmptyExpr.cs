using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class OnEmptyExpr : Saxon.Hej.expr.UnaryExpression {
        public OnEmptyExpr(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        public override bool isInstruction() {
            return true;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.TRANSMISSION);
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
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
            return new Saxon.Hej.expr.instruct.OnEmptyExpr(getBaseExpression().copy(rebindings));
        }
        public override int getImplementationMethod() {
            return getBaseExpression().getImplementationMethod();
        }
        public override string getExpressionName() {
            return "onEmpty";
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            dispatchTailCall(/*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context));
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return getBaseExpression().iterate(context);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("onEmpty", this);
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string getStreamerName() {
            return "OnEmpty";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.SequenceInstr.SequenceInstrElaborator();
        }
    
    }
}