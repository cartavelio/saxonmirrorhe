using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class VennExpression : Saxon.Hej.expr.BinaryExpression {
        public VennExpression(Saxon.Hej.expr.Expression p1, int op, Saxon.Hej.expr.Expression p2) : base (p1, op, p2) {
        }
        public override Saxon.Hej.expr.Expression simplify() {
            if (!(((getLhsExpression() is Saxon.Hej.expr.sort.DocumentSorter)))) {
                setLhsExpression(new Saxon.Hej.expr.sort.DocumentSorter(getLhsExpression()));
            }
            if (!(((getRhsExpression() is Saxon.Hej.expr.sort.DocumentSorter)))) {
                setRhsExpression(new Saxon.Hej.expr.sort.DocumentSorter(getRhsExpression()));
            }
            base.simplify();
            return this;
        }
        public override string getExpressionName() {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return "union";
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return "intersect";
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return "except";
              default:
                return "unknown";
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType t1 = getLhsExpression().getItemType();
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                Saxon.Hej.type.ItemType t2 = getRhsExpression().getItemType();
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                return Saxon.Hej.type.Type.getCommonSuperType(t1, t2, th);
            } else {
                return t1;
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return getLhsExpression().getStaticUType(contextItemType).union(getRhsExpression().getStaticUType(contextItemType));
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return getLhsExpression().getStaticUType(contextItemType).intersection(getRhsExpression().getStaticUType(contextItemType));
              case Saxon.Hej.expr.parser.Token.EXCEPT:
              default:
                return getLhsExpression().getStaticUType(contextItemType);
            }
        }
        protected internal override int computeCardinality() {
            int c1 = getLhsExpression().getCardinality();
            int c2 = getRhsExpression().getCardinality();
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                if (Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression())) {
                    return c2;
                }
                if (Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression())) {
                    return c1;
                }
                return (((c1|c2)|Saxon.Hej.expr.StaticProperty.ALLOWS_ONE)|Saxon.Hej.expr.StaticProperty.ALLOWS_MANY);
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                if (Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression())) {
                    return Saxon.Hej.expr.StaticProperty.EMPTY;
                }
                if (Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression())) {
                    return Saxon.Hej.expr.StaticProperty.EMPTY;
                }
                return ((((c1&c2))|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)|Saxon.Hej.expr.StaticProperty.ALLOWS_ONE);
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                if (Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression())) {
                    return Saxon.Hej.expr.StaticProperty.EMPTY;
                }
                if (Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression())) {
                    return c1;
                }
                return ((c1|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)|Saxon.Hej.expr.StaticProperty.ALLOWS_ONE);
            }
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        protected internal override int computeSpecialProperties() {
            int prop0 = getLhsExpression().getSpecialProperties();
            int prop1 = getRhsExpression().getSpecialProperties();
            int props = Saxon.Hej.expr.StaticProperty.ORDERED_NODESET;
            if (testContextDocumentNodeSet(prop0, prop1)) {
                props |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            if (testSubTree(prop0, prop1)) {
                props |= Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET;
            }
            if (createsNoNewNodes(prop0, prop1)) {
                props |= Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
            }
            return props;
        }
        private bool testContextDocumentNodeSet(int prop0, int prop1) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return ((((prop0&prop1)&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0);
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return (((((prop0|prop1))&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0);
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return (((prop0&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0);
            }
            return false;
        }
        public void gatherComponents(int @operator, System.Collections.Generic.ISet<Saxon.Hej.expr.Expression> set) {
            if (((getLhsExpression() is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(getLhsExpression())).@operator == @operator))) {
                ((Saxon.Hej.expr.VennExpression)(getLhsExpression())).gatherComponents(@operator, set);
            } else {
                set.Add(getLhsExpression());
            }
            if (((getRhsExpression() is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(getRhsExpression())).@operator == @operator))) {
                ((Saxon.Hej.expr.VennExpression)(getRhsExpression())).gatherComponents(@operator, set);
            } else {
                set.Add(getRhsExpression());
            }
        }
        private bool testSubTree(int prop0, int prop1) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return ((((prop0&prop1)&Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET)) != 0);
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return (((((prop0|prop1))&Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET)) != 0);
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return (((prop0&Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET)) != 0);
            }
            return false;
        }
        private bool createsNoNewNodes(int prop0, int prop1) {
            return ((((prop0&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) != 0) && (((prop1&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) != 0));
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            if (!(((getLhsExpression() is Saxon.Hej.pattern.Pattern)))) {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role0 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 0);
                setLhsExpression(tc.staticTypeCheck(getLhsExpression(), Saxon.Hej.value.SequenceType.NODE_SEQUENCE, role0, visitor));
            }
            if (!(((getRhsExpression() is Saxon.Hej.pattern.Pattern)))) {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role1 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 1);
                setRhsExpression(tc.staticTypeCheck(getRhsExpression(), Saxon.Hej.value.SequenceType.NODE_SEQUENCE, role1, visitor));
            }
            if ((@operator != Saxon.Hej.expr.parser.Token.UNION)) {
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.type.ItemType t0 = getLhsExpression().getItemType();
                Saxon.Hej.type.ItemType t1 = getRhsExpression().getItemType();
                if ((th.relationship(t0, t1) == Saxon.Hej.type.Affinity.DISJOINT)) {
                    if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                        return Saxon.Hej.expr.Literal.makeEmptySequence();
                    } else {
                        if (getLhsExpression().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) {
                            return getLhsExpression();
                        } else {
                            return new Saxon.Hej.expr.sort.DocumentSorter(getLhsExpression());
                        }
                    }
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression e = base.optimize(visitor, contextItemType);
            if ((e != this)) {
                return e;
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.Expression lhs = getLhsExpression();
            Saxon.Hej.expr.Expression rhs = getRhsExpression();
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                if ((Saxon.Hej.expr.Literal.isEmptySequence(lhs) && (((rhs.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0))) {
                    return rhs;
                }
                if ((Saxon.Hej.expr.Literal.isEmptySequence(rhs) && (((lhs.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0))) {
                    return lhs;
                }
                if (contextItemWithCurrentGroup(lhs, rhs)) {
                    return rhs;
                }
                if (contextItemWithCurrentGroup(rhs, lhs)) {
                    return lhs;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                if (Saxon.Hej.expr.Literal.isEmptySequence(lhs)) {
                    return lhs;
                }
                if (Saxon.Hej.expr.Literal.isEmptySequence(rhs)) {
                    return rhs;
                }
                if (contextItemWithCurrentGroup(lhs, rhs)) {
                    return lhs;
                }
                if (contextItemWithCurrentGroup(rhs, lhs)) {
                    return rhs;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                if (Saxon.Hej.expr.Literal.isEmptySequence(lhs)) {
                    return lhs;
                }
                if ((Saxon.Hej.expr.Literal.isEmptySequence(rhs) && (((lhs.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0))) {
                    return lhs;
                }
                if (contextItemWithCurrentGroup(lhs, rhs)) {
                    return Saxon.Hej.expr.Literal.makeEmptySequence();
                }
                if (contextItemWithCurrentGroup(rhs, lhs)) {
                    return new Saxon.Hej.expr.TailExpression(lhs, 2);
                }
                break ;
            }
            if (((lhs is Saxon.Hej.expr.AxisExpression) && (rhs is Saxon.Hej.expr.AxisExpression))) {
                Saxon.Hej.expr.AxisExpression a1 = (Saxon.Hej.expr.AxisExpression)(lhs);
                Saxon.Hej.expr.AxisExpression a2 = (Saxon.Hej.expr.AxisExpression)(rhs);
                if ((a1.getAxis() == a2.getAxis())) {
                    if (a1.getNodeTest().Equals(a2.getNodeTest())) {
                        if ((@operator == Saxon.Hej.expr.parser.Token.EXCEPT)) {
                            return Saxon.Hej.expr.Literal.makeEmptySequence();
                        } else {
                            return a1;
                        }
                    } else {
                        Saxon.Hej.expr.AxisExpression ax = new Saxon.Hej.expr.AxisExpression(a1.getAxis(), new Saxon.Hej.pattern.CombinedNodeTest(a1.getNodeTest(), @operator, a2.getNodeTest()));
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ax);
                        return ax;
                    }
                }
            }
            if ((((lhs is Saxon.Hej.expr.SlashExpression) && (rhs is Saxon.Hej.expr.SlashExpression)) && (@operator == Saxon.Hej.expr.parser.Token.UNION))) {
                Saxon.Hej.expr.SlashExpression path1 = (Saxon.Hej.expr.SlashExpression)(lhs);
                Saxon.Hej.expr.SlashExpression path2 = (Saxon.Hej.expr.SlashExpression)(rhs);
                if (path1.getFirstStep().isEqual(path2.getFirstStep())) {
                    Saxon.Hej.expr.VennExpression venn = new Saxon.Hej.expr.VennExpression(path1.getRemainingSteps(), @operator, path2.getRemainingSteps());
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, venn);
                    Saxon.Hej.expr.Expression path = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(path1.getFirstStep(), venn);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, path);
                    return path.optimize(visitor, contextItemType);
                }
            }
            if (((lhs is Saxon.Hej.expr.FilterExpression) && (rhs is Saxon.Hej.expr.FilterExpression))) {
                Saxon.Hej.expr.FilterExpression exp0 = (Saxon.Hej.expr.FilterExpression)(lhs);
                Saxon.Hej.expr.FilterExpression exp1 = (Saxon.Hej.expr.FilterExpression)(rhs);
                if (((!(exp0.isPositional(th)) && !(exp1.isPositional(th))) && exp0.getSelectExpression().isEqual(exp1.getSelectExpression()))) {
                    Saxon.Hej.expr.Expression filter;
                    switch (@operator) {
                      case Saxon.Hej.expr.parser.Token.UNION:
                        filter = new Saxon.Hej.expr.OrExpression(exp0.getFilter(), exp1.getFilter());
                        break ;
                      case Saxon.Hej.expr.parser.Token.INTERSECT:
                        filter = new Saxon.Hej.expr.AndExpression(exp0.getFilter(), exp1.getFilter());
                        break ;
                      case Saxon.Hej.expr.parser.Token.EXCEPT:
                        Saxon.Hej.expr.Expression negate2 = Saxon.Hej.functions.SystemFunction.makeCall("not", getRetainedStaticContext(), exp1.getFilter());
                        filter = new Saxon.Hej.expr.AndExpression(exp0.getFilter(), negate2);
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.AssertionError(("Unknown operator "+@operator));
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, filter);
                    Saxon.Hej.expr.FilterExpression f = new Saxon.Hej.expr.FilterExpression(exp0.getSelectExpression(), filter);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f);
                    return f.simplify().typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
                }
            }
            if ((((!(visitor.isOptimizeForStreaming()) && (@operator == Saxon.Hej.expr.parser.Token.UNION)) && (lhs is Saxon.Hej.expr.AxisExpression)) && (rhs is Saxon.Hej.expr.AxisExpression))) {
                Saxon.Hej.expr.AxisExpression a0 = (Saxon.Hej.expr.AxisExpression)(lhs);
                Saxon.Hej.expr.AxisExpression a1 = (Saxon.Hej.expr.AxisExpression)(rhs);
                if (((a0.getAxis() == Saxon.Hej.om.AxisInfo.ATTRIBUTE) && (a1.getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                    return new Saxon.Hej.expr.instruct.Block(new Saxon.Hej.expr.Expression[]{lhs, rhs});
                } else {
                    if (((a1.getAxis() == Saxon.Hej.om.AxisInfo.ATTRIBUTE) && (a0.getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                        return new Saxon.Hej.expr.instruct.Block(new Saxon.Hej.expr.Expression[]{rhs, lhs});
                    }
                }
            }
            if (((@operator == Saxon.Hej.expr.parser.Token.INTERSECT) && !(Saxon.Hej.value.Cardinality.allowsMany(lhs.getCardinality())))) {
                return new Saxon.Hej.expr.SingletonIntersectExpression(lhs, @operator, rhs.unordered(false, false));
            }
            if (((@operator == Saxon.Hej.expr.parser.Token.INTERSECT) && !(Saxon.Hej.value.Cardinality.allowsMany(rhs.getCardinality())))) {
                return new Saxon.Hej.expr.SingletonIntersectExpression(rhs, @operator, lhs.unordered(false, false));
            }
            if (operandsAreDisjoint(th)) {
                if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                    return Saxon.Hej.expr.Literal.makeEmptySequence();
                } else {
                    if ((@operator == Saxon.Hej.expr.parser.Token.EXCEPT)) {
                        if ((((lhs.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0)) {
                            return lhs;
                        } else {
                            return new Saxon.Hej.expr.sort.DocumentSorter(lhs);
                        }
                    }
                }
            }
            return this;
        }
        private bool operandsAreDisjoint(Saxon.Hej.type.TypeHierarchy th) {
            return (th.relationship(getLhsExpression().getItemType(), getRhsExpression().getItemType()) == Saxon.Hej.type.Affinity.DISJOINT);
        }
        private bool contextItemWithCurrentGroup(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs) {
            if (((lhs is Saxon.Hej.expr.ContextItemExpression) && (rhs is Saxon.Hej.functions.CurrentGroupCall))) {
                Saxon.Hej.expr.Expression focusSetter = Saxon.Hej.expr.parser.ExpressionTool.getFocusSettingContainer(lhs);
                Saxon.Hej.expr.Expression forEachGroup = ((Saxon.Hej.functions.CurrentGroupCall)(rhs)).getControllingInstruction();
                return ((forEachGroup != null) && (focusSetter == forEachGroup));
            }
            return false;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            if ((((@operator == Saxon.Hej.expr.parser.Token.UNION) && !(forStreaming)) && operandsAreDisjoint(getConfiguration().getTypeHierarchy()))) {
                Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(new Saxon.Hej.expr.Expression[]{getLhsExpression(), getRhsExpression()});
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, block);
                return block;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.VennExpression exp = new Saxon.Hej.expr.VennExpression(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.expr.VennExpression)) {
                Saxon.Hej.expr.VennExpression b = (Saxon.Hej.expr.VennExpression)(other);
                if ((@operator != b.@operator)) {
                    return false;
                }
                if ((getLhsExpression().isEqual(b.getLhsExpression()) && getRhsExpression().isEqual(b.getRhsExpression()))) {
                    return true;
                }
                if (((@operator == Saxon.Hej.expr.parser.Token.UNION) || (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                    System.Collections.Generic.ISet<Saxon.Hej.expr.Expression> s0 = new System.Collections.Generic.HashSet<Saxon.Hej.expr.Expression>(10);
                    gatherComponents(@operator, s0);
                    System.Collections.Generic.ISet<Saxon.Hej.expr.Expression> s1 = new System.Collections.Generic.HashSet<Saxon.Hej.expr.Expression>(10);
                    ((Saxon.Hej.expr.VennExpression)(other)).gatherComponents(@operator, s1);
                    return s0.Equals(s1);
                }
            }
            return false;
        }
        protected internal override int computeHashCode() {
            return (getLhsExpression().GetHashCode()^getRhsExpression().GetHashCode());
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            if ((isPredicatePattern(getLhsExpression()) || isPredicatePattern(getRhsExpression()))) {
                throw new Saxon.Hej.trans.XPathException("Cannot use a predicate pattern as an operand of a union, intersect, or except operator", "XTSE0340");
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                return new Saxon.Hej.pattern.UnionPattern(getLhsExpression().toPattern(config), getRhsExpression().toPattern(config));
            } else {
                int commonAxis = Saxon.Hej.expr.parser.ExpressionTool.getAxisNavigation(this);
                if (((((commonAxis == Saxon.Hej.om.AxisInfo.CHILD) || (commonAxis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) || independentOfContextItem(getLhsExpression())) || independentOfContextItem(getRhsExpression()))) {
                    if ((@operator == Saxon.Hej.expr.parser.Token.EXCEPT)) {
                        return new Saxon.Hej.pattern.ExceptPattern(getLhsExpression().toPattern(config), getRhsExpression().toPattern(config));
                    } else {
                        return new Saxon.Hej.pattern.IntersectPattern(getLhsExpression().toPattern(config), getRhsExpression().toPattern(config));
                    }
                }
                return new Saxon.Hej.pattern.GeneralNodePattern(this, (Saxon.Hej.pattern.NodeTest)(getItemType()));
            }
        }
        private bool independentOfContextItem(Saxon.Hej.expr.Expression exp) {
            return (((exp.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM)) == 0);
        }
        private bool isPredicatePattern(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.ItemChecker)) {
                exp = ((Saxon.Hej.expr.ItemChecker)(exp)).getBaseExpression();
            }
            return ((exp is Saxon.Hej.expr.FilterExpression) && ((((Saxon.Hej.expr.FilterExpression)(exp)).getSelectExpression() is Saxon.Hej.expr.ContextItemExpression)));
        }
        protected internal override string tag() {
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                return "union";
            }
            return Saxon.Hej.expr.parser.Token.tokens[@operator];
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                {
                    System.Collections.Generic.IList<Saxon.Hej.om.SequenceIterator> operands = new System.Collections.Generic.List<Saxon.Hej.om.SequenceIterator>();
                    gatherUnionLeafIterators(operands, c);
                    return new Saxon.Hej.expr.UnionIterator(operands, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                }
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                {
                    Saxon.Hej.om.SequenceIterator i1 = getLhsExpression().iterate(c);
                    Saxon.Hej.om.SequenceIterator i2 = getRhsExpression().iterate(c);
                    return new Saxon.Hej.expr.IntersectionIterator(i1, i2, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                }
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                {
                    Saxon.Hej.om.SequenceIterator i1 = getLhsExpression().iterate(c);
                    Saxon.Hej.om.SequenceIterator i2 = getRhsExpression().iterate(c);
                    return new Saxon.Hej.expr.DifferenceIterator(i1, i2, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                }
            }
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Unknown operator in Venn Expression");
        }
        private void gatherUnionLeafIterators(System.Collections.Generic.IList<Saxon.Hej.om.SequenceIterator> leafIterators, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression e1 = getLhsExpression();
            if (((e1 is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(e1)).@operator == Saxon.Hej.expr.parser.Token.UNION))) {
                ((Saxon.Hej.expr.VennExpression)(e1)).gatherUnionLeafIterators(leafIterators, context);
            } else {
                leafIterators.Add(e1.iterate(context));
            }
            Saxon.Hej.expr.Expression e2 = getRhsExpression();
            if (((e2 is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(e2)).@operator == Saxon.Hej.expr.parser.Token.UNION))) {
                ((Saxon.Hej.expr.VennExpression)(e2)).gatherUnionLeafIterators(leafIterators, context);
            } else {
                leafIterators.Add(e2.iterate(context));
            }
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                return (getLhsExpression().effectiveBooleanValue(context) || getRhsExpression().effectiveBooleanValue(context));
            } else {
                return base.effectiveBooleanValue(context);
            }
        }
        public override string getStreamerName() {
            return "VennExpression";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.VennExpression.VennElaborator();
        }
        public class VennElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.VennExpression exp = (Saxon.Hej.expr.VennExpression)(getExpression());
                if ((exp.getOperator() == Saxon.Hej.expr.parser.Token.UNION)) {
                    System.Collections.Generic.IList<Saxon.Hej.expr.elab.PullEvaluator> leafEvaluators = new System.Collections.Generic.List<Saxon.Hej.expr.elab.PullEvaluator>();
                    gatherUnionLeafEvaluators(exp, leafEvaluators);
                    return context => {
                        System.Collections.Generic.IList<Saxon.Hej.om.SequenceIterator> iterators = new System.Collections.Generic.List<Saxon.Hej.om.SequenceIterator>(leafEvaluators.Count);
                        foreach (Saxon.Hej.expr.elab.PullEvaluator evaluator in leafEvaluators) {
                            iterators.Add(/*Delegate.iterate()*/evaluator(context));
                        }
                        return new Saxon.Hej.expr.UnionIterator(iterators, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                    } ;
                } else {
                    Saxon.Hej.expr.elab.PullEvaluator p1 = exp.getLhsExpression().makeElaborator().elaborateForPull();
                    Saxon.Hej.expr.elab.PullEvaluator p2 = exp.getRhsExpression().makeElaborator().elaborateForPull();
                    if ((exp.getOperator() == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                        return context => new Saxon.Hej.expr.IntersectionIterator(/*Delegate.iterate()*/p1(context), /*Delegate.iterate()*/p2(context), Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                    } else {
                        return context => new Saxon.Hej.expr.DifferenceIterator(/*Delegate.iterate()*/p1(context), /*Delegate.iterate()*/p2(context), Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                    }
                }
            }
            private static void gatherUnionLeafEvaluators(Saxon.Hej.expr.VennExpression exp, System.Collections.Generic.IList<Saxon.Hej.expr.elab.PullEvaluator> leafEvaluators) {
                Saxon.Hej.expr.Expression e1 = exp.getLhsExpression();
                if (((e1 is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(e1)).getOperator() == Saxon.Hej.expr.parser.Token.UNION))) {
                    gatherUnionLeafEvaluators(((Saxon.Hej.expr.VennExpression)(e1)), leafEvaluators);
                } else {
                    leafEvaluators.Add(e1.makeElaborator().elaborateForPull());
                }
                Saxon.Hej.expr.Expression e2 = exp.getRhsExpression();
                if (((e2 is Saxon.Hej.expr.VennExpression) && (((Saxon.Hej.expr.VennExpression)(e2)).getOperator() == Saxon.Hej.expr.parser.Token.UNION))) {
                    gatherUnionLeafEvaluators(((Saxon.Hej.expr.VennExpression)(e2)), leafEvaluators);
                } else {
                    leafEvaluators.Add(e2.makeElaborator().elaborateForPull());
                }
            }
        
        }
    
    }
}