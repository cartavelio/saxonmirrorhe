using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class DynamicFunctionCall : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand targetFunction;
        private readonly Saxon.Hej.expr.oper.OperandArray suppliedArguments;
        public DynamicFunctionCall(Saxon.Hej.expr.Expression fn, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> args)  {
            targetFunction = new Saxon.Hej.expr.Operand(this, fn, Saxon.Hej.expr.OperandRole.INSPECT);
            suppliedArguments = new Saxon.Hej.expr.oper.OperandArray(this, args.ToArray());
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType fnType = targetFunction.getChildExpression().getItemType();
            if ((fnType is Saxon.Hej.ma.map.MapType)) {
                return ((Saxon.Hej.ma.map.MapType)(fnType)).getValueType().getPrimaryType();
            } else {
                if ((fnType is Saxon.Hej.ma.arrays.ArrayItemType)) {
                    return ((Saxon.Hej.ma.arrays.ArrayItemType)(fnType)).getMemberType().getPrimaryType();
                } else {
                    if ((fnType is Saxon.Hej.type.FunctionItemType)) {
                        return ((Saxon.Hej.type.FunctionItemType)(fnType)).getResultType().getPrimaryType();
                    } else {
                        if ((fnType is Saxon.Hej.type.AnyFunctionType)) {
                            return Saxon.Hej.type.AnyItemType.getInstance();
                        } else {
                            return Saxon.Hej.type.AnyItemType.getInstance();
                        }
                    }
                }
            }
        }
        protected internal override int computeCardinality() {
            Saxon.Hej.type.ItemType fnType = targetFunction.getChildExpression().getItemType();
            if ((fnType is Saxon.Hej.ma.map.MapType)) {
                return Saxon.Hej.value.Cardinality.union(((Saxon.Hej.ma.map.MapType)(fnType)).getValueType().getCardinality(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
            } else {
                if ((fnType is Saxon.Hej.ma.arrays.ArrayItemType)) {
                    return ((Saxon.Hej.ma.arrays.ArrayItemType)(fnType)).getMemberType().getCardinality();
                } else {
                    if ((fnType is Saxon.Hej.type.FunctionItemType)) {
                        return ((Saxon.Hej.type.FunctionItemType)(fnType)).getResultType().getCardinality();
                    } else {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                    }
                }
            }
        }
        public int getArity() {
            return suppliedArguments.getNumberOfOperands();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> allOperands = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>((getArity()+1));
            allOperands.Add(targetFunction);
            for (int i = 0; (i<getArity()); i++) {
                allOperands.Add(suppliedArguments.getOperand(i));
            }
            return allOperands;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier0 = () => {
                return new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, "dynamic function call", 0);
            } ;
            targetFunction.setChildExpression(tc.staticTypeCheck(targetFunction.getChildExpression(), Saxon.Hej.value.SequenceType.SINGLE_FUNCTION, roleSupplier0, visitor));
            if ((getArity() == 1)) {
                Saxon.Hej.expr.Expression target = targetFunction.getChildExpression();
                if ((target.getItemType() is Saxon.Hej.ma.map.MapType)) {
                    return makeGetCall(visitor, Saxon.Hej.ma.map.MapFunctionSet.getInstance(31), contextInfo);
                } else {
                    if ((target.getItemType() is Saxon.Hej.ma.arrays.ArrayItemType)) {
                        return makeGetCall(visitor, Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(31), contextInfo);
                    }
                }
            }
            return this;
        }
        private Saxon.Hej.expr.Expression makeGetCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.functions.registry.BuiltInFunctionSet fnSet, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression target = targetFunction.getChildExpression();
            Saxon.Hej.expr.Expression key = suppliedArguments.getOperandExpression(0);
            Saxon.Hej.expr.Expression getter = fnSet.makeFunction("get", 2).makeFunctionCall(target, key);
            getter.setRetainedStaticContext(target.getRetainedStaticContext());
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(visitor.getStaticContext().isInBackwardsCompatibleMode());
            if ((fnSet == Saxon.Hej.ma.map.MapFunctionSet.getInstance(31))) {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "key value supplied when calling a map as a function", 0);
                ((Saxon.Hej.expr.SystemFunctionCall)(getter)).setArg(1, tc.staticTypeCheck(key, Saxon.Hej.value.SequenceType.SINGLE_ATOMIC, role, visitor));
            } else {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "subscript supplied when calling an array as a function", 0);
                ((Saxon.Hej.expr.SystemFunctionCall)(getter)).setArg(1, tc.staticTypeCheck(key, Saxon.Hej.value.SequenceType.SINGLE_INTEGER, role, visitor));
            }
            return getter.typeCheck(visitor, contextInfo);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            if (("JS".Equals(@out.getOptions().target) && (@out.getOptions().targetVersion == 2))) {
                @out.startElement("ifCall", this);
                @out.emitAttribute("name", "Q{http://saxon.sf.net/}apply");
                @out.emitAttribute("type", "*");
                if ((targetFunction.getChildExpression() is Saxon.Hej.expr.Literal)) {
                    Saxon.Hej.om.FunctionItem f = (Saxon.Hej.om.FunctionItem)((((Saxon.Hej.expr.Literal)(targetFunction.getChildExpression())).getGroundedValue()));
                    if ((f.getFunctionName() != null)) {
                        @out.emitAttribute("dyn", ((f.getFunctionName().getEQName()+"#")+f.getArity()));
                    }
                }
                targetFunction.getChildExpression().export(@out);
                @out.startSubsidiaryElement("arrayBlock");
                foreach (Saxon.Hej.expr.Operand o in suppliedArguments) {
                    o.getChildExpression().export(@out);
                }
                @out.endSubsidiaryElement();
                @out.endElement();
            } else {
                @out.startElement("dynCall", this);
                targetFunction.getChildExpression().export(@out);
                foreach (Saxon.Hej.expr.Operand o in suppliedArguments) {
                    o.getChildExpression().export(@out);
                }
                @out.endElement();
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression f = targetFunction.getChildExpression().copy(rebindings);
            System.Collections.Generic.List<Saxon.Hej.expr.Expression> args = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(getArity());
            foreach (Saxon.Hej.expr.Operand o in suppliedArguments) {
                args.Add(o.getChildExpression().copy(rebindings));
            }
            return new Saxon.Hej.expr.DynamicFunctionCall(f, args);
        }
        public override void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.DynamicFunctionCall.DynamicFunctionCallElaborator();
        }
        private class DynamicFunctionCallElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.DynamicFunctionCall expr = (Saxon.Hej.expr.DynamicFunctionCall)(getExpression());
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                Saxon.Hej.expr.elab.SequenceEvaluator[] argEvaluators = new Saxon.Hej.expr.elab.SequenceEvaluator[expr.getArity()];
                for (int i = 0; (i<argEvaluators.Length); i++) {
                    Saxon.Hej.expr.Expression arg = expr.suppliedArguments.getOperand(i).getChildExpression();
                    argEvaluators[i] = new Saxon.Hej.expr.elab.LearningEvaluator(arg, arg.makeElaborator().lazily(true));
                }
                Saxon.Hej.expr.Expression body = expr.targetFunction.getChildExpression();
                Saxon.Hej.expr.elab.ItemEvaluator functionEvaluator = body.makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.FunctionItem fn = (Saxon.Hej.om.FunctionItem)(/*Delegate.eval()*/functionEvaluator(context));
                    Saxon.Hej.type.FunctionItemType fit = fn.getFunctionItemType();
                    if ((fn.getArity() != argEvaluators.Length)) {
                        string errorCode = "XPTY0004";
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("Number of arguments required for dynamic call to "+fn.getDescription())+" is ")+fn.getArity())+"; number supplied = ")+argEvaluators.Length), errorCode);
                        err.setIsTypeError(true);
                        err.setXPathContext(context);
                        err.setLocation(expr.getLocation());
                        throw err;
                    }
                    Saxon.Hej.om.Sequence[] argValues = new Saxon.Hej.om.Sequence[argEvaluators.Length];
                    if ((fit == Saxon.Hej.type.AnyFunctionType.ANY_FUNCTION)) {
                        for (int i = 0; (i<argEvaluators.Length); i++) {
                            argValues[i] = argEvaluators[i].evaluate(context);
                        }
                    } else {
                        for (int i = 0; (i<argEvaluators.Length); i++) {
                            Saxon.Hej.value.SequenceType expected = fit.getArgumentTypes()[i];
                            Saxon.Hej.om.Sequence actual = argEvaluators[i].evaluate(context);
                            if (!(expected.Equals(Saxon.Hej.value.SequenceType.ANY_SEQUENCE))) {
                                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role;
                                role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, fn.getDescription(), 0);
                                actual = th.applyFunctionConversionRules(actual, expected, role, Saxon.Hej.expr.parser.Loc.NONE);
                            }
                            argValues[i] = actual;
                        }
                    }
                    Saxon.Hej.expr.XPathContext c2 = fn.makeNewContext(context, null);
                    c2.setCurrentOutputUri(null);
                    if ((c2 is Saxon.Hej.expr.XPathContextMajor)) {
                        ((Saxon.Hej.expr.XPathContextMajor)(c2)).setCurrentRegexIterator(null);
                    }
                    Saxon.Hej.om.Sequence rawResult = fn.call(c2, argValues);
                    if (fn.isTrustedResultType()) {
                        return rawResult.iterate();
                    } else {
                        Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> resultRole = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION_RESULT, "fn:apply", -1);
                        return th.applyFunctionConversionRules(rawResult, fit.getResultType(), resultRole, Saxon.Hej.expr.parser.Loc.NONE).iterate();
                    }
                } ;
            }
        
        }
    
    }
}