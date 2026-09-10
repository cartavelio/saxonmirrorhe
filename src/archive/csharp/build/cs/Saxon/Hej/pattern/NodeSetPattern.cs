using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class NodeSetPattern : Saxon.Hej.pattern.Pattern {
        private readonly Saxon.Hej.expr.Operand selectionOp;
        private Saxon.Hej.type.ItemType itemType;
        public NodeSetPattern(Saxon.Hej.expr.Expression exp)  {
            selectionOp = new Saxon.Hej.expr.Operand(this, exp, Saxon.Hej.expr.OperandRole.NAVIGATE);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return selectionOp;
        }
        public Saxon.Hej.expr.Expression getSelectionExpression() {
            return selectionOp.getChildExpression();
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            selectionOp.setChildExpression(getSelectionExpression().typeCheck(visitor, contextItemType));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MATCH_PATTERN, getSelectionExpression().ToString(), 0);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Hej.expr.Expression @checked;
            try {
                @checked = tc.staticTypeCheck(getSelectionExpression(), Saxon.Hej.value.SequenceType.NODE_SEQUENCE, role, visitor);
            } catch (Saxon.Hej.trans.XPathException e) {
                visitor.issueWarning(("Pattern will never match anything. "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXWN9015, getLocation());
                @checked = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            selectionOp.setChildExpression(@checked);
            itemType = getSelectionExpression().getItemType();
            return this;
        }
        public override Saxon.Hej.pattern.Pattern optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            visitor.obtainOptimizer().optimizeNodeSetPattern(this);
            return this;
        }
        public void setItemType(Saxon.Hej.type.ItemType type) {
            this.itemType = type;
        }
        public override int getDependencies() {
            return getSelectionExpression().getDependencies();
        }
        public override bool isMotionless() {
            Saxon.Hej.expr.Expression select = getSelectionExpression();
            /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(select, new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(getItemType(), false, Saxon.Eej.ee.stream.Posture.STRIDING), null);
            return (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(select) == Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            return Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(getSelectionExpression(), nextFree, slotManager);
        }
        public override Saxon.Hej.om.SequenceIterator selectNodes(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            Saxon.Hej.tree.iter.ManualIterator mi = new Saxon.Hej.tree.iter.ManualIterator(doc.getRootNode());
            c2.setCurrentIterator(mi);
            return getSelectionExpression().iterate(c2);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                try {
                    Saxon.Hej.expr.Expression exp = getSelectionExpression();
                    if ((exp is Saxon.Hej.expr.GlobalVariableReference)) {
                        Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.GlobalVariableReference)(exp)).evaluateVariable(context);
                        return value.containsNode((Saxon.Hej.om.NodeInfo)(item));
                    } else {
                        Saxon.Hej.om.SequenceIterator iter = exp.iterate(context);
                        return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iter, (Saxon.Hej.om.NodeInfo)(item));
                    }
                } catch (Exception e) when (e is Saxon.Hej.trans.XPathException.Circularity || e is Saxon.Hej.trans.XPathException.StackOverflow) {
                    throw;
                } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.UncheckedXPathException) {
                    return false;
                }
            } else {
                return false;
            }
        }
        public override Saxon.Hej.type.UType getUType() {
            return getItemType().getUType();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((itemType == null)) {
                itemType = getSelectionExpression().getItemType();
            }
            if ((itemType is Saxon.Hej.pattern.NodeTest)) {
                return itemType;
            } else {
                return Saxon.Hej.pattern.AnyNodeTest.getInstance();
            }
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.pattern.NodeSetPattern)) && ((Saxon.Hej.pattern.NodeSetPattern)(other)).getSelectionExpression().isEqual(getSelectionExpression()));
        }
        protected internal override int computeHashCode() {
            return (0x73108728^getSelectionExpression().GetHashCode());
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.NodeSetPattern n = new Saxon.Hej.pattern.NodeSetPattern(getSelectionExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.nodeSet");
            if ((itemType != null)) {
                presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(itemType));
            }
            getSelectionExpression().export(presenter);
            presenter.endElement();
        }
    
    }
}