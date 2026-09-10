using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class ConditionalSorter : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand conditionOp;
        private readonly Saxon.Hej.expr.Operand sorterOp;
        private delegate Saxon.Hej.expr.Expression RewriteAction (Saxon.Hej.expr.Expression e);
        private readonly static Saxon.Hej.expr.OperandRole DOC_SORTER_ROLE = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.CONSTRAINED_CLASS, Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE, expr => (expr is Saxon.Hej.expr.sort.DocumentSorter));
        public ConditionalSorter(Saxon.Hej.expr.Expression condition, Saxon.Hej.expr.sort.DocumentSorter sorter)  {
            conditionOp = new Saxon.Hej.expr.Operand(this, condition, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            sorterOp = new Saxon.Hej.expr.Operand(this, sorter, DOC_SORTER_ROLE);
            adoptChildExpression(condition);
            adoptChildExpression(sorter);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(conditionOp, sorterOp);
        }
        public void setCondition(Saxon.Hej.expr.Expression condition) {
            conditionOp.setChildExpression(condition);
        }
        public void setDocumentSorter(Saxon.Hej.expr.sort.DocumentSorter documentSorter) {
            sorterOp.setChildExpression(documentSorter);
        }
        public Saxon.Hej.expr.Expression getCondition() {
            return conditionOp.getChildExpression();
        }
        public Saxon.Hej.expr.sort.DocumentSorter getDocumentSorter() {
            return (Saxon.Hej.expr.sort.DocumentSorter)(sorterOp.getChildExpression());
        }
        public override Saxon.Hej.expr.Expression simplify() {
            return rewrite(x => x.simplify() /* method reference */);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return rewrite(exp => exp.typeCheck(visitor, contextInfo));
        }
        public override int getCardinality() {
            return getDocumentSorter().getCardinality();
        }
        protected internal override int computeSpecialProperties() {
            return (getCondition().getSpecialProperties()|(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET&~(Saxon.Hej.expr.StaticProperty.REVERSE_DOCUMENT_ORDER)));
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return rewrite(exp => exp.optimize(visitor, contextInfo));
        }
        private Saxon.Hej.expr.Expression rewrite(Saxon.Hej.expr.sort.ConditionalSorter.RewriteAction rewriter) {
            Saxon.Hej.expr.Expression @base = /*Delegate.rewrite()*/rewriter(getDocumentSorter());
            if ((@base is Saxon.Hej.expr.sort.DocumentSorter)) {
                sorterOp.setChildExpression(@base);
            } else {
                return @base;
            }
            Saxon.Hej.expr.Expression cond = /*Delegate.rewrite()*/rewriter(getCondition());
            if ((cond is Saxon.Hej.expr.Literal)) {
                bool b = ((Saxon.Hej.expr.Literal)(cond)).getGroundedValue().effectiveBooleanValue();
                if (b) {
                    return @base;
                } else {
                    return ((Saxon.Hej.expr.sort.DocumentSorter)(@base)).getBaseExpression();
                }
            } else {
                conditionOp.setChildExpression(cond);
                return this;
            }
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            Saxon.Hej.expr.Expression @base = getDocumentSorter().unordered(retainAllNodes, forStreaming);
            if ((@base is Saxon.Hej.expr.sort.DocumentSorter)) {
                return this;
            } else {
                return @base;
            }
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.sort.ConditionalSorter cs = new Saxon.Hej.expr.sort.ConditionalSorter(getCondition().copy(rebindings), (Saxon.Hej.expr.sort.DocumentSorter)(getDocumentSorter().copy(rebindings)));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cs);
            return cs;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("conditionalSort", this);
            getCondition().export(@out);
            getDocumentSorter().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getDocumentSorter().getItemType();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override string getExpressionName() {
            return "conditionalSort";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.sort.ConditionalSorter.ConditionalSorterElaborator();
        }
        public class ConditionalSorterElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.sort.ConditionalSorter expr = (Saxon.Hej.expr.sort.ConditionalSorter)(getExpression());
                Saxon.Hej.expr.elab.BooleanEvaluator condition = expr.getCondition().makeElaborator().elaborateForBoolean();
                Saxon.Hej.expr.elab.PullEvaluator sorter = expr.getDocumentSorter().makeElaborator().elaborateForPull();
                Saxon.Hej.expr.elab.PullEvaluator nonSorter = expr.getDocumentSorter().getBaseExpression().makeElaborator().elaborateForPull();
                return context => {
                    bool b = /*Delegate.eval()*/condition(context);
                    if (b) {
                        return /*Delegate.iterate()*/sorter(context);
                    } else {
                        return /*Delegate.iterate()*/nonSorter(context);
                    }
                } ;
            }
        
        }
    
    }
}