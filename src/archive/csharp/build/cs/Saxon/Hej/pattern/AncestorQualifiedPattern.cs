using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    #pragma warning disable 659
    public sealed class AncestorQualifiedPattern : Saxon.Hej.pattern.Pattern {
        private Saxon.Hej.pattern.Pattern basePattern;
        private Saxon.Hej.pattern.Pattern upperPattern;
        private int upwardsAxis = Saxon.Hej.om.AxisInfo.PARENT;
        private Saxon.Hej.type.ItemType refinedItemType;
        private bool testUpperPatternFirst = false;
        public AncestorQualifiedPattern(Saxon.Hej.pattern.Pattern @base, Saxon.Hej.pattern.Pattern upper, int axis)  {
            this.basePattern = @base;
            this.upperPattern = upper;
            this.upwardsAxis = axis;
            adoptChildExpression(@base);
            adoptChildExpression(upper);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(new Saxon.Hej.expr.Operand(this, upperPattern, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION), new Saxon.Hej.expr.Operand(this, basePattern, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION));
        }
        public override void bindCurrent(Saxon.Hej.expr.LocalBinding binding) {
            basePattern.bindCurrent(binding);
            upperPattern.bindCurrent(binding);
        }
        public Saxon.Hej.pattern.Pattern getBasePattern() {
            return basePattern;
        }
        public Saxon.Hej.pattern.Pattern getUpperPattern() {
            return upperPattern;
        }
        public int getUpwardsAxis() {
            return upwardsAxis;
        }
        public override bool isMotionless() {
            return (basePattern.isMotionless() && upperPattern.isMotionless());
        }
        public override bool matchesCurrentGroup() {
            return upperPattern.matchesCurrentGroup();
        }
        public override Saxon.Hej.pattern.Pattern simplify() {
            upperPattern = upperPattern.simplify();
            basePattern = basePattern.simplify();
            return this;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            basePattern = basePattern.typeCheck(visitor, contextItemType);
            upperPattern = upperPattern.typeCheck(visitor, contextItemType);
            if ((upwardsAxis == Saxon.Hej.om.AxisInfo.PARENT)) {
                Saxon.Hej.type.ItemType type = basePattern.getItemType();
                if ((type is Saxon.Hej.pattern.NodeTest)) {
                    Saxon.Hej.expr.AxisExpression step;
                    if ((type.getPrimitiveType() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        step = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, (Saxon.Hej.pattern.NodeTest)(type));
                    } else {
                        step = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, (Saxon.Hej.pattern.NodeTest)(type));
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, step);
                    Saxon.Hej.expr.Expression exp = step.typeCheck(visitor, visitor.getConfiguration().makeContextItemStaticInfo(upperPattern.getItemType(), false));
                    refinedItemType = exp.getItemType();
                }
            }
            testUpperPatternFirst = (upperPattern.getCost()<basePattern.getCost());
            return this;
        }
        public override int getDependencies() {
            return (basePattern.getDependencies()|upperPattern.getDependencies());
        }
        public override int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            nextFree = upperPattern.allocateSlots(slotManager, nextFree);
            nextFree = basePattern.allocateSlots(slotManager, nextFree);
            return nextFree;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return ((item is Saxon.Hej.om.NodeInfo) && matchesBeneathAnchor((Saxon.Hej.om.NodeInfo)(item), null, context));
        }
        public override bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            if (testUpperPatternFirst) {
                return (matchesUpperPattern(node, anchor, context) && basePattern.matches(node, context));
            } else {
                return (basePattern.matchesBeneathAnchor(node, anchor, context) && matchesUpperPattern(node, anchor, context));
            }
        }
        private bool matchesUpperPattern(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            switch (upwardsAxis) {
              case Saxon.Hej.om.AxisInfo.SELF:
                return upperPattern.matchesBeneathAnchor(node, anchor, context);
              case Saxon.Hej.om.AxisInfo.PARENT:
                Saxon.Hej.om.NodeInfo par = node.getParent();
                return ((par != null) && upperPattern.matchesBeneathAnchor(par, anchor, context));
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
                {
                    Saxon.Hej.om.NodeInfo anc = node.getParent();
                    return hasMatchingAncestor(anchor, anc, context);
                }
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                {
                    return hasMatchingAncestor(anchor, node, context);
                }
              default:
                throw new Saxon.Hej.trans.XPathException((("Unsupported axis "+Saxon.Hej.om.AxisInfo.axisName[upwardsAxis])+" in pattern"));
            }
        }
        private bool hasMatchingAncestor(Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.om.NodeInfo anc, Saxon.Hej.expr.XPathContext context) {
            while ((anc != null)) {
                if (upperPattern.matchesBeneathAnchor(anc, anchor, context)) {
                    return true;
                }
                if (anc.Equals(anchor)) {
                    return false;
                }
                anc = anc.getParent();
            }
            return false;
        }
        public override Saxon.Hej.type.UType getUType() {
            return basePattern.getUType();
        }
        public override int getFingerprint() {
            return basePattern.getFingerprint();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((refinedItemType != null)) {
                return refinedItemType;
            }
            return basePattern.getItemType();
        }
        public override Saxon.Hej.pattern.Pattern convertToTypedPattern(string val) {
            if (upperPattern.getUType().Equals(Saxon.Hej.type.UType.DOCUMENT)) {
                Saxon.Hej.pattern.Pattern b2 = basePattern.convertToTypedPattern(val);
                if ((b2 == basePattern)) {
                    return this;
                } else {
                    return new Saxon.Hej.pattern.AncestorQualifiedPattern(b2, upperPattern, upwardsAxis);
                }
            } else {
                Saxon.Hej.pattern.Pattern u2 = upperPattern.convertToTypedPattern(val);
                if ((u2 == upperPattern)) {
                    return this;
                } else {
                    return new Saxon.Hej.pattern.AncestorQualifiedPattern(basePattern, u2, upwardsAxis);
                }
            }
        }
        public override string reconstruct() {
            return ((upperPattern+(((upwardsAxis == Saxon.Hej.om.AxisInfo.PARENT) ? "/" : "//")))+basePattern);
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.pattern.AncestorQualifiedPattern)) {
                Saxon.Hej.pattern.AncestorQualifiedPattern aqp = (Saxon.Hej.pattern.AncestorQualifiedPattern)(other);
                return ((basePattern.isEqual(aqp.basePattern) && upperPattern.isEqual(aqp.upperPattern)) && (upwardsAxis == aqp.upwardsAxis));
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            return (((88267^basePattern.GetHashCode())^upperPattern.GetHashCode())^((upwardsAxis<<22)));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.withUpper");
            presenter.emitAttribute("axis", Saxon.Hej.om.AxisInfo.axisName[getUpwardsAxis()]);
            presenter.emitAttribute("upFirst", (""+testUpperPatternFirst));
            basePattern.export(presenter);
            upperPattern.export(presenter);
            presenter.endElement();
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.pattern.AncestorQualifiedPattern n = new Saxon.Hej.pattern.AncestorQualifiedPattern(basePattern.copy(rebindings), upperPattern.copy(rebindings), upwardsAxis);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, n);
            n.setOriginalText(getOriginalText());
            return n;
        }
    
    }
}