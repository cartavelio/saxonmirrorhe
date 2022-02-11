using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public sealed class GeneralNodePattern : Saxon.Hej.pattern.Pattern {
        private Saxon.Hej.expr.Expression equivalentExpr = null;
        private Saxon.Hej.pattern.NodeTest itemType = null;
        public GeneralNodePattern(Saxon.Hej.expr.Expression expr, Saxon.Hej.pattern.NodeTest itemType)  {
            equivalentExpr = expr;
            this.itemType = itemType;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return new Saxon.Hej.expr.Operand(this, equivalentExpr, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
        }
        public override bool isMotionless() {
            return false;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().getDefaultContextItemStaticInfo();
            equivalentExpr = equivalentExpr.typeCheck(visitor, cit);
            return this;
        }
        public override Saxon.Hej.pattern.Pattern optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.ContextItemStaticInfo defaultInfo = config.getDefaultContextItemStaticInfo();
            equivalentExpr = equivalentExpr.optimize(visitor, defaultInfo);
            if (((equivalentExpr is Saxon.Hej.expr.FilterExpression) && !(((Saxon.Hej.expr.FilterExpression)(equivalentExpr)).isFilterIsPositional()))) {
                try {
                    return Saxon.Hej.pattern.PatternMaker.fromExpression(equivalentExpr, config, true).typeCheck(visitor, defaultInfo);
                } catch (Saxon.Hej.trans.XPathException err) {
                }
            }
            return this;
        }
        public override int getDependencies() {
            return (equivalentExpr.getDependencies()&((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS)));
        }
        public override void bindCurrent(Saxon.Hej.expr.LocalBinding binding) {
            if (Saxon.Hej.expr.parser.ExpressionTool.callsFunction(equivalentExpr, Saxon.Hej.functions.Current.FN_CURRENT, false)) {
                if (equivalentExpr.isCallOn(typeof(Saxon.Hej.functions.Current))) {
                    equivalentExpr = new Saxon.Hej.expr.LocalVariableReference(binding);
                } else {
                    replaceCurrent(equivalentExpr, binding);
                }
            }
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            return Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(equivalentExpr, nextFree, slotManager);
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            if (!(itemType.matches(item, th))) {
                return false;
            }
            Saxon.Hej.tree.iter.AxisIterator anc = ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF);
            while (true) {
                Saxon.Hej.om.NodeInfo a = ((/*1430*/Saxon.Hej.om.NodeInfo)anc.next());
                if ((a == null)) {
                    return false;
                }
                if (matchesBeneathAnchor((Saxon.Hej.om.NodeInfo)(item), a, context)) {
                    return true;
                }
            }
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            if (!(itemType.test(node))) {
                return false;
            }
            if ((anchor == null)) {
                Saxon.Hej.tree.iter.AxisIterator ancestors = node.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF);
                while (true) {
                    Saxon.Hej.om.NodeInfo ancestor = ((/*1430*/Saxon.Hej.om.NodeInfo)ancestors.next());
                    if ((ancestor == null)) {
                        return false;
                    }
                    if (matchesBeneathAnchor(node, ancestor, context)) {
                        return true;
                    }
                }
            }
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator(anchor);
            c2.setCurrentIterator(iter);
            try {
                Saxon.Hej.om.SequenceIterator nsv = equivalentExpr.iterate(c2);
                while (true) {
                    Saxon.Hej.om.NodeInfo n = (Saxon.Hej.om.NodeInfo)(nsv.next());
                    if ((n == null)) {
                        return false;
                    }
                    if (n.Equals(node)) {
                        return true;
                    }
                }
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException.Circularity || e is Saxon.Hej.trans.XPathException.StackOverflow) {
                throw;
            } catch (Saxon.Hej.trans.XPathException e) {
                handleDynamicError(e, c2);
                return false;
            }
        }
        public override Saxon.Hej.type.UType getUType() {
            return itemType.getUType();
        }
        public override int getFingerprint() {
            return itemType.getFingerprint();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return itemType;
        }
        public Saxon.Hej.expr.Expression getEquivalentExpr() {
            return equivalentExpr;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.pattern.GeneralNodePattern)) {
                Saxon.Hej.pattern.GeneralNodePattern lpp = (Saxon.Hej.pattern.GeneralNodePattern)(other);
                return equivalentExpr.isEqual(lpp.equivalentExpr);
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            return (83641^equivalentExpr.GetHashCode());
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.GeneralNodePattern n = new Saxon.Hej.pattern.GeneralNodePattern(equivalentExpr.copy(rebindings), itemType);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.genNode");
            presenter.emitAttribute("test", Saxon.Hej.type.AlphaCode.fromItemType(itemType));
            equivalentExpr.export(presenter);
            presenter.endElement();
        }
    
    }
}