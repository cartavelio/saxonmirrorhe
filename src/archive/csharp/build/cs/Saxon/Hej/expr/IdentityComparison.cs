using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class IdentityComparison : Saxon.Hej.expr.BinaryExpression {
        private bool generateIdEmulation = false;
        public IdentityComparison(Saxon.Hej.expr.Expression p1, int op, Saxon.Hej.expr.Expression p2) : base (p1, op, p2) {
        }
        public void setGenerateIdEmulation(bool flag) {
            generateIdEmulation = flag;
        }
        public bool isGenerateIdEmulation() {
            return generateIdEmulation;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            if (!(generateIdEmulation)) {
                if ((Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression()))) {
                    return Saxon.Hej.expr.Literal.makeEmptySequence();
                }
            }
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role0 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 0);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            setLhsExpression(tc.staticTypeCheck(getLhsExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_NODE, role0, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role1 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 1);
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), Saxon.Hej.value.SequenceType.OPTIONAL_NODE, role1, visitor));
            if ((!(Saxon.Hej.value.Cardinality.allowsZero(getLhsExpression().getCardinality())) && !(Saxon.Hej.value.Cardinality.allowsZero(getRhsExpression().getCardinality())))) {
                generateIdEmulation = false;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression r = base.optimize(visitor, contextItemType);
            if ((r != this)) {
                if (!(generateIdEmulation)) {
                    if ((Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression()))) {
                        return Saxon.Hej.expr.Literal.makeEmptySequence();
                    }
                }
            }
            return r;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return Saxon.Hej.expr.OperandRole.INSPECT;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.IdentityComparison ic = new Saxon.Hej.expr.IdentityComparison(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ic);
            ic.generateIdEmulation = generateIdEmulation;
            return ic;
        }
        protected internal override string tag() {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.IS:
                return "is";
              case Saxon.Hej.expr.parser.Token.PRECEDES:
                return "precedes";
              case Saxon.Hej.expr.parser.Token.FOLLOWS:
                return "follows";
              default:
                return "?";
            }
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo node0 = getNode(getLhsExpression(), context);
            if ((node0 == null)) {
                if (generateIdEmulation) {
                    return Saxon.Hej.value.BooleanValue.get((getNode(getRhsExpression(), context) == null));
                }
                return null;
            }
            Saxon.Hej.om.NodeInfo node1 = getNode(getRhsExpression(), context);
            if ((node1 == null)) {
                if (generateIdEmulation) {
                    return Saxon.Hej.value.BooleanValue.FALSE;
                }
                return null;
            }
            return Saxon.Hej.value.BooleanValue.get(compareIdentity(node0, node1));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo node0 = getNode(getLhsExpression(), context);
            if ((node0 == null)) {
                return (generateIdEmulation && (getNode(getRhsExpression(), context) == null));
            }
            Saxon.Hej.om.NodeInfo node1 = getNode(getRhsExpression(), context);
            return ((node1 != null) && compareIdentity(node0, node1));
        }
        private bool compareIdentity(Saxon.Hej.om.NodeInfo node0, Saxon.Hej.om.NodeInfo node1) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.IS:
                return node0.Equals(node1);
              case Saxon.Hej.expr.parser.Token.PRECEDES:
                return (Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(node0, node1)<0);
              case Saxon.Hej.expr.parser.Token.FOLLOWS:
                return (Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(node0, node1)>0);
              default:
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Unknown node identity test");
            }
        }
        private static Saxon.Hej.om.NodeInfo getNode(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.XPathContext c) {
            return (Saxon.Hej.om.NodeInfo)(exp.evaluateItem(c));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        public override string getExpressionName() {
            return "nodeComparison";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.IdentityComparison.IdentityComparisonElaborator();
        }
        public class IdentityComparisonElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.IdentityComparison exp = (Saxon.Hej.expr.IdentityComparison)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator p0 = exp.getLhsExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator p1 = exp.getRhsExpression().makeElaborator().elaborateForItem();
                bool nullable0 = Saxon.Hej.value.Cardinality.allowsZero(exp.getLhsExpression().getCardinality());
                bool nullable1 = Saxon.Hej.value.Cardinality.allowsZero(exp.getRhsExpression().getCardinality());
                int @operator = exp.getOperator();
                return context => {
                    Saxon.Hej.om.NodeInfo v0 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p0(context));
                    if ((nullable0 && (v0 == null))) {
                        return false;
                    }
                    Saxon.Hej.om.NodeInfo v1 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p1(context));
                    if ((nullable1 && (v1 == null))) {
                        return false;
                    }
                    switch (@operator) {
                      case Saxon.Hej.expr.parser.Token.IS:
                        return v0.Equals(v1);
                      case Saxon.Hej.expr.parser.Token.PRECEDES:
                        return (Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(v0, v1)<0);
                      case Saxon.Hej.expr.parser.Token.FOLLOWS:
                        return (Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(v0, v1)>0);
                      default:
                        throw new Saxon.Ejava.lang.UnsupportedOperationException("Unknown node identity test");
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.IdentityComparison exp = (Saxon.Hej.expr.IdentityComparison)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator p0 = exp.getLhsExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator p1 = exp.getRhsExpression().makeElaborator().elaborateForItem();
                bool nullable0 = Saxon.Hej.value.Cardinality.allowsZero(exp.getLhsExpression().getCardinality());
                bool nullable1 = Saxon.Hej.value.Cardinality.allowsZero(exp.getRhsExpression().getCardinality());
                int @operator = exp.getOperator();
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.IS:
                    if ((nullable0 || nullable1)) {
                        return context => {
                            Saxon.Hej.om.NodeInfo v0 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p0(context));
                            Saxon.Hej.om.NodeInfo v1 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p1(context));
                            if (((v0 == null) || (v1 == null))) {
                                return null;
                            }
                            return Saxon.Hej.value.BooleanValue.get(v0.Equals(v1));
                        } ;
                    } else {
                        return context => Saxon.Hej.value.BooleanValue.get(/*Delegate.eval()*/p0(context).Equals(/*Delegate.eval()*/p1(context)));
                    }
                  case Saxon.Hej.expr.parser.Token.PRECEDES:
                    return context => {
                        Saxon.Hej.om.NodeInfo v0 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p0(context));
                        Saxon.Hej.om.NodeInfo v1 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p1(context));
                        if (((v0 == null) || (v1 == null))) {
                            return null;
                        }
                        return Saxon.Hej.value.BooleanValue.get((Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(v0, v1)<0));
                    } ;
                  case Saxon.Hej.expr.parser.Token.FOLLOWS:
                    return context => {
                        Saxon.Hej.om.NodeInfo v0 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p0(context));
                        Saxon.Hej.om.NodeInfo v1 = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/p1(context));
                        if (((v0 == null) || (v1 == null))) {
                            return null;
                        }
                        return Saxon.Hej.value.BooleanValue.get((Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(v0, v1)>0));
                    } ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
        
        }
    
    }
}