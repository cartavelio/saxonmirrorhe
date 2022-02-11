using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public abstract class FunctionCall : Saxon.Hej.expr.Expression {
        private Saxon.Hej.expr.oper.OperandArray operanda;
        protected internal void setOperanda(Saxon.Hej.expr.oper.OperandArray operanda) {
            this.operanda = operanda;
        }
        public Saxon.Hej.expr.oper.OperandArray getOperanda() {
            return operanda;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            if ((operanda != null)) {
                return operanda.operands();
            } else {
                return new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            }
        }
        public abstract Saxon.Hej.om.Function getTargetFunction(Saxon.Hej.expr.XPathContext context);
        public abstract Saxon.Hej.om.StructuredQName getFunctionName();
        public int getArity() {
            return getOperanda().getNumberOfOperands();
        }
        public void setArguments(Saxon.Hej.expr.Expression[] args) {
            setOperanda(new Saxon.Hej.expr.oper.OperandArray(this, args));
        }
        protected internal void setOperanda(Saxon.Hej.expr.Expression[] args, Saxon.Hej.expr.OperandRole[] roles) {
            setOperanda(new Saxon.Hej.expr.oper.OperandArray(this, args, roles));
        }
        public Saxon.Hej.expr.Expression[] getArguments() {
            Saxon.Hej.expr.Expression[] result = new Saxon.Hej.expr.Expression[getArity()];
            int i = 0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                result[i++] = o.getChildExpression();
            }
            return result;
        }
        public Saxon.Hej.expr.Expression getArg(int n) {
            return getOperanda().getOperandExpression(n);
        }
        public void setArg(int n, Saxon.Hej.expr.Expression child) {
            getOperanda().setOperand(n, child);
            adoptChildExpression(child);
        }
        protected internal Saxon.Hej.expr.Expression simplifyArguments(Saxon.Hej.expr.StaticContext env) {
            for (int i = 0; (i<getArguments().Length); i++) {
                Saxon.Hej.expr.Expression exp = getArg(i).simplify();
                if ((exp != getArg(i))) {
                    adoptChildExpression(exp);
                    setArg(i, exp);
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            checkArguments(visitor);
            return preEvaluateIfConstant(visitor);
        }
        protected internal Saxon.Hej.expr.Expression preEvaluateIfConstant(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)) {
                bool @fixed = true;
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    if (!(((o.getChildExpression() is Saxon.Hej.expr.Literal)))) {
                        @fixed = false;
                    }
                }
                if (@fixed) {
                    try {
                        return preEvaluate(visitor);
                    } catch (Saxon.Hej.trans.NoDynamicContextException err) {
                        return this;
                    }
                }
            }
            return this;
        }
        public void checkFunctionCall(Saxon.Hej.om.Function target, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(visitor.getStaticContext().isInBackwardsCompatibleMode());
            Saxon.Hej.value.SequenceType[] argTypes = target.getFunctionItemType().getArgumentTypes();
            if ((target.isSequenceVariadic() && (getArity() == 1))) {
                string name = ((getFunctionName() == null) ? "" : getFunctionName().getDisplayName());
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, name, 0);
                setArg(0, tc.staticTypeCheck(getArg(0), new Saxon.Hej.value.SequenceType(argTypes[0].getPrimaryType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE), role, visitor));
            } else {
                int n = target.getArity();
                for (int i = 0; (i<n); i++) {
                    string name = ((getFunctionName() == null) ? "" : getFunctionName().getDisplayName());
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, name, i);
                    setArg(i, tc.staticTypeCheck(getArg(i), argTypes[i], role, visitor));
                }
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            optimizeChildren(visitor, contextItemType);
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)) {
                bool @fixed = true;
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    if (!(((o.getChildExpression() is Saxon.Hej.expr.Literal)))) {
                        @fixed = false;
                        break ;
                    }
                }
                if (@fixed) {
                    return preEvaluate(visitor);
                }
            }
            return this;
        }
        public override int getNetCost() {
            return 5;
        }
        public virtual Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            if ((((getIntrinsicDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT))) != 0)) {
                return this;
            }
            try {
                try {
                    Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(visitor.getStaticContext().makeEarlyEvaluationContext())), this);
                    Saxon.Hej.expr.parser.Optimizer.trace(visitor.getConfiguration(), ("Pre-evaluated function call "+toShortString()), lit);
                    return lit;
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw e.getXPathException();
                }
            } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                return this;
            } catch (Saxon.Ejava.lang.UnsupportedOperationException e) {
                if ((e.InnerException is Saxon.Hej.trans.NoDynamicContextException)) {
                    return this;
                } else {
                    throw;
                }
            }
        }
        protected internal virtual void checkArguments(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
        }
        protected internal int checkArgumentCount(int min, int max) {
            int numArgs = getArity();
            string msg = null;
            if (((min == max) && (numArgs != min))) {
                msg = ((("Function "+getDisplayName())+" must have ")+pluralArguments(min));
            } else {
                if ((numArgs<min)) {
                    msg = ((("Function "+getDisplayName())+" must have at least ")+pluralArguments(min));
                } else {
                    if ((numArgs>max)) {
                        msg = ((("Function "+getDisplayName())+" must have no more than ")+pluralArguments(max));
                    }
                }
            }
            if ((msg != null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg, "XPST0017");
                err.setIsStaticError(true);
                err.setLocation(getLocation());
                throw err;
            }
            return numArgs;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public static string pluralArguments(int num) {
            return ((num == 1) ? "one argument" : ((num+" arguments")));
        }
        public Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addExternalFunctionCallToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodes) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                result.addNodeSet(o.getChildExpression().addToPathMap(pathMap, pathMapNodes));
            }
            result.setHasUnknownDependencies();
            return result;
        }
        public override string getExpressionName() {
            return "functionCall";
        }
        public string getDisplayName() {
            Saxon.Hej.om.StructuredQName fName = getFunctionName();
            return ((fName == null) ? "(anonymous)" : fName.getDisplayName());
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
            Saxon.Hej.om.StructuredQName fName = getFunctionName();
            string f;
            if ((fName == null)) {
                f = "$anonymousFunction";
            } else {
                if (fName.hasURI(Saxon.Hej.lib.NamespaceConstant.FN)) {
                    f = fName.getLocalPart();
                } else {
                    f = fName.getEQName();
                }
            }
            buff.append(f);
            bool first = true;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                buff.append((first ? "(" : ", "));
                buff.append(o.getChildExpression().ToString());
                first = false;
            }
            buff.append((first ? "()" : ")"));
            return buff.ToString();
        }
        public override string toShortString() {
            Saxon.Hej.om.StructuredQName fName = getFunctionName();
            return ((((((fName == null) ? "$anonFn" : fName.getDisplayName()))+"(")+(((getArity() == 0) ? "" : "...")))+")");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("functionCall", this);
            if ((getFunctionName() == null)) {
                throw new Saxon.Ejava.lang.AssertionError("Exporting call to anonymous function");
            } else {
                @out.emitAttribute("name", getFunctionName().getDisplayName());
            }
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            @out.endElement();
        }
        public override bool Equals(System.Object o) {
            if (!(((o is Saxon.Hej.expr.FunctionCall)))) {
                return false;
            }
            if ((getFunctionName() == null)) {
                return (this == o);
            }
            Saxon.Hej.expr.FunctionCall f = (Saxon.Hej.expr.FunctionCall)(o);
            if (!(getFunctionName().Equals(f.getFunctionName()))) {
                return false;
            }
            if ((getArity() != f.getArity())) {
                return false;
            }
            for (int i = 0; (i<getArity()); i++) {
                if (!(getArg(i).isEqual(f.getArg(i)))) {
                    return false;
                }
            }
            return true;
        }
        protected internal override int computeHashCode() {
            if ((getFunctionName() == null)) {
                return base.computeHashCode();
            }
            int h = getFunctionName().GetHashCode();
            for (int i = 0; (i<getArity()); i++) {
                h ^= getArg(i).GetHashCode();
            }
            return h;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Function target = getTargetFunction(context);
            Saxon.Hej.om.Sequence[] actualArgs = evaluateArguments(context);
            try {
                return target.call(context, actualArgs).iterate();
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                e.maybeSetFailingExpression(this);
                throw;
            }
        }
        public virtual Saxon.Hej.om.Sequence[] evaluateArguments(Saxon.Hej.expr.XPathContext context) {
            int numArgs = getArity();
            Saxon.Hej.om.Sequence[] actualArgs = new Saxon.Hej.om.Sequence[numArgs];
            for (int i = 0; (i<numArgs); i++) {
                actualArgs[i] = Saxon.Hej.expr.parser.ExpressionTool.lazyEvaluate(getArg(i), context, false);
            }
            return actualArgs;
        }
        public bool adjustRequiredType(Saxon.Hej.type.JavaExternalObjectType requiredType) {
            return false;
        }
    
    }
}