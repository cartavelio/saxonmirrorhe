using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class IsLastExpression : Saxon.Hej.expr.Expression {
        private readonly bool condition;
        public IsLastExpression(bool condition)  {
            this.condition = condition;
        }
        public bool getCondition() {
            return condition;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext c) {
            return Saxon.Hej.value.BooleanValue.get((condition == c.isAtLast()));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return (condition == context.isAtLast());
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override int getIntrinsicDependencies() {
            return (Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.IsLastExpression exp = new Saxon.Hej.expr.IsLastExpression(condition);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.expr.IsLastExpression) && (((Saxon.Hej.expr.IsLastExpression)(other)).condition == condition));
        }
        protected internal override int computeHashCode() {
            return (condition ? 0x236b91a0 : 0x396b92a0);
        }
        public override string getExpressionName() {
            return "isLast";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("isLast", this);
            destination.emitAttribute("test", (condition ? "1" : "0"));
            destination.endElement();
        }
        public override string ToString() {
            if (condition) {
                return "position() eq last()";
            } else {
                return "position() ne last()";
            }
        }
        public override string getStreamerName() {
            return "IsLastExpr";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.IsLastExpression.IsLastElaborator();
        }
        public class IsLastElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.IsLastExpression expr = (Saxon.Hej.expr.IsLastExpression)(getExpression());
                if (expr.getCondition()) {
                    return x => x.isAtLast() /* method reference */;
                } else {
                    return context => !(context.isAtLast());
                }
            }
        
        }
    
    }
}