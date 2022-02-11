using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class CardinalityChecker : Saxon.Hej.expr.UnaryExpression {
        private int requiredCardinality = -1;
        private readonly Saxon.Hej.expr.parser.RoleDiagnostic role;
        private CardinalityChecker(Saxon.Hej.expr.Expression sequence, int cardinality, Saxon.Hej.expr.parser.RoleDiagnostic role) : base (sequence) {
            requiredCardinality = cardinality;
            this.role = role;
        }
        public static Saxon.Hej.expr.Expression makeCardinalityChecker(Saxon.Hej.expr.Expression sequence, int cardinality, Saxon.Hej.expr.parser.RoleDiagnostic role) {
            Saxon.Hej.expr.Expression result;
            if (((sequence is Saxon.Hej.expr.Literal) && Saxon.Hej.value.Cardinality.subsumes(cardinality, Saxon.Hej.om.SequenceTool.getCardinality(((Saxon.Hej.expr.Literal)(sequence)).getGroundedValue())))) {
                return sequence;
            }
            if (((sequence is Saxon.Hej.expr.Atomizer) && !(Saxon.Hej.value.Cardinality.allowsMany(cardinality)))) {
                Saxon.Hej.expr.Expression @base = ((Saxon.Hej.expr.Atomizer)(sequence)).getBaseExpression();
                result = new Saxon.Hej.expr.SingletonAtomizer(@base, role, Saxon.Hej.value.Cardinality.allowsZero(cardinality));
            } else {
                result = new Saxon.Hej.expr.CardinalityChecker(sequence, cardinality, role);
            }
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(sequence, result);
            return result;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public int getRequiredCardinality() {
            return requiredCardinality;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.Expression @base = getBaseExpression();
            if (((requiredCardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE) || Saxon.Hej.value.Cardinality.subsumes(requiredCardinality, @base.getCardinality()))) {
                return @base;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            Saxon.Hej.expr.Expression @base = getBaseExpression();
            if (((requiredCardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE) || Saxon.Hej.value.Cardinality.subsumes(requiredCardinality, @base.getCardinality()))) {
                return @base;
            }
            if ((((@base.getCardinality()&requiredCardinality)) == 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The "+role.getMessage())+" does not satisfy the cardinality constraints"), role.getErrorCode());
                err.setLocation(getLocation());
                err.setIsTypeError(role.isTypeError());
                throw err;
            }
            if ((@base is Saxon.Hej.expr.ItemChecker)) {
                Saxon.Hej.expr.ItemChecker checker = (Saxon.Hej.expr.ItemChecker)(@base);
                Saxon.Hej.expr.Expression other = checker.getBaseExpression();
                setBaseExpression(other);
                checker.setBaseExpression(this);
                checker.setParentExpression(null);
                return checker;
            }
            return this;
        }
        public void setErrorCode(string code) {
            role.setErrorCode(code);
        }
        public Saxon.Hej.expr.parser.RoleDiagnostic getRoleLocator() {
            return role;
        }
        public override int getImplementationMethod() {
            int m = ((ITERATE_METHOD|PROCESS_METHOD)|ITEM_FEED_METHOD);
            if (!(Saxon.Hej.value.Cardinality.allowsMany(requiredCardinality))) {
                m |= EVALUATE_METHOD;
            }
            return m;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getBaseExpression().getIntegerBounds();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator @base = getBaseExpression().iterate(context);
            if (Saxon.Hej.om.SequenceTool.supportsGetLength(@base)) {
                int count = Saxon.Hej.om.SequenceTool.getLength(@base);
                if (((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(requiredCardinality)))) {
                    typeError(("An empty sequence is not allowed as the "+role.getMessage()), role.getErrorCode(), context);
                } else {
                    if (((count == 1) && (requiredCardinality == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                        typeError((("The only value allowed for the "+role.getMessage())+" is an empty sequence"), role.getErrorCode(), context);
                    } else {
                        if (((count>1) && !(Saxon.Hej.value.Cardinality.allowsMany(requiredCardinality)))) {
                            typeError((("A sequence of more than one item is not allowed as the "+role.getMessage())+depictSequenceStart(@base, 2)), role.getErrorCode(), context);
                        }
                    }
                }
                return @base;
            }
            return new Saxon.Hej.expr.CardinalityCheckingIterator(@base, requiredCardinality, role, getLocation());
        }
        public static string depictSequenceStart(Saxon.Hej.om.SequenceIterator seq, int max) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            int count = 0;
            sb.append(" (");
            Saxon.Hej.om.Item next;
            while (((next = seq.next()) != null)) {
                if ((count++>0)) {
                    sb.append(", ");
                }
                if ((count>max)) {
                    sb.append("...) ");
                    return sb.ToString();
                }
                sb.append(Saxon.Hej.trans.Err.depict(next));
            }
            sb.append(") ");
            return sb.ToString();
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.om.SequenceIterator iter = getBaseExpression().iterate(context);
                Saxon.Hej.om.Item first = iter.next();
                if ((first == null)) {
                    if (!(Saxon.Hej.value.Cardinality.allowsZero(requiredCardinality))) {
                        typeError(("An empty sequence is not allowed as the "+role.getMessage()), role.getErrorCode(), context);
                    }
                    return null;
                } else {
                    if ((requiredCardinality == Saxon.Hej.expr.StaticProperty.EMPTY)) {
                        typeError(("An empty sequence is required as the "+role.getMessage()), role.getErrorCode(), context);
                        return null;
                    }
                    Saxon.Hej.om.Item second = iter.next();
                    if ((second != null)) {
                        typeError((("A sequence of more than one item is not allowed as the "+role.getMessage())+depictSequenceStart(new Saxon.Hej.tree.iter.TwoItemIterator(first, second), 2)), role.getErrorCode(), context);
                        return null;
                    }
                }
                return first;
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression next = getBaseExpression();
            Saxon.Hej.type.ItemType type = Saxon.Hej.type.Type.ITEM_TYPE;
            if ((next is Saxon.Hej.expr.ItemChecker)) {
                type = ((Saxon.Hej.expr.ItemChecker)(next)).getRequiredType();
                next = ((Saxon.Hej.expr.ItemChecker)(next)).getBaseExpression();
            }
            if (((((next.getImplementationMethod()&PROCESS_METHOD)) != 0) && !(((type is Saxon.Hej.pattern.DocumentNodeTest))))) {
                Saxon.Hej.@event.TypeCheckingFilter filter = new Saxon.Hej.@event.TypeCheckingFilter(output);
                filter.setRequiredType(type, requiredCardinality, role, getLocation());
                next.process(filter, context);
                try {
                    filter.finalCheck();
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(getLocation());
                    throw;
                }
            } else {
                base.process(output, context);
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getBaseExpression().getItemType();
        }
        protected internal override int computeCardinality() {
            return requiredCardinality;
        }
        protected internal override int computeSpecialProperties() {
            return getBaseExpression().getSpecialProperties();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.CardinalityChecker c2 = new Saxon.Hej.expr.CardinalityChecker(getBaseExpression().copy(rebindings), requiredCardinality, role);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c2);
            return c2;
        }
        public override bool Equals(System.Object other) {
            return (base.Equals(other) && (requiredCardinality == ((Saxon.Hej.expr.CardinalityChecker)(other)).requiredCardinality));
        }
        protected internal override int computeHashCode() {
            return (base.computeHashCode()^requiredCardinality);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("check", this);
            string occ = Saxon.Hej.value.Cardinality.getOccurrenceIndicator(requiredCardinality);
            if (occ.Equals("")) {
                occ = "1";
            }
            @out.emitAttribute("card", occ);
            @out.emitAttribute("diag", role.save());
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string ToString() {
            Saxon.Hej.expr.Expression operand = getBaseExpression();
            switch (requiredCardinality) {
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE:
                return (("exactly-one("+operand)+")");
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE:
                return (("zero-or-one("+operand)+")");
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE:
                return (("one-or-more("+operand)+")");
              case Saxon.Hej.expr.StaticProperty.EMPTY:
                return (("must-be-empty("+operand)+")");
              default:
                return (("check("+operand)+")");
            }
        }
        public override string getExpressionName() {
            return "CheckCardinality";
        }
        public override string toShortString() {
            return getBaseExpression().toShortString();
        }
        public override string getStreamerName() {
            return "CardinalityChecker";
        }
        public override void setLocation(Saxon.Hej.s9api.Location id) {
            base.setLocation(id);
        }
    
    }
}