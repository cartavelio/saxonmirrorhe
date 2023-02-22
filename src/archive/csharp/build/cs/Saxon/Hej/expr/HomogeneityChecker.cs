using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class HomogeneityChecker : Saxon.Hej.expr.UnaryExpression {
        public HomogeneityChecker(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.INSPECT;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((getBaseExpression() is Saxon.Hej.expr.HomogeneityChecker)) {
                return getBaseExpression().typeCheck(visitor, contextInfo);
            }
            getOperand().typeCheck(visitor, contextInfo);
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType type = getBaseExpression().getItemType();
            if (type.Equals(Saxon.Hej.type.ErrorType.getInstance())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.type.Affinity rel = th.relationship(type, Saxon.Hej.pattern.AnyNodeTest.getInstance());
            if ((rel == Saxon.Hej.type.Affinity.DISJOINT)) {
                return getBaseExpression();
            } else {
                if (((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    Saxon.Hej.expr.Expression savedBase = getBaseExpression();
                    Saxon.Hej.expr.Expression parent = getParentExpression();
                    getOperand().detachChild();
                    Saxon.Hej.expr.sort.DocumentSorter ds = new Saxon.Hej.expr.sort.DocumentSorter(savedBase);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ds);
                    ds.setParentExpression(parent);
                    return ds;
                }
            }
            return this;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            return getBaseExpression().toPattern(config);
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((getBaseExpression() is Saxon.Hej.expr.HomogeneityChecker)) {
                return getBaseExpression().optimize(visitor, contextInfo);
            }
            return base.optimize(visitor, contextInfo);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.HomogeneityChecker hc = new Saxon.Hej.expr.HomogeneityChecker(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, hc);
            return hc;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator @base = getBaseExpression().iterate(context);
            return new Saxon.Hej.tree.iter.HomogeneityCheckerIterator(@base, getLocation());
        }
        public override string getExpressionName() {
            return "homCheck";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.HomogeneityChecker.HomogeneityCheckerElaborator();
        }
        public class HomogeneityCheckerElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.HomogeneityChecker exp = (Saxon.Hej.expr.HomogeneityChecker)(getExpression());
                Saxon.Hej.s9api.Location location = exp.getLocation();
                Saxon.Hej.expr.Expression arg = exp.getBaseExpression();
                Saxon.Hej.expr.elab.PullEvaluator argEval = arg.makeElaborator().elaborateForPull();
                return context => new Saxon.Hej.tree.iter.HomogeneityCheckerIterator(/*Delegate.iterate()*/argEval(context), location);
            }
        
        }
    
    }
}