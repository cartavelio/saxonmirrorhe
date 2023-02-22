using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class DocumentSorter : Saxon.Hej.expr.UnaryExpression {
        private readonly System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> comparer;
        public DocumentSorter(Saxon.Hej.expr.Expression @base) : base (@base) {
            int props = @base.getSpecialProperties();
            if ((((((props&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0)) || (((props&Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)) != 0))) {
                comparer = Saxon.Hej.expr.sort.LocalOrderComparer.getInstance();
            } else {
                comparer = Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance();
            }
        }
        public DocumentSorter(Saxon.Hej.expr.Expression @base, bool intraDocument) : base (@base) {
            if (intraDocument) {
                comparer = Saxon.Hej.expr.sort.LocalOrderComparer.getInstance();
            } else {
                comparer = Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance();
            }
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public override string getExpressionName() {
            return "docOrder";
        }
        public System.Collections.Generic.Comparer<Saxon.Hej.om.NodeInfo> getComparer() {
            return comparer;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.Expression operand = getBaseExpression().simplify();
            if (operand.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) {
                return operand;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e2 = base.typeCheck(visitor, contextInfo);
            if ((e2 != this)) {
                return e2;
            }
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if ((th.relationship(getBaseExpression().getItemType(), Saxon.Hej.pattern.AnyNodeTest.getInstance()) == Saxon.Hej.type.Affinity.DISJOINT)) {
                return getBaseExpression();
            }
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "document-order sorter", 0);
            Saxon.Hej.expr.Expression operand = visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(getBaseExpression(), Saxon.Hej.value.SequenceType.NODE_SEQUENCE, role, visitor);
            setBaseExpression(operand);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            Saxon.Hej.expr.Expression sortable = getBaseExpression();
            bool tryHarder = sortable.isStaticPropertiesKnown();
            while (true) {
                if (sortable.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) {
                    return sortable;
                }
                if (!(Saxon.Hej.value.Cardinality.allowsMany(sortable.getCardinality()))) {
                    return sortable;
                }
                if ((sortable is Saxon.Hej.expr.SlashExpression)) {
                    Saxon.Hej.expr.SlashExpression slash = (Saxon.Hej.expr.SlashExpression)(sortable);
                    Saxon.Hej.expr.Expression lhs = slash.getLhsExpression();
                    Saxon.Hej.expr.Expression rhs = slash.getRhsExpression();
                    if (((lhs is Saxon.Hej.expr.sort.ConditionalSorter) && slash.getRhsExpression().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.PEER_NODESET))) {
                        Saxon.Hej.expr.sort.ConditionalSorter c = (Saxon.Hej.expr.sort.ConditionalSorter)(lhs);
                        Saxon.Hej.expr.sort.DocumentSorter d = c.getDocumentSorter();
                        Saxon.Hej.expr.Expression condition = c.getCondition();
                        Saxon.Hej.expr.Expression s = new Saxon.Hej.expr.SlashExpression(d.getBaseExpression(), rhs);
                        s = s.optimize(visitor, contextInfo);
                        return new Saxon.Hej.expr.sort.ConditionalSorter(condition, new Saxon.Hej.expr.sort.DocumentSorter(s));
                    }
                    if ((((lhs is Saxon.Hej.expr.sort.DocumentSorter) && (rhs is Saxon.Hej.expr.AxisExpression)) && (((Saxon.Hej.expr.AxisExpression)(rhs)).getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                        Saxon.Hej.expr.SlashExpression s1 = new Saxon.Hej.expr.SlashExpression(((Saxon.Hej.expr.sort.DocumentSorter)(lhs)).getBaseExpression(), rhs);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, s1);
                        return new Saxon.Hej.expr.sort.DocumentSorter(s1).optimize(visitor, contextInfo);
                    }
                    if (((!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(rhs)) && !(rhs.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS))) && rhs.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED))) {
                        Saxon.Hej.expr.Expression e1 = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(slash.getLhsExpression());
                        Saxon.Hej.expr.Expression e2 = new Saxon.Hej.expr.sort.DocumentSorter(slash.getRhsExpression());
                        Saxon.Hej.expr.SlashExpression e3 = new Saxon.Hej.expr.SlashExpression(e1, e2);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e3);
                        return e3.optimize(visitor, contextInfo);
                    }
                }
                if (tryHarder) {
                    sortable.resetLocalStaticProperties();
                    tryHarder = false;
                } else {
                    break ;
                }
            }
            if ((((sortable is Saxon.Hej.expr.SlashExpression) && !(visitor.isOptimizeForStreaming())) && !(((getParentExpression() is Saxon.Hej.expr.sort.ConditionalSorter))))) {
                return visitor.obtainOptimizer().makeConditionalDocumentSorter(this, (Saxon.Hej.expr.SlashExpression)(sortable));
            }
            return this;
        }
        public override int getNetCost() {
            return 30;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            Saxon.Hej.expr.Expression operand = getBaseExpression().unordered(retainAllNodes, forStreaming);
            if (operand.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) {
                return operand;
            }
            if (!(retainAllNodes)) {
                return operand;
            } else {
                if ((operand is Saxon.Hej.expr.SlashExpression)) {
                    Saxon.Hej.expr.SlashExpression exp = (Saxon.Hej.expr.SlashExpression)(operand);
                    Saxon.Hej.expr.Expression a = exp.getSelectExpression();
                    Saxon.Hej.expr.Expression b = exp.getActionExpression();
                    a = Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(a, false);
                    b = Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(b, false);
                    if (((((a is Saxon.Hej.expr.AxisExpression) && (((((Saxon.Hej.expr.AxisExpression)(a)).getAxis() == Saxon.Hej.om.AxisInfo.DESCENDANT) || (((Saxon.Hej.expr.AxisExpression)(a)).getAxis() == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF)))) && (b is Saxon.Hej.expr.AxisExpression)) && (((Saxon.Hej.expr.AxisExpression)(b)).getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                        return operand.unordered(retainAllNodes, false);
                    }
                }
            }
            setBaseExpression(operand);
            return this;
        }
        protected internal override int computeSpecialProperties() {
            return (getBaseExpression().getSpecialProperties()|Saxon.Hej.expr.StaticProperty.ORDERED_NODESET);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.sort.DocumentSorter ds = new Saxon.Hej.expr.sort.DocumentSorter(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ds);
            return ds;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            return getBaseExpression().toPattern(config);
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.sort.DocumentOrderIterator(getBaseExpression().iterate(context), comparer);
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return getBaseExpression().effectiveBooleanValue(context);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("docOrder", this);
            @out.emitAttribute("intra", ((comparer is Saxon.Hej.expr.sort.LocalOrderComparer) ? "1" : "0"));
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string getStreamerName() {
            return "DocumentSorterAdjunct";
        }
    
    }
}