using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class AdjacentTextNodeMerger : Saxon.Hej.expr.UnaryExpression {
        public AdjacentTextNodeMerger(Saxon.Hej.expr.Expression p0) : base (p0) {
        }
        public static Saxon.Hej.expr.Expression makeAdjacentTextNodeMerger(Saxon.Hej.expr.Expression @base) {
            if (((@base is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(@base)).getGroundedValue() is Saxon.Hej.om.AtomicSequence))) {
                return @base;
            } else {
                return new Saxon.Hej.expr.AdjacentTextNodeMerger(@base);
            }
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            if (((operand is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(operand)).getGroundedValue() is Saxon.Hej.value.AtomicValue))) {
                return operand;
            } else {
                return base.simplify();
            }
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if ((th.relationship(getBaseExpression().getItemType(), Saxon.Hej.pattern.NodeKindTest.TEXT) == Saxon.Hej.type.Affinity.DISJOINT)) {
                Saxon.Hej.expr.Expression @base = getBaseExpression();
                @base.setParentExpression(getParentExpression());
                return @base;
            }
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getBaseExpression().getCardinality()))) {
                Saxon.Hej.expr.Expression @base = getBaseExpression();
                @base.setParentExpression(getParentExpression());
                return @base;
            }
            if ((getBaseExpression() is Saxon.Hej.expr.instruct.Choose)) {
                Saxon.Hej.expr.instruct.Choose choose = (Saxon.Hej.expr.instruct.Choose)(getBaseExpression());
                for (int i = 0; (i<choose.size()); i++) {
                    Saxon.Hej.expr.AdjacentTextNodeMerger atm2 = new Saxon.Hej.expr.AdjacentTextNodeMerger(choose.getAction(i));
                    choose.setAction(i, atm2.typeCheck(visitor, contextInfo));
                }
                return choose;
            }
            if ((getBaseExpression() is Saxon.Hej.expr.instruct.Block)) {
                Saxon.Hej.expr.instruct.Block block = (Saxon.Hej.expr.instruct.Block)(getBaseExpression());
                Saxon.Hej.expr.Operand[] actions = block.getOperanda();
                bool prevtext = false;
                bool needed = false;
                bool maybeEmpty = false;
                foreach (Saxon.Hej.expr.Operand o in actions) {
                    Saxon.Hej.expr.Expression action = o.getChildExpression();
                    bool maybetext;
                    if ((action is Saxon.Hej.expr.instruct.ValueOf)) {
                        maybetext = true;
                        Saxon.Hej.expr.Expression content = ((Saxon.Hej.expr.instruct.ValueOf)(action)).getSelect();
                        if ((content is Saxon.Hej.expr.StringLiteral)) {
                            maybeEmpty |= ((Saxon.Hej.expr.StringLiteral)(content)).getString().isEmpty();
                        } else {
                            maybeEmpty = true;
                        }
                    } else {
                        maybetext = (th.relationship(action.getItemType(), Saxon.Hej.pattern.NodeKindTest.TEXT) != Saxon.Hej.type.Affinity.DISJOINT);
                        maybeEmpty |= maybetext;
                    }
                    if ((prevtext && maybetext)) {
                        needed = true;
                        break ;
                    }
                    if ((maybetext && Saxon.Hej.value.Cardinality.allowsMany(action.getCardinality()))) {
                        needed = true;
                        break ;
                    }
                    prevtext = maybetext;
                }
                if (!(needed)) {
                    if (maybeEmpty) {
                        return new Saxon.Hej.expr.EmptyTextNodeRemover(block);
                    } else {
                        return block;
                    }
                }
            }
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getBaseExpression().getStaticUType(contextItemType);
        }
        protected internal override int computeCardinality() {
            return (getBaseExpression().getCardinality()|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.AdjacentTextNodeMerger a2 = new Saxon.Hej.expr.AdjacentTextNodeMerger(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            return a2;
        }
        public override int getImplementationMethod() {
            return (((Saxon.Hej.expr.Expression.PROCESS_METHOD|Saxon.Hej.expr.Expression.ITERATE_METHOD)|ITEM_FEED_METHOD)|WATCH_METHOD);
        }
        public override string getStreamerName() {
            return "AdjacentTextNodeMerger";
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.tree.iter.AdjacentTextNodeMergingIterator(getBaseExpression().iterate(context));
        }
        public override string getExpressionName() {
            return "mergeAdj";
        }
        public static bool isTextNode(Saxon.Hej.om.Item item) {
            return ((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.TEXT));
        }
    
    }
}