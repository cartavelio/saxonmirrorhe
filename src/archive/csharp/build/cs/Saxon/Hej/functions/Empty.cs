using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Empty : Saxon.Hej.functions.Aggregate {
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            int c = arguments[0].getCardinality();
            if ((c == Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE)) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, arguments[0]);
            } else {
                if ((c == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, arguments[0]);
                }
            }
            if (((arguments[0] is Saxon.Hej.expr.VennExpression) && !(visitor.isOptimizeForStreaming()))) {
                Saxon.Hej.expr.VennExpression v = (Saxon.Hej.expr.VennExpression)(arguments[0]);
                if ((v.getOperator() == Saxon.Hej.expr.parser.Token.UNION)) {
                    Saxon.Hej.expr.Expression e0 = Saxon.Hej.functions.SystemFunction.makeCall("empty", getRetainedStaticContext(), v.getLhsExpression());
                    Saxon.Hej.expr.Expression e1 = Saxon.Hej.functions.SystemFunction.makeCall("empty", getRetainedStaticContext(), v.getRhsExpression());
                    return new Saxon.Hej.expr.AndExpression(e0, e1).optimize(visitor, contextInfo);
                }
            }
            return null;
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.BooleanValue.get(empty(arguments[0].iterate()));
        }
        private static bool empty(Saxon.Hej.om.SequenceIterator iter) {
            bool result;
            if (((iter is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(iter)).supportsHasNext())) {
                result = !(((Saxon.Hej.tree.iter.LookaheadIterator)(iter)).hasNext());
            } else {
                result = (iter.next() == null);
            }
            iter.close();
            return result;
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_d1039e380(this, this, arguments);
        }
        public override string getCompilerName() {
            return "EmptyCompiler";
        }
        public override string getStreamerName() {
            return "Empty";
        }
    
        private class Inner_SystemFunctionCall_d1039e380 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly Empty outerInstance;
            public Inner_SystemFunctionCall_d1039e380(Empty outerInstance, Empty arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
                Saxon.Hej.expr.Expression e2 = base.optimize(/*localparam*/visitor, /*localparam*/contextInfo);
                if ((/*localvar*/e2 != this)) {
                    return /*localvar*/e2;
                }
                int c = /*superMethod*/getArg(0).getCardinality();
                if ((/*localvar*/c == Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE)) {
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, /*localvar*/e2);
                } else {
                    if ((/*localvar*/c == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                        return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, /*localvar*/e2);
                    }
                }
                /*superMethod*/setArg(0, /*superMethod*/getArg(0).unordered(false, /*localparam*/visitor.isOptimizeForStreaming()));
                if ((/*superMethod*/getArg(0) is Saxon.Hej.expr.VennExpression)) {
                    Saxon.Hej.expr.VennExpression v = (Saxon.Hej.expr.VennExpression)(/*superMethod*/getArg(0));
                    if (((/*localvar*/v.getOperator() == Saxon.Hej.expr.parser.Token.UNION) && !(/*localparam*/visitor.isOptimizeForStreaming()))) {
                        Saxon.Hej.expr.Expression e0 = Saxon.Hej.functions.SystemFunction.makeCall("empty", /*superMethod*/getRetainedStaticContext(), /*localvar*/v.getLhsExpression());
                        Saxon.Hej.expr.Expression e1 = Saxon.Hej.functions.SystemFunction.makeCall("empty", /*superMethod*/getRetainedStaticContext(), /*localvar*/v.getRhsExpression());
                        return new Saxon.Hej.expr.AndExpression(/*localvar*/e0, /*localvar*/e1).optimize(/*localparam*/visitor, /*localparam*/contextInfo);
                    }
                }
                return this;
            }
            public override  /* REPLACING public */ Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
                return Saxon.Hej.value.BooleanValue.get(/*method*/effectiveBooleanValue(/*localparam*/context));
            }
            public override  /* REPLACING public */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.om.SequenceIterator iter = /*superMethod*/getArg(0).iterate(/*localparam*/c);
                bool result;
                if (((/*localvar*/iter is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(/*localvar*/iter)).supportsHasNext())) {
                    /*localvar*/result = !(((Saxon.Hej.tree.iter.LookaheadIterator)(/*localvar*/iter)).hasNext());
                } else {
                    /*localvar*/result = (/*localvar*/iter.next() == null);
                }
                /*localvar*/iter.close();
                return /*localvar*/result;
            }
            public override int getNetCost() {
                return 0;
            }
        }

    }
}