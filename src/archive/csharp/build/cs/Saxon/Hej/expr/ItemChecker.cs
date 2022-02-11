using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class ItemChecker : Saxon.Hej.expr.UnaryExpression {
        private readonly Saxon.Hej.type.ItemType requiredItemType;
        private readonly Saxon.Hej.expr.parser.RoleDiagnostic role;
        public ItemChecker(Saxon.Hej.expr.Expression sequence, Saxon.Hej.type.ItemType itemType, Saxon.Hej.expr.parser.RoleDiagnostic role) : base (sequence) {
            requiredItemType = itemType;
            this.role = role;
        }
        public Saxon.Hej.type.ItemType getRequiredType() {
            return requiredItemType;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public Saxon.Hej.expr.parser.RoleDiagnostic getRoleLocator() {
            return role;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.Expression operand = getBaseExpression().simplify();
            if ((requiredItemType is Saxon.Hej.type.AnyItemType)) {
                return operand;
            }
            setBaseExpression(operand);
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            if ((operand is Saxon.Hej.expr.instruct.Block)) {
                Saxon.Hej.expr.instruct.Block block = (Saxon.Hej.expr.instruct.Block)(operand);
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> checkedOperands = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                foreach (Saxon.Hej.expr.Operand o in block.operands()) {
                    Saxon.Hej.expr.ItemChecker checkedOp = new Saxon.Hej.expr.ItemChecker(o.getChildExpression(), requiredItemType, role);
                    checkedOperands.Add(checkedOp);
                }
                Saxon.Hej.expr.instruct.Block newBlock = new Saxon.Hej.expr.instruct.Block(checkedOperands.ToArray());
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newBlock);
                return newBlock.typeCheck(visitor, contextInfo);
            }
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            int card = operand.getCardinality();
            if ((card == Saxon.Hej.expr.StaticProperty.EMPTY)) {
                return operand;
            }
            Saxon.Hej.type.ItemType supplied = operand.getItemType();
            Saxon.Hej.type.Affinity relation = th.relationship(requiredItemType, supplied);
            if (((relation == Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == Saxon.Hej.type.Affinity.SUBSUMES))) {
                return operand;
            } else {
                if ((relation == Saxon.Hej.type.Affinity.DISJOINT)) {
                    if ((requiredItemType.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING) && th.isSubType(supplied, Saxon.Hej.type.BuiltInAtomicType.ANY_URI))) {
                        return operand;
                    } else {
                        if (Saxon.Hej.value.Cardinality.allowsZero(card)) {
                            if (!(((operand is Saxon.Hej.expr.Literal)))) {
                                string message = role.composeErrorMessage(requiredItemType, operand, th);
                                visitor.getStaticContext().issueWarning(("The only value that can pass type-checking is an empty sequence. "+message), getLocation());
                            }
                        } else {
                            string message = role.composeErrorMessage(requiredItemType, operand, th);
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                            err.setErrorCode(role.getErrorCode());
                            err.setLocation(this.getLocation());
                            err.setIsTypeError(role.isTypeError());
                            throw err;
                        }
                    }
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.Affinity rel = th.relationship(requiredItemType, getBaseExpression().getItemType());
            if (((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMES))) {
                return getBaseExpression();
            }
            return this;
        }
        public override int getImplementationMethod() {
            int m = ((ITERATE_METHOD|PROCESS_METHOD)|ITEM_FEED_METHOD);
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getCardinality()))) {
                m |= EVALUATE_METHOD;
            }
            return m;
        }
        public override string getStreamerName() {
            return "ItemChecker";
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getBaseExpression().getIntegerBounds();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression baseExpr = getBaseExpression();
            Saxon.Hej.om.SequenceIterator @base = baseExpr.iterate(context);
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            Saxon.Ejava.util.function.Consumer<Saxon.Hej.om.Item> checker = item => {
                if (requiredItemType.matches(item, th)) {
                } else {
                    if ((requiredItemType.getUType().subsumes(Saxon.Hej.type.UType.STRING) && Saxon.Hej.type.BuiltInAtomicType.ANY_URI.matches(item, th))) {
                    } else {
                        string message = role.composeErrorMessage(requiredItemType, item, th);
                        string errorCode = role.getErrorCode();
                        Saxon.Hej.trans.XPathException te = new Saxon.Hej.trans.XPathException(message, errorCode);
                        te.setFailingExpression(baseExpr);
                        te.setLocator(baseExpr.getLocation());
                        te.setIsTypeError(!("XPDY0050".Equals(errorCode)));
                        throw new Saxon.Hej.trans.UncheckedXPathException(te);
                    }
                }
            } ;
            return new Saxon.Hej.expr.ItemCheckingIterator(@base, checker);
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            Saxon.Hej.om.Item item = getBaseExpression().evaluateItem(context);
            if ((item == null)) {
                return null;
            }
            if (requiredItemType.matches(item, th)) {
                return item;
            } else {
                if ((requiredItemType.getUType().subsumes(Saxon.Hej.type.UType.STRING) && ((Saxon.Hej.type.BuiltInAtomicType.ANY_URI.matches(item, th) || Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC.matches(item, th))))) {
                    return item;
                } else {
                    string message = role.composeErrorMessage(requiredItemType, item, th);
                    string errorCode = role.getErrorCode();
                    if ("XPDY0050".Equals(errorCode)) {
                        dynamicError(message, errorCode, context);
                    } else {
                        typeError(message, errorCode, context);
                    }
                    return null;
                }
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression next = getBaseExpression();
            int card = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            if ((next is Saxon.Hej.expr.CardinalityChecker)) {
                card = ((Saxon.Hej.expr.CardinalityChecker)(next)).getRequiredCardinality();
                next = ((Saxon.Hej.expr.CardinalityChecker)(next)).getBaseExpression();
            }
            if (((((next.getImplementationMethod()&PROCESS_METHOD)) != 0) && !(((requiredItemType is Saxon.Hej.pattern.DocumentNodeTest))))) {
                Saxon.Hej.@event.TypeCheckingFilter filter = new Saxon.Hej.@event.TypeCheckingFilter(output);
                filter.setRequiredType(requiredItemType, card, role, getLocation());
                next.process(filter, context);
                filter.finalCheck();
            } else {
                base.process(output, context);
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ItemChecker exp = new Saxon.Hej.expr.ItemChecker(getBaseExpression().copy(rebindings), requiredItemType, role);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType operandType = getBaseExpression().getItemType();
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.Affinity relationship = th.relationship(requiredItemType, operandType);
            switch (relationship) {
              case Saxon.Hej.type.Affinity.OVERLAPS:
                if (((requiredItemType is Saxon.Hej.pattern.NodeTest) && (operandType is Saxon.Hej.pattern.NodeTest))) {
                    return new Saxon.Hej.pattern.CombinedNodeTest((Saxon.Hej.pattern.NodeTest)(requiredItemType), Saxon.Hej.expr.parser.Token.INTERSECT, (Saxon.Hej.pattern.NodeTest)(operandType));
                } else {
                    return requiredItemType;
                }
              case Saxon.Hej.type.Affinity.SUBSUMES:
              case Saxon.Hej.type.Affinity.SAME_TYPE:
                return operandType;
              case Saxon.Hej.type.Affinity.SUBSUMED_BY:
              default:
                return requiredItemType;
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.fromTypeCode(requiredItemType.getPrimitiveType());
        }
        public override bool Equals(System.Object other) {
            return (base.Equals(other) && (requiredItemType == ((Saxon.Hej.expr.ItemChecker)(other)).requiredItemType));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^requiredItemType.GetHashCode());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("treat", this);
            @out.emitAttribute("as", Saxon.Hej.type.AlphaCode.fromItemType(requiredItemType));
            @out.emitAttribute("diag", role.save());
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string getExpressionName() {
            return "treatAs";
        }
        public override string ToString() {
            string typeDesc = requiredItemType.ToString();
            return ((("("+getBaseExpression())+") treat as ")+typeDesc);
        }
        public override string toShortString() {
            return getBaseExpression().toShortString();
        }
    
    }
}