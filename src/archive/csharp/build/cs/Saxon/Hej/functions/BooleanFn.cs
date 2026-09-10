using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class BooleanFn : Saxon.Hej.functions.SystemFunction {
        public override void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.trans.XPathException err = Saxon.Hej.expr.parser.TypeChecker.ebvError(arguments[0], visitor.getConfiguration().getTypeHierarchy());
            if ((err != null)) {
                throw err;
            }
        }
        public static Saxon.Hej.expr.Expression rewriteEffectiveBooleanValue(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            bool forStreaming = visitor.isOptimizeForStreaming();
            exp = Saxon.Hej.expr.parser.ExpressionTool.unsortedIfHomogeneous(exp, forStreaming);
            if ((exp is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue();
                if ((val is Saxon.Hej.value.BooleanValue)) {
                    return exp;
                }
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(val.iterate())), exp);
            }
            if ((exp is Saxon.Hej.expr.ValueComparison)) {
                Saxon.Hej.expr.ValueComparison vc = (Saxon.Hej.expr.ValueComparison)(exp);
                if ((vc.getResultWhenEmpty() == null)) {
                    vc.setResultWhenEmpty(Saxon.Hej.value.BooleanValue.FALSE);
                }
                return exp;
            } else {
                if (exp.isCallOn(typeof(Saxon.Hej.functions.BooleanFn))) {
                    return ((Saxon.Hej.expr.SystemFunctionCall)(exp)).getArg(0);
                } else {
                    if ((th.isSubType(exp.getItemType(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) && (exp.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                        return exp;
                    } else {
                        if (exp.isCallOn(typeof(Saxon.Hej.functions.Count))) {
                            Saxon.Hej.expr.Expression exists = Saxon.Hej.functions.SystemFunction.makeCall("exists", exp.getRetainedStaticContext(), ((Saxon.Hej.expr.SystemFunctionCall)(exp)).getArg(0));
                            System.Diagnostics.Debug.Assert((exists != null));
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, exists);
                            return exists.optimize(visitor, contextItemType);
                        } else {
                            if ((exp.getItemType() is Saxon.Hej.pattern.NodeTest)) {
                                Saxon.Hej.expr.Expression exists = Saxon.Hej.functions.SystemFunction.makeCall("exists", exp.getRetainedStaticContext(), exp);
                                System.Diagnostics.Debug.Assert((exists != null));
                                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, exists);
                                return exists.optimize(visitor, contextItemType);
                            } else {
                                return null;
                            }
                        }
                    }
                }
            }
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext c, Saxon.Hej.om.Sequence[] arguments) {
            bool bValue = Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(arguments[0].iterate());
            return Saxon.Hej.value.BooleanValue.get(bValue);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_d1035e554(this, this, arguments);
        }
        public override string getCompilerName() {
            return "BooleanFnCompiler";
        }
        public override string getStreamerName() {
            return "BooleanFn";
        }
    
        private class Inner_SystemFunctionCall_d1035e554 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly BooleanFn outerInstance;
            public Inner_SystemFunctionCall_d1035e554(BooleanFn outerInstance, BooleanFn arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
                Saxon.Hej.expr.Expression e = base.optimize(/*localparam*/visitor, /*localparam*/contextItemType);
                if ((/*localvar*/e == this)) {
                    Saxon.Hej.expr.Expression ebv = /*outerStaticMethod*/rewriteEffectiveBooleanValue(this.getArg(0), /*localparam*/visitor, /*localparam*/contextItemType);
                    if ((/*localvar*/ebv != null)) {
                        /*localvar*/ebv = /*localvar*/ebv.optimize(/*localparam*/visitor, /*localparam*/contextItemType);
                        if (((/*localvar*/ebv.getItemType() == Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) && (/*localvar*/ebv.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                            /*localvar*/ebv.setParentExpression(/*superMethod*/getParentExpression());
                            return /*localvar*/ebv;
                        } else {
                            /*superMethod*/setArg(0, /*localvar*/ebv);
                            /*superMethod*/adoptChildExpression(/*localvar*/ebv);
                            return this;
                        }
                    }
                }
                return /*localvar*/e;
            }
            public override  /* REPLACING public */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
                try {
                    return /*superMethod*/getArg(0).effectiveBooleanValue(/*localparam*/c);
                } catch (Saxon.Hej.trans.XPathException e) {
                    /*exception*/e.maybeSetLocation(/*superMethod*/getLocation());
                    /*exception*/e.maybeSetContext(/*localparam*/c);
                    throw;
                }
            }
            public override  /* REPLACING public */ Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
                return Saxon.Hej.value.BooleanValue.get(/*method*/effectiveBooleanValue(/*localparam*/context));
            }
        }

    }
}