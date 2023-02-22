using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class BooleanExpression : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.Negatable {
        public BooleanExpression(Saxon.Hej.expr.Expression p1, int @operator, Saxon.Hej.expr.Expression p2) : base (p1, @operator, p2) {
        }
        public override string getExpressionName() {
            return (Saxon.Hej.expr.parser.Token.tokens[getOperator()]+"-expression");
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.trans.XPathException err0 = Saxon.Hej.expr.parser.TypeChecker.ebvError(getLhsExpression(), th);
            if ((err0 != null)) {
                err0.setLocator(getLocation());
                throw err0;
            }
            Saxon.Hej.trans.XPathException err1 = Saxon.Hej.expr.parser.TypeChecker.ebvError(getRhsExpression(), th);
            if ((err1 != null)) {
                err1.setLocator(getLocation());
                throw err1;
            }
            if (((getLhsExpression() is Saxon.Hej.expr.Literal) && !(((((Saxon.Hej.expr.Literal)(getLhsExpression())).getGroundedValue() is Saxon.Hej.value.BooleanValue))))) {
                setLhsExpression(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(((Saxon.Hej.expr.Literal)(getLhsExpression())).getGroundedValue().effectiveBooleanValue()), this));
            }
            if (((getRhsExpression() is Saxon.Hej.expr.Literal) && !(((((Saxon.Hej.expr.Literal)(getRhsExpression())).getGroundedValue() is Saxon.Hej.value.BooleanValue))))) {
                setRhsExpression(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(((Saxon.Hej.expr.Literal)(getRhsExpression())).getGroundedValue().effectiveBooleanValue()), this));
            }
            return preEvaluate();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            optimizeChildren(visitor, contextItemType);
            bool forStreaming = visitor.isOptimizeForStreaming();
            setLhsExpression(Saxon.Hej.expr.parser.ExpressionTool.unsortedIfHomogeneous(getLhsExpression(), forStreaming));
            setRhsExpression(Saxon.Hej.expr.parser.ExpressionTool.unsortedIfHomogeneous(getRhsExpression(), forStreaming));
            Saxon.Hej.expr.Expression op0 = Saxon.Hej.functions.BooleanFn.rewriteEffectiveBooleanValue(getLhsExpression(), visitor, contextItemType);
            if ((op0 != null)) {
                setLhsExpression(op0);
            }
            Saxon.Hej.expr.Expression op1 = Saxon.Hej.functions.BooleanFn.rewriteEffectiveBooleanValue(getRhsExpression(), visitor, contextItemType);
            if ((op1 != null)) {
                setRhsExpression(op1);
            }
            return preEvaluate();
        }
        protected internal abstract Saxon.Hej.expr.Expression preEvaluate();
        protected internal Saxon.Hej.expr.Expression forceToBoolean(Saxon.Hej.expr.Expression @in) {
            if (((@in.getItemType() == Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) && (@in.getCardinality() == Saxon.Hej.expr.StaticProperty.ALLOWS_ONE))) {
                return @in;
            } else {
                return Saxon.Hej.functions.SystemFunction.makeCall("boolean", getRetainedStaticContext(), @in);
            }
        }
        public bool isNegatable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public abstract Saxon.Hej.expr.Expression negate();
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public abstract override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c);
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        public static void listAndComponents(Saxon.Hej.expr.Expression exp, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if (((exp is Saxon.Hej.expr.BooleanExpression) && (((Saxon.Hej.expr.BooleanExpression)(exp)).getOperator() == Saxon.Hej.expr.parser.Token.AND))) {
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    listAndComponents(o.getChildExpression(), list);
                }
            } else {
                list.Add(exp);
            }
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return Saxon.Hej.expr.OperandRole.INSPECT;
        }
    
    }
}