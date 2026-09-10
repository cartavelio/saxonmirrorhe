using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    #pragma warning disable 659
    public class SquareArrayConstructor : Saxon.Hej.expr.Expression, Saxon.Hej.expr.elab.Pingable {
        private Saxon.Hej.expr.oper.OperandArray operanda;
        private double numberOfCalls = 0;
        private double numberOfConversions = 0;
        public SquareArrayConstructor(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children)  {
            Saxon.Hej.expr.Expression[] kids = children.ToArray();
            foreach (Saxon.Hej.expr.Expression e in children) {
                adoptChildExpression(e);
            }
            setOperanda(new Saxon.Hej.expr.oper.OperandArray(this, kids, Saxon.Hej.expr.OperandRole.NAVIGATE));
        }
        protected internal void setOperanda(Saxon.Hej.expr.oper.OperandArray operanda) {
            this.operanda = operanda;
        }
        public Saxon.Hej.expr.oper.OperandArray getOperanda() {
            return operanda;
        }
        public Saxon.Hej.expr.Operand getOperand(int i) {
            return operanda.getOperand(i);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operanda;
        }
        public override string getExpressionName() {
            return "SquareArrayConstructor";
        }
        public override string getStreamerName() {
            return "ArrayBlock";
        }
        protected internal override int computeSpecialProperties() {
            return 0;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.ma.arrays.SquareArrayConstructor)))) {
                return false;
            } else {
                Saxon.Hej.ma.arrays.SquareArrayConstructor ab2 = (Saxon.Hej.ma.arrays.SquareArrayConstructor)(other);
                if ((ab2.getOperanda().getNumberOfOperands() != getOperanda().getNumberOfOperands())) {
                    return false;
                }
                for (int i = 0; (i<getOperanda().getNumberOfOperands()); i++) {
                    if (!(getOperanda().getOperand(i).Equals(ab2.getOperanda().getOperand(i)))) {
                        return false;
                    }
                }
                return true;
            }
        }
        protected internal override int computeHashCode() {
            int h = 0x778b92a0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                h ^= o.getChildExpression().GetHashCode();
            }
            return h;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.typeCheck(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            return preEvaluate(visitor);
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextInfo);
            if ((e != this)) {
                return e;
            }
            return preEvaluate(visitor);
        }
        private Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if (!(((o.getChildExpression() is Saxon.Hej.expr.Literal)))) {
                    return this;
                }
            }
            try {
                return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(visitor.makeDynamicContext()), this);
            } catch (Saxon.Hej.trans.XPathException e) {
                return this;
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> m2 = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(getOperanda().getNumberOfOperands());
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                m2.Add(o.getChildExpression().copy(rebindings));
            }
            Saxon.Hej.ma.arrays.SquareArrayConstructor b2 = new Saxon.Hej.ma.arrays.SquareArrayConstructor(m2);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, b2);
            return b2;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType contentType = null;
            int contentCardinality = Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            foreach (Saxon.Hej.expr.Expression e in getOperanda().operandExpressions()) {
                if ((contentType == null)) {
                    contentType = e.getItemType();
                    contentCardinality = e.getCardinality();
                } else {
                    contentType = Saxon.Hej.type.Type.getCommonSuperType(contentType, e.getItemType(), th);
                    contentCardinality = Saxon.Hej.value.Cardinality.union(contentCardinality, e.getCardinality());
                }
            }
            if ((contentType == null)) {
                contentType = Saxon.Hej.type.ErrorType.getInstance();
            }
            return new Saxon.Hej.ma.arrays.ArrayItemType(Saxon.Hej.value.SequenceType.makeSequenceType(contentType, contentCardinality));
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.FUNCTION;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("arrayBlock", this);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            @out.endElement();
        }
        public override string toShortString() {
            int n = getOperanda().getNumberOfOperands();
            switch (n) {
              case 0:
                return "[]";
              case 1:
                return (("["+getOperanda().getOperand(0).getChildExpression().toShortString())+"]");
              case 2:
                return (((("["+getOperanda().getOperand(0).getChildExpression().toShortString())+", ")+getOperanda().getOperand(1).getChildExpression().toShortString())+"]");
              default:
                return (("["+getOperanda().getOperand(0).getChildExpression().toShortString())+", ...]");
            }
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public void ping() {
            numberOfConversions++;
        }
        protected internal Saxon.Hej.ma.arrays.ArrayItem makeArray(System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members) {
            if ((numberOfConversions>(numberOfCalls*0.5))) {
                return new Saxon.Impl.DataModel.ImmArray(members);
            } else {
                numberOfCalls++;
                Saxon.Hej.ma.arrays.SimpleArrayItem result = new Saxon.Hej.ma.arrays.SimpleArrayItem(members);
                result.requestNotification(this);
                return result;
            }
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> value = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(getOperanda().getNumberOfOperands());
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.om.GroundedValue s = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(o.getChildExpression(), context);
                value.Add(s);
            }
            return makeArray(value);
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.ma.arrays.SquareArrayConstructor.SquareArrayConstructorElaborator();
        }
        private class SquareArrayConstructorElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.ma.arrays.SquareArrayConstructor expr = (Saxon.Hej.ma.arrays.SquareArrayConstructor)(getExpression());
                System.Collections.Generic.IList<Saxon.Hej.expr.elab.SequenceEvaluator> eagerEvaluators = new System.Collections.Generic.List<Saxon.Hej.expr.elab.SequenceEvaluator>(expr.getOperanda().getNumberOfOperands());
                foreach (Saxon.Hej.expr.Operand o in expr.operands()) {
                    eagerEvaluators.Add(o.getChildExpression().makeElaborator().eagerly());
                }
                return context => {
                    System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> members = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(eagerEvaluators.Count);
                    foreach (Saxon.Hej.expr.elab.SequenceEvaluator e in eagerEvaluators) {
                        members.Add((Saxon.Hej.om.GroundedValue)(e.evaluate(context)));
                    }
                    return expr.makeArray(members);
                } ;
            }
        
        }
    
    }
}