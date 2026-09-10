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
            Saxon.Hej.expr.Expression unorderedArg0 = arguments[0].unordered(false, visitor.isOptimizeForStreaming());
            if ((unorderedArg0 != arguments[0])) {
                return makeFunctionCall(unorderedArg0);
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
        public override string getCompilerName() {
            return "EmptyCompiler";
        }
        public override string getStreamerName() {
            return "Empty";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Empty.EmptyFnElaborator();
        }
        private class EmptyFnElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.PullEvaluator puller = arg.makeElaborator().elaborateForPull();
                return context => empty(/*Delegate.iterate()*/puller(context));
            }
        
        }
    
    }
}