using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ApplyFn : Saxon.Hej.functions.SystemFunction {
        private string dynamicFunctionCall;
        public ApplyFn()  {
        }
        public void setDynamicFunctionCall(string fnExpr) {
            dynamicFunctionCall = fnExpr;
        }
        public bool isDynamicFunctionCall() {
            return (dynamicFunctionCall != null);
        }
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.ItemType fnType = args[0].getItemType();
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
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments.Length == 2) && (arguments[1] is Saxon.Hej.ma.arrays.SquareArrayConstructor))) {
                Saxon.Hej.expr.Expression target = arguments[0];
                if ((target.getItemType() is Saxon.Hej.ma.map.MapType)) {
                    return makeGetCall(visitor, Saxon.Hej.ma.map.MapFunctionSet.getInstance(31), contextInfo, arguments);
                } else {
                    if ((target.getItemType() is Saxon.Hej.ma.arrays.ArrayItemType)) {
                        return makeGetCall(visitor, Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance(31), contextInfo, arguments);
                    }
                }
            }
            return null;
        }
        private Saxon.Hej.expr.Expression makeGetCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.functions.registry.BuiltInFunctionSet fnSet, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.expr.Expression target = arguments[0];
            Saxon.Hej.expr.Expression key = ((Saxon.Hej.ma.arrays.SquareArrayConstructor)(arguments[1])).getOperanda().getOperand(0).getChildExpression();
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
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.FunctionItem function = (Saxon.Hej.om.FunctionItem)(arguments[0].head());
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.FunctionItemType fit = function.getFunctionItemType();
            Saxon.Hej.ma.arrays.ArrayItem args = (Saxon.Hej.ma.arrays.ArrayItem)(arguments[1].head());
            if ((function.getArity() != args.arrayLength())) {
                string errorCode = (isDynamicFunctionCall() ? "XPTY0004" : "FOAP0001");
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((((("Number of arguments required for dynamic call to "+function.getDescription())+" is ")+function.getArity())+"; number supplied = ")+args.arrayLength()), errorCode);
                err.setIsTypeError(isDynamicFunctionCall());
                err.setXPathContext(context);
                throw err;
            }
            Saxon.Hej.om.Sequence[] argArray = new Saxon.Hej.om.Sequence[args.arrayLength()];
            if ((fit == Saxon.Hej.type.AnyFunctionType.ANY_FUNCTION)) {
                for (int i = 0; (i<argArray.Length); i++) {
                    argArray[i] = args.get(i);
                }
            } else {
                for (int i = 0; (i<argArray.Length); i++) {
                    Saxon.Hej.value.SequenceType expected = fit.getArgumentTypes()[i];
                    Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role;
                    if (isDynamicFunctionCall()) {
                        role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, dynamicFunctionCall.ToString(), 0);
                    } else {
                        int pos = i;
                        role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, "fn:apply", (pos+1));
                    }
                    Saxon.Hej.om.Sequence converted = th.applyFunctionConversionRules(args.get(i), expected, role, Saxon.Hej.expr.parser.Loc.NONE);
                    argArray[i] = converted.materialize();
                }
            }
            if (function.isSequenceVariadic()) {
                System.Collections.Generic.IList<Saxon.Hej.om.Item> members = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                foreach (Saxon.Hej.om.GroundedValue mem in args.members()) {
                    foreach (Saxon.Hej.om.Item it in mem.asIterable()) {
                        members.Add(it);
                    }
                }
                Saxon.Hej.om.GroundedValue argSequence = Saxon.Hej.value.SequenceExtent.makeSequenceExtent(members);
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> singletonArg = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(1);
                singletonArg.Add(argSequence);
            } else {
            }
            Saxon.Hej.om.Sequence rawResult = dynamicCall(function, context, argArray);
            if (function.isTrustedResultType()) {
                return rawResult;
            } else {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> resultRole = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION_RESULT, "fn:apply", -1);
                return th.applyFunctionConversionRules(rawResult, fit.getResultType(), resultRole, Saxon.Hej.expr.parser.Loc.NONE);
            }
        }
        public override void exportAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.emitAttribute("dyn", dynamicFunctionCall);
        }
        public override void importAttributes(Saxon.Ejava.util.Properties attributes) {
            dynamicFunctionCall = attributes.getProperty("dyn");
        }
    
    }
}