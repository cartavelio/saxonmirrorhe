using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class EmptyTextNodeRemover : Saxon.Hej.expr.UnaryExpression, Saxon.Hej.expr.ItemMappingFunction {
        public EmptyTextNodeRemover(Saxon.Hej.expr.Expression p0) : base (p0) {
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        protected internal override int computeCardinality() {
            return (getBaseExpression().getCardinality()|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.EmptyTextNodeRemover e2 = new Saxon.Hej.expr.EmptyTextNodeRemover(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e2);
            return e2;
        }
        public override int getImplementationMethod() {
            return ((Saxon.Hej.expr.Expression.ITERATE_METHOD|ITEM_FEED_METHOD)|WATCH_METHOD);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.ItemMappingIterator(getBaseExpression().iterate(context), this);
        }
        public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
            if ((((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.TEXT)) && item.getUnicodeStringValue().isEmpty())) {
                return null;
            } else {
                return item;
            }
        }
        public override string getStreamerName() {
            return "EmptyTextNodeRemover";
        }
        public override string getExpressionName() {
            return "emptyTextNodeRemover";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.EmptyTextNodeRemover.EmptyTextNodeRemoverElaborator();
        }
        public class EmptyTextNodeRemoverElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.EmptyTextNodeRemover expr = (Saxon.Hej.expr.EmptyTextNodeRemover)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBaseExpression().makeElaborator().elaborateForPull();
                return context => new Saxon.Hej.expr.ItemMappingIterator(/*Delegate.iterate()*/baseEval(context), expr);
            }
        
        }
    
    }
}