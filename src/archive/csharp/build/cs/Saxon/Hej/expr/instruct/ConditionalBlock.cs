using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ConditionalBlock : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand[] operanda;
        private bool allNodesUntyped;
        public ConditionalBlock(Saxon.Hej.expr.Expression[] children)  {
            operanda = new Saxon.Hej.expr.Operand[children.Length];
            for (int i = 0; (i<children.Length); i++) {
                operanda[i] = new Saxon.Hej.expr.Operand(this, children[i], Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
                if ((children[i] is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                    operanda[i].setOperandRole(Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION.withConstrainedClass());
                }
            }
        }
        public ConditionalBlock(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children) : this (children.ToArray()) {
        }
        public Saxon.Hej.expr.Expression getChildExpression(int n) {
            return operanda[n].getChildExpression();
        }
        public int size() {
            return operanda.Length;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return Saxon.Ejava.util.Arrays.asList(operanda);
        }
        public override string getExpressionName() {
            return "condSeq";
        }
        protected internal override int computeSpecialProperties() {
            if ((size() == 0)) {
                return (Saxon.Hej.expr.StaticProperty.SPECIAL_PROPERTY_MASK&~(Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS));
            }
            int p = base.computeSpecialProperties();
            if (allNodesUntyped) {
                p |= Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED;
            }
            bool allAxisExpressions = true;
            bool allChildAxis = true;
            bool allSubtreeAxis = true;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (!(((child is Saxon.Hej.expr.AxisExpression)))) {
                    allAxisExpressions = false;
                    allChildAxis = false;
                    allSubtreeAxis = false;
                    break ;
                }
                int axis = ((Saxon.Hej.expr.AxisExpression)(child)).getAxis();
                if ((axis != Saxon.Hej.om.AxisInfo.CHILD)) {
                    allChildAxis = false;
                }
                if (!(Saxon.Hej.om.AxisInfo.isSubtreeAxis[axis])) {
                    allSubtreeAxis = false;
                }
            }
            if (allAxisExpressions) {
                p |= ((Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
                if (allChildAxis) {
                    p |= Saxon.Hej.expr.StaticProperty.PEER_NODESET;
                }
                if (allSubtreeAxis) {
                    p |= Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET;
                }
                if ((((size() == 2) && (((Saxon.Hej.expr.AxisExpression)(getChildExpression(0))).getAxis() == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) && (((Saxon.Hej.expr.AxisExpression)(getChildExpression(1))).getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                    p |= Saxon.Hej.expr.StaticProperty.ORDERED_NODESET;
                }
            }
            return p;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression[] c2 = new Saxon.Hej.expr.Expression[size()];
            for (int c = 0; (c<size()); c++) {
                c2[c] = getChildExpression(c).copy(rebindings);
            }
            Saxon.Hej.expr.instruct.ConditionalBlock b2 = new Saxon.Hej.expr.instruct.ConditionalBlock(c2);
            for (int c = 0; (c<size()); c++) {
                b2.adoptChildExpression(c2[c]);
            }
            b2.allNodesUntyped = allNodesUntyped;
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, b2);
            return b2;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((size() == 0)) {
                return Saxon.Hej.type.ErrorType.getInstance();
            }
            Saxon.Hej.type.ItemType t1 = getChildExpression(0).getItemType();
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            for (int i = 1; (i<size()); i++) {
                t1 = Saxon.Hej.type.Type.getCommonSuperType(t1, getChildExpression(i).getItemType(), th);
                if ((t1 is Saxon.Hej.type.AnyItemType)) {
                    return t1;
                }
            }
            return t1;
        }
        public override int getCardinality() {
            if ((size() == 0)) {
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            }
            int c1 = getChildExpression(0).getCardinality();
            for (int i = 1; (i<size()); i++) {
                c1 = Saxon.Hej.value.Cardinality.sum(c1, getChildExpression(i).getCardinality());
                if ((c1 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                    break ;
                }
            }
            return c1;
        }
        public override bool mayCreateNewNodes() {
            return someOperandCreatesNewNodes();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            if (Saxon.Hej.expr.instruct.Block.neverReturnsTypedNodes(this, visitor.getConfiguration().getTypeHierarchy())) {
                resetLocalStaticProperties();
                allNodesUntyped = true;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            int lastOrdinaryInstruction = -1;
            bool alwaysNonEmpty = false;
            bool alwaysEmpty = true;
            for (int c = 0; (c<size()); c++) {
                if (!((((getChildExpression(c) is Saxon.Hej.expr.instruct.OnEmptyExpr) || (getChildExpression(c) is Saxon.Hej.expr.instruct.OnNonEmptyExpr))))) {
                    lastOrdinaryInstruction = c;
                    if (getChildExpression(c).getItemType().getUType().intersection(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.TEXT)).Equals(Saxon.Hej.type.UType.VOID)) {
                        int card = getChildExpression(c).getCardinality();
                        if (!(Saxon.Hej.value.Cardinality.allowsZero(card))) {
                            alwaysNonEmpty = true;
                        }
                        if ((card != Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                            alwaysEmpty = false;
                        }
                    } else {
                        alwaysEmpty = false;
                        alwaysNonEmpty = false;
                        break ;
                    }
                }
            }
            if (alwaysEmpty) {
                visitor.getStaticContext().issueWarning(("The result of the sequence constructor will always be empty, so xsl:on-empty "+"instructions will always be evaluated, and xsl:on-non-empty instructions will never be evaluated"), getLocation());
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> retain = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                for (int c = 0; (c<size()); c++) {
                    if ((getChildExpression(c) is Saxon.Hej.expr.instruct.OnNonEmptyExpr)) {
                    } else {
                        if ((getChildExpression(c) is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                            retain.Add(((Saxon.Hej.expr.instruct.OnEmptyExpr)(getChildExpression(c))).getBaseExpression());
                        } else {
                            retain.Add(getChildExpression(c));
                        }
                    }
                }
                return Saxon.Hej.expr.instruct.Block.makeBlock(retain);
            }
            if (alwaysNonEmpty) {
                visitor.getStaticContext().issueWarning(("The result of the sequence constructor will never be empty, so xsl:on-empty "+"instructions will never be evaluated, and xsl:on-non-empty instructions will always be evaluated"), getLocation());
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> retain = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                for (int c = 0; (c<size()); c++) {
                    if ((getChildExpression(c) is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                    } else {
                        if ((getChildExpression(c) is Saxon.Hej.expr.instruct.OnNonEmptyExpr)) {
                            retain.Add(((Saxon.Hej.expr.instruct.OnNonEmptyExpr)(getChildExpression(c))).getBaseExpression());
                        } else {
                            retain.Add(getChildExpression(c));
                        }
                    }
                }
                return Saxon.Hej.expr.instruct.Block.makeBlock(retain);
            }
            if ((lastOrdinaryInstruction == -1)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> retain = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                for (int c = 0; (c<size()); c++) {
                    if ((getChildExpression(c) is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                        retain.Add(((Saxon.Hej.expr.instruct.OnEmptyExpr)(getChildExpression(c))).getBaseExpression());
                    }
                }
                return Saxon.Hej.expr.instruct.Block.makeBlock(retain);
            }
            return this;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                child.checkPermittedContents(parentType, false);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("condSeq", this);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                child.export(@out);
            }
            @out.endElement();
        }
        public override string toShortString() {
            return (("("+getChildExpression(0).toShortString())+", ...)");
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.OnNonEmptyExpr> onNonEmptyPending = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.OnNonEmptyExpr>();
            Saxon.Hej.om.Action action = () => {
                foreach (Saxon.Hej.expr.Expression e in onNonEmptyPending) {
                    e.process(output, context);
                }
            } ;
            Saxon.Hej.@event.SignificantItemDetector significantItemDetector = new Saxon.Hej.@event.SignificantItemDetector(output, action);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                try {
                    if ((child is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                    } else {
                        if ((child is Saxon.Hej.expr.instruct.OnNonEmptyExpr)) {
                            if (significantItemDetector.isEmpty()) {
                                onNonEmptyPending.Add((Saxon.Hej.expr.instruct.OnNonEmptyExpr)(child));
                            } else {
                                child.process(output, context);
                            }
                        } else {
                            child.process(significantItemDetector, context);
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(child.getLocation());
                    e.maybeSetContext(context);
                    throw;
                }
            }
            if (significantItemDetector.isEmpty()) {
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    Saxon.Hej.expr.Expression child = o.getChildExpression();
                    if ((child is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                        child.process(output, context);
                    }
                }
            }
            return null;
        }
        public override int getImplementationMethod() {
            return PROCESS_METHOD;
        }
        public override string getStreamerName() {
            return "ConditionalBlock";
        }
    
    }
}