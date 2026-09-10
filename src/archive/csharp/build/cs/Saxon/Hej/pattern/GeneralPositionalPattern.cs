using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public class GeneralPositionalPattern : Saxon.Hej.pattern.Pattern {
        private readonly Saxon.Hej.pattern.NodeTest nodeTest;
        private Saxon.Hej.expr.Expression positionExpr;
        private bool usesPosition = true;
        public GeneralPositionalPattern(Saxon.Hej.pattern.NodeTest @base, Saxon.Hej.expr.Expression positionExpr)  {
            this.nodeTest = @base;
            this.positionExpr = positionExpr;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return new Saxon.Hej.expr.Operand(this, positionExpr, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
        }
        public Saxon.Hej.expr.Expression getPositionExpr() {
            return positionExpr;
        }
        public Saxon.Hej.pattern.NodeTest getNodeTest() {
            return nodeTest;
        }
        public void setUsesPosition(bool usesPosition) {
            this.usesPosition = usesPosition;
        }
        public override Saxon.Hej.pattern.Pattern simplify() {
            positionExpr = positionExpr.simplify();
            return this;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(getItemType(), false);
            positionExpr = positionExpr.typeCheck(visitor, cit);
            positionExpr = Saxon.Hej.expr.parser.ExpressionTool.unsortedIfHomogeneous(positionExpr, false);
            return this;
        }
        public override Saxon.Hej.pattern.Pattern optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(getItemType(), false);
            positionExpr = positionExpr.optimize(visitor, cit);
            if (Saxon.Hej.expr.Literal.isConstantBoolean(positionExpr, true)) {
                return new Saxon.Hej.pattern.NodeTestPattern(nodeTest);
            } else {
                if (Saxon.Hej.expr.Literal.isConstantBoolean(positionExpr, false)) {
                    return new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
                }
            }
            if ((((positionExpr.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION)) == 0)) {
                usesPosition = false;
            }
            if (!(Saxon.Hej.expr.FilterExpression.isPositionalFilter(positionExpr, config.getTypeHierarchy()))) {
                byte axis = Saxon.Hej.om.AxisInfo.CHILD;
                if ((nodeTest.getPrimitiveType() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    axis = Saxon.Hej.om.AxisInfo.ATTRIBUTE;
                } else {
                    if ((nodeTest.getPrimitiveType() == Saxon.Hej.type.Type.NAMESPACE)) {
                        axis = Saxon.Hej.om.AxisInfo.NAMESPACE;
                    }
                }
                Saxon.Hej.expr.AxisExpression ae = new Saxon.Hej.expr.AxisExpression(axis, nodeTest);
                Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(ae, positionExpr);
                return Saxon.Hej.pattern.PatternMaker.fromExpression(fe, config, true).typeCheck(visitor, contextInfo);
            }
            return this;
        }
        public override int getDependencies() {
            return (positionExpr.getDependencies()&((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS)));
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            return Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(positionExpr, nextFree, slotManager);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return ((item is Saxon.Hej.om.NodeInfo) && matchesBeneathAnchor((Saxon.Hej.om.NodeInfo)(item), null, context));
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return internalMatches(node, anchor, context);
        }
        private bool internalMatches(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            if (!(nodeTest.test(node))) {
                return false;
            }
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator(node);
            c2.setCurrentIterator(iter);
            try {
                Saxon.Hej.expr.XPathContext c = c2;
                int actualPosition = -1;
                if (usesPosition) {
                    actualPosition = getActualPosition(node, System.Int32.MaxValue, context.getCurrentIterator());
                    Saxon.Hej.tree.iter.ManualIterator man = new Saxon.Hej.tree.iter.ManualIterator(node, actualPosition);
                    Saxon.Hej.expr.XPathContext c3 = c2.newMinorContext();
                    c3.setCurrentIterator(man);
                    c = c3;
                }
                Saxon.Hej.om.Item predicate = positionExpr.evaluateItem(c);
                if ((predicate is Saxon.Hej.value.NumericValue)) {
                    Saxon.Hej.value.NumericValue position = (Saxon.Hej.value.NumericValue)(positionExpr.evaluateItem(context));
                    int requiredPos = position.asSubscript();
                    if (((actualPosition<0) && (requiredPos != -1))) {
                        actualPosition = getActualPosition(node, requiredPos, context.getCurrentIterator());
                    }
                    return ((requiredPos != -1) && (actualPosition == requiredPos));
                } else {
                    return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(predicate);
                }
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException.Circularity || e is Saxon.Hej.trans.XPathException.StackOverflow) {
                throw;
            } catch (Saxon.Hej.trans.XPathException e) {
                handleDynamicError(e, c2);
                return false;
            }
        }
        private int getActualPosition(Saxon.Hej.om.NodeInfo node, int max, Saxon.Hej.om.FocusIterator iterator) {
            if ((iterator is Saxon.Hej.om.FocusTrackingIterator)) {
                return ((Saxon.Hej.om.FocusTrackingIterator)(iterator)).getSiblingPosition(node, nodeTest, max);
            }
            return Saxon.Hej.tree.util.Navigator.getSiblingPosition(node, nodeTest, max);
        }
        public override Saxon.Hej.type.UType getUType() {
            return nodeTest.getUType();
        }
        public override int getFingerprint() {
            return nodeTest.getFingerprint();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return nodeTest;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.pattern.GeneralPositionalPattern)) {
                Saxon.Hej.pattern.GeneralPositionalPattern fp = (Saxon.Hej.pattern.GeneralPositionalPattern)(other);
                return (nodeTest.Equals(fp.nodeTest) && positionExpr.isEqual(fp.positionExpr));
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            return (nodeTest.GetHashCode()^positionExpr.GetHashCode());
        }
        public override bool isMotionless() {
            return false;
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.GeneralPositionalPattern n = new Saxon.Hej.pattern.GeneralPositionalPattern(nodeTest.copy(), positionExpr.copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override string reconstruct() {
            return (((nodeTest+"[")+positionExpr)+"]");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.genPos");
            presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(nodeTest));
            if (!(usesPosition)) {
                presenter.emitAttribute("flags", "P");
            }
            positionExpr.export(presenter);
            presenter.endElement();
        }
    
    }
}