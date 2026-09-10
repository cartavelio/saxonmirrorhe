using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    #pragma warning disable 659
    public class TupleExpression : Saxon.Hej.expr.Expression {
        private Saxon.Hej.expr.oper.OperandArray operanda;
        public TupleExpression()  {
        }
        protected internal void setOperanda(Saxon.Hej.expr.oper.OperandArray operanda) {
            this.operanda = operanda;
        }
        protected internal Saxon.Hej.expr.oper.OperandArray getOperanda() {
            return operanda;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operanda.operands();
        }
        public void setVariables(System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> refs) {
            Saxon.Hej.expr.Expression[] e = new Saxon.Hej.expr.Expression[refs.Count];
            for (int i = 0; (i<refs.Count); i++) {
                e[i] = refs[i];
            }
            setOperanda(new Saxon.Hej.expr.oper.OperandArray(this, e, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION));
        }
        public int getSize() {
            return getOperanda().getNumberOfOperands();
        }
        public Saxon.Hej.expr.LocalVariableReference getSlot(int i) {
            return (Saxon.Hej.expr.LocalVariableReference)(getOperanda().getOperandExpression(i));
        }
        public void setSlot(int i, Saxon.Hej.expr.LocalVariableReference @ref) {
            getOperanda().setOperand(i, @ref);
        }
        public bool includesBinding(Saxon.Hej.expr.Binding binding) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if ((((Saxon.Hej.expr.LocalVariableReference)(o.getChildExpression())).getBinding() == binding)) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return new Saxon.Hej.type.JavaExternalObjectType(typeof(Saxon.Hej.expr.flwor.Tuple));
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            for (int i = 0; (i<getSize()); i++) {
                operanda.getOperand(i).typeCheck(visitor, contextInfo);
            }
            return this;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.expr.flwor.TupleExpression)))) {
                return false;
            } else {
                Saxon.Hej.expr.flwor.TupleExpression t2 = (Saxon.Hej.expr.flwor.TupleExpression)(other);
                if ((getOperanda().getNumberOfOperands() != t2.getOperanda().getNumberOfOperands())) {
                    return false;
                }
                for (int i = 0; (i<getSize()); i++) {
                    if (!(getSlot(i).isEqual(t2.getSlot(i)))) {
                        return false;
                    }
                }
                return true;
            }
        }
        protected internal override int computeHashCode() {
            int h = 77;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                h ^= o.getChildExpression().GetHashCode();
            }
            return h;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            int n = getOperanda().getNumberOfOperands();
            System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> refs2 = new System.Collections.Generic.List<Saxon.Hej.expr.LocalVariableReference>(n);
            for (int i = 0; (i<n); i++) {
                refs2.Add((Saxon.Hej.expr.LocalVariableReference)(getSlot(i).copy(rebindings)));
            }
            Saxon.Hej.expr.flwor.TupleExpression t2 = new Saxon.Hej.expr.flwor.TupleExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, t2);
            t2.setVariables(refs2);
            return t2;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("tuple", this);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            @out.endElement();
        }
        public override Saxon.Hej.expr.flwor.Tuple evaluateItem(Saxon.Hej.expr.XPathContext context) {
            int n = getSize();
            Saxon.Hej.om.Sequence[] tuple = new Saxon.Hej.om.Sequence[n];
            for (int i = 0; (i<n); i++) {
                tuple[i] = getSlot(i).evaluateVariable(context);
            }
            return new Saxon.Hej.expr.flwor.Tuple(tuple);
        }
        public override string getExpressionName() {
            return "tuple";
        }
        public void setCurrentTuple(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.flwor.Tuple tuple) {
            Saxon.Hej.om.Sequence[] members = tuple.getMembers();
            int n = getSize();
            for (int i = 0; (i<n); i++) {
                context.setLocalVariable(getSlot(i).getBinding().getLocalSlotNumber(), members[i]);
            }
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override int getIntrinsicDependencies() {
            return 0;
        }
    
    }
}