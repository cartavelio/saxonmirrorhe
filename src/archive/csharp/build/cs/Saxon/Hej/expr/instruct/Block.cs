using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class Block : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand[] operanda;
        private bool allNodesUntyped;
        public Block(Saxon.Hej.expr.Expression[] children)  {
            operanda = new Saxon.Hej.expr.Operand[children.Length];
            for (int i = 0; (i<children.Length); i++) {
                operanda[i] = new Saxon.Hej.expr.Operand(this, children[i], Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            }
            foreach (Saxon.Hej.expr.Expression e in children) {
                adoptChildExpression(e);
            }
        }
        public override bool isInstruction() {
            return false;
        }
        private Saxon.Hej.expr.Expression child(int n) {
            return operanda[n].getChildExpression();
        }
        private void setChild(int n, Saxon.Hej.expr.Expression child) {
            operanda[n].setChildExpression(child);
        }
        private int size() {
            return operanda.Length;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return Saxon.Ejava.util.Arrays.asList(operanda);
        }
        public override bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            if ((binding is Saxon.Hej.expr.instruct.LocalParam)) {
                foreach (Saxon.Hej.expr.Operand o in operanda) {
                    if ((o.getChildExpression() == binding)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public static Saxon.Hej.expr.Expression makeBlock(Saxon.Hej.expr.Expression e1, Saxon.Hej.expr.Expression e2) {
            if (((e1 == null) || Saxon.Hej.expr.Literal.isEmptySequence(e1))) {
                return e2;
            }
            if (((e2 == null) || Saxon.Hej.expr.Literal.isEmptySequence(e2))) {
                return e1;
            }
            if (((e1 is Saxon.Hej.expr.instruct.Block) || (e2 is Saxon.Hej.expr.instruct.Block))) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
                if ((e1 is Saxon.Hej.expr.instruct.Block)) {
                    foreach (Saxon.Hej.expr.Operand o in e1.operands()) {
                        list.Add(o.getChildExpression());
                    }
                } else {
                    list.Add(e1);
                }
                if ((e2 is Saxon.Hej.expr.instruct.Block)) {
                    foreach (Saxon.Hej.expr.Operand o in e2.operands()) {
                        list.Add(o.getChildExpression());
                    }
                } else {
                    list.Add(e2);
                }
                Saxon.Hej.expr.Expression[] exps = new Saxon.Hej.expr.Expression[list.Count];
                exps = list.ToArray();
                return new Saxon.Hej.expr.instruct.Block(exps);
            } else {
                Saxon.Hej.expr.Expression[] exps = {e1, e2};
                return new Saxon.Hej.expr.instruct.Block(exps);
            }
        }
        public static Saxon.Hej.expr.Expression makeBlock(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if (list.Count==0) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                if ((list.Count == 1)) {
                    return list[0];
                } else {
                    Saxon.Hej.expr.Expression[] exps = new Saxon.Hej.expr.Expression[list.Count];
                    exps = list.ToArray();
                    return new Saxon.Hej.expr.instruct.Block(exps);
                }
            }
        }
        public override string getExpressionName() {
            return "sequence";
        }
        public Saxon.Hej.expr.Operand[] getOperanda() {
            return operanda;
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
                Saxon.Hej.expr.Expression childExpr = o.getChildExpression();
                if (!(((childExpr is Saxon.Hej.expr.AxisExpression)))) {
                    allAxisExpressions = false;
                    allChildAxis = false;
                    allSubtreeAxis = false;
                    break ;
                }
                int axis = ((Saxon.Hej.expr.AxisExpression)(childExpr)).getAxis();
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
                if ((((size() == 2) && (((Saxon.Hej.expr.AxisExpression)(child(0))).getAxis() == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) && (((Saxon.Hej.expr.AxisExpression)(child(1))).getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                    p |= Saxon.Hej.expr.StaticProperty.ORDERED_NODESET;
                }
            }
            return p;
        }
        public override bool implementsStaticTypeCheck() {
            return true;
        }
        public override Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.value.SequenceType req, bool backwardsCompatible, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(backwardsCompatible);
            if ((backwardsCompatible && !(Saxon.Hej.value.Cardinality.allowsMany(req.getCardinality())))) {
                Saxon.Hej.expr.Expression first = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(this);
                return tc.staticTypeCheck(first, req, role, visitor);
            }
            Saxon.Hej.expr.Expression[] @checked = new Saxon.Hej.expr.Expression[operanda.Length];
            Saxon.Hej.value.SequenceType subReq = req;
            if ((req.getCardinality() != Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                subReq = Saxon.Hej.value.SequenceType.makeSequenceType(req.getPrimaryType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            }
            for (int i = 0; (i<operanda.Length); i++) {
                @checked[i] = tc.staticTypeCheck(operanda[i].getChildExpression(), subReq, role, visitor);
            }
            Saxon.Hej.expr.instruct.Block b2 = new Saxon.Hej.expr.instruct.Block(@checked);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, b2);
            b2.allNodesUntyped = allNodesUntyped;
            int reqCard = req.getCardinality();
            int suppliedCard = b2.getCardinality();
            if (!(Saxon.Hej.value.Cardinality.subsumes(req.getCardinality(), suppliedCard))) {
                if ((((reqCard&suppliedCard)) == 0)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("The required cardinality of the "+role.getMessage())+" is ")+Saxon.Hej.value.Cardinality.describe(reqCard))+", but the supplied cardinality is ")+Saxon.Hej.value.Cardinality.describe(suppliedCard)), role.getErrorCode(), getLocation());
                    err.setIsTypeError(true);
                    err.setFailingExpression(this);
                    throw err;
                } else {
                    return Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(b2, reqCard, role);
                }
            }
            return b2;
        }
        public static bool neverReturnsTypedNodes(Saxon.Hej.expr.instruct.Instruction insn, Saxon.Hej.type.TypeHierarchy th) {
            foreach (Saxon.Hej.expr.Operand o in insn.operands()) {
                Saxon.Hej.expr.Expression exp = o.getChildExpression();
                if (!(exp.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED))) {
                    Saxon.Hej.type.ItemType it = exp.getItemType();
                    if (((th.relationship(it, Saxon.Hej.pattern.NodeKindTest.ELEMENT) != Saxon.Hej.type.Affinity.DISJOINT) || (th.relationship(it, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE) != Saxon.Hej.type.Affinity.DISJOINT))) {
                        return false;
                    }
                }
            }
            return true;
        }
        public Saxon.Hej.expr.Expression mergeAdjacentTextInstructions() {
            bool[] isLiteralText = new bool[size()];
            bool hasAdjacentTextNodes = false;
            for (int i = 0; (i<size()); i++) {
                isLiteralText[i] = (((child(i) is Saxon.Hej.expr.instruct.ValueOf) && (((Saxon.Hej.expr.instruct.ValueOf)(child(i))).getSelect() is Saxon.Hej.expr.StringLiteral)) && !(((Saxon.Hej.expr.instruct.ValueOf)(child(i))).isDisableOutputEscaping()));
                if ((((i>0) && isLiteralText[i]) && isLiteralText[(i-1)])) {
                    hasAdjacentTextNodes = true;
                }
            }
            if (hasAdjacentTextNodes) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> content = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(size());
                string pendingText = null;
                for (int i = 0; (i<size()); i++) {
                    if (isLiteralText[i]) {
                        pendingText = ((((pendingText == null) ? "" : pendingText))+((Saxon.Hej.expr.StringLiteral)(((Saxon.Hej.expr.instruct.ValueOf)(child(i))).getSelect())).getString());
                    } else {
                        if ((pendingText != null)) {
                            Saxon.Hej.expr.instruct.ValueOf inst = new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral(pendingText), false, false);
                            content.Add(inst);
                            pendingText = null;
                        }
                        content.Add(child(i));
                    }
                }
                if ((pendingText != null)) {
                    Saxon.Hej.expr.instruct.ValueOf inst = new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral(pendingText), false, false);
                    content.Add(inst);
                }
                return makeBlock(content);
            } else {
                return this;
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression[] c2 = new Saxon.Hej.expr.Expression[size()];
            for (int c = 0; (c<size()); c++) {
                c2[c] = child(c).copy(rebindings);
            }
            Saxon.Hej.expr.instruct.Block b2 = new Saxon.Hej.expr.instruct.Block(c2);
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
            Saxon.Hej.type.ItemType t1 = null;
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            for (int i = 0; (i<size()); i++) {
                Saxon.Hej.expr.Expression childExpr = child(i);
                if (!(((childExpr is Saxon.Hej.expr.instruct.MessageInstr)))) {
                    Saxon.Hej.type.ItemType t = childExpr.getItemType();
                    t1 = ((t1 == null) ? t : Saxon.Hej.type.Type.getCommonSuperType(t1, t, th));
                    if ((t1 is Saxon.Hej.type.AnyItemType)) {
                        return t1;
                    }
                }
            }
            return ((t1 == null) ? Saxon.Hej.type.ErrorType.getInstance() : t1);
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if (isInstruction()) {
                return base.getStaticUType(contextItemType);
            } else {
                if ((size() == 0)) {
                    return Saxon.Hej.type.UType.VOID;
                }
                Saxon.Hej.type.UType t1 = child(0).getStaticUType(contextItemType);
                for (int i = 1; (i<size()); i++) {
                    t1 = t1.union(child(i).getStaticUType(contextItemType));
                    if ((t1 == Saxon.Hej.type.UType.ANY)) {
                        return t1;
                    }
                }
                return t1;
            }
        }
        protected internal override int computeCardinality() {
            if ((size() == 0)) {
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            }
            int c1 = child(0).getCardinality();
            for (int i = 1; (i<size()); i++) {
                c1 = Saxon.Hej.value.Cardinality.sum(c1, child(i).getCardinality());
                if ((c1 == Saxon.Hej.expr.StaticProperty.ALLOWS_MANY)) {
                    break ;
                }
            }
            return c1;
        }
        public override bool mayCreateNewNodes() {
            return someOperandCreatesNewNodes();
        }
        public override void checkForUpdatingSubexpressions() {
            if ((size()<2)) {
                return ;
            }
            bool updating = false;
            bool nonUpdating = false;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (Saxon.Hej.expr.parser.ExpressionTool.isNotAllowedInUpdatingContext(child)) {
                    if (updating) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("If any subexpression is updating, then all must be updating", "XUST0001");
                        err.setLocation(child.getLocation());
                        throw err;
                    }
                    nonUpdating = true;
                }
                if (child.isUpdatingExpression()) {
                    if (nonUpdating) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("If any subexpression is updating, then all must be updating", "XUST0001");
                        err.setLocation(child.getLocation());
                        throw err;
                    }
                    updating = true;
                }
            }
        }
        public override bool isVacuousExpression() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (!(child.isVacuousExpression())) {
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            bool allAtomic = true;
            bool nested = false;
            for (int c = 0; (c<size()); c++) {
                setChild(c, child(c).simplify());
                if (!(Saxon.Hej.expr.Literal.isAtomic(child(c)))) {
                    allAtomic = false;
                }
                if ((child(c) is Saxon.Hej.expr.instruct.Block)) {
                    nested = true;
                } else {
                    if (Saxon.Hej.expr.Literal.isEmptySequence(child(c))) {
                        nested = true;
                    }
                }
            }
            if ((size() == 1)) {
                Saxon.Hej.expr.Expression e = getOperanda()[0].getChildExpression();
                e.setParentExpression(getParentExpression());
                return e;
            } else {
                if ((size() == 0)) {
                    Saxon.Hej.expr.Expression result = Saxon.Hej.expr.Literal.makeEmptySequence();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, result);
                    result.setParentExpression(getParentExpression());
                    return result;
                } else {
                    if (nested) {
                        System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>((size()*2));
                        flatten(list);
                        Saxon.Hej.expr.Expression[] children = new Saxon.Hej.expr.Expression[list.Count];
                        for (int i = 0; (i<list.Count); i++) {
                            children[i] = list[i];
                        }
                        Saxon.Hej.expr.instruct.Block newBlock = new Saxon.Hej.expr.instruct.Block(children);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newBlock);
                        return newBlock.simplify();
                    } else {
                        if (allAtomic) {
                            Saxon.Hej.value.AtomicValue[] values = new Saxon.Hej.value.AtomicValue[size()];
                            for (int c = 0; (c<size()); c++) {
                                values[c] = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(child(c))).getGroundedValue());
                            }
                            Saxon.Hej.expr.Expression result = Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.value.AtomicValue>(values), this);
                            result.setParentExpression(getParentExpression());
                            return result;
                        } else {
                            return this;
                        }
                    }
                }
            }
        }
        private void flatten(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> targetList) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> currentLiteralList = null;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (Saxon.Hej.expr.Literal.isEmptySequence(child)) {
                } else {
                    if ((child is Saxon.Hej.expr.instruct.Block)) {
                        flushCurrentLiteralList(currentLiteralList, targetList);
                        currentLiteralList = null;
                        ((Saxon.Hej.expr.instruct.Block)(child)).flatten(targetList);
                    } else {
                        if (((child is Saxon.Hej.expr.Literal) && !(((((Saxon.Hej.expr.Literal)(child)).getGroundedValue() is Saxon.Hej.value.IntegerRange))))) {
                            Saxon.Hej.om.SequenceIterator iterator = ((Saxon.Hej.expr.Literal)(child)).getGroundedValue().iterate();
                            if ((currentLiteralList == null)) {
                                currentLiteralList = new System.Collections.Generic.List<Saxon.Hej.om.Item>(10);
                            }
                            Saxon.Hej.om.Item item;
                            while (((item = iterator.next()) != null)) {
                                currentLiteralList.Add(item);
                            }
                        } else {
                            flushCurrentLiteralList(currentLiteralList, targetList);
                            currentLiteralList = null;
                            targetList.Add(child);
                        }
                    }
                }
            }
            flushCurrentLiteralList(currentLiteralList, targetList);
        }
        private void flushCurrentLiteralList(System.Collections.Generic.IList<Saxon.Hej.om.Item> currentLiteralList, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if ((currentLiteralList != null)) {
                Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item> iter = new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentLiteralList);
                Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(iter.materialize(), this);
                list.Add(lit);
            }
        }
        public bool isCandidateForSharedAppend() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression exp = o.getChildExpression();
                if (((exp is Saxon.Hej.expr.VariableReference) || (exp is Saxon.Hej.expr.Literal))) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            if (neverReturnsTypedNodes(this, visitor.getConfiguration().getTypeHierarchy())) {
                resetLocalStaticProperties();
                allNodesUntyped = true;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            optimizeChildren(visitor, contextInfo);
            bool canSimplify = false;
            bool prevLiteral = false;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if ((child is Saxon.Hej.expr.instruct.Block)) {
                    canSimplify = true;
                    break ;
                }
                if (((child is Saxon.Hej.expr.Literal) && !(((((Saxon.Hej.expr.Literal)(child)).getGroundedValue() is Saxon.Hej.value.IntegerRange))))) {
                    if ((prevLiteral || Saxon.Hej.expr.Literal.isEmptySequence(child))) {
                        canSimplify = true;
                        break ;
                    }
                    prevLiteral = true;
                } else {
                    prevLiteral = false;
                }
            }
            if (canSimplify) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>((size()*2));
                flatten(list);
                Saxon.Hej.expr.Expression result = Saxon.Hej.expr.instruct.Block.makeBlock(list);
                result.setRetainedStaticContext(getRetainedStaticContext());
                return result;
            }
            if ((size() == 0)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                if ((size() == 1)) {
                    return child(0);
                } else {
                    return this;
                }
            }
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                child.checkPermittedContents(parentType, false);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("sequence", this);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                child.export(@out);
            }
            @out.endElement();
        }
        public override string toShortString() {
            return (("("+child(0).toShortString())+", ...)");
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.TailCall tc = null;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                try {
                    if ((child is Saxon.Hej.expr.instruct.TailCallReturner)) {
                        tc = ((Saxon.Hej.expr.instruct.TailCallReturner)(child)).processLeavingTail(output, context);
                    } else {
                        child.process(output, context);
                        tc = null;
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(child.getLocation());
                    e.maybeSetContext(context);
                    throw;
                }
            }
            return tc;
        }
        public override int getImplementationMethod() {
            return (ITERATE_METHOD|PROCESS_METHOD);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            if ((size() == 0)) {
                return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
            } else {
                if ((size() == 1)) {
                    return child(0).iterate(context);
                } else {
                    return new Saxon.Hej.expr.instruct.BlockIterator(operanda, context);
                }
            }
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                child.evaluatePendingUpdates(context, pul);
            }
        }
        public override string getStreamerName() {
            return "Block";
        }
    
    }
}