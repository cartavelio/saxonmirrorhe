using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class FilterFnStreamer : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.Expression exp = getExpression();
            System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> ops = exp.operands();
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, ops, contextInfo, reasons);
            if (((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                Saxon.Hej.expr.Expression fn = ((Saxon.Hej.expr.SystemFunctionCall)(exp)).getArg(1);
                if (checkPredicateFunction(fn, reasons)) {
                    return ps;
                } else {
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            } else {
                return ps;
            }
        }
        public static bool checkPredicateFunction(Saxon.Hej.expr.Expression fn, System.Collections.Generic.IList<string> reasons) {
            if ((fn is Saxon.Hej.functions.hof.FunctionSequenceCoercer)) {
                fn = ((Saxon.Hej.functions.hof.FunctionSequenceCoercer)(fn)).getBaseExpression();
            }
            Saxon.Hej.expr.instruct.UserFunction uf = null;
            if ((fn is Saxon.Hej.functions.hof.UserFunctionReference)) {
                uf = (Saxon.Hej.expr.instruct.UserFunction)(((Saxon.Hej.functions.hof.UserFunctionReference)(fn)).getFixedTarget().getActor());
            }
            if ((uf == null)) {
                reasons.Add((("Call to "+fn.getParentExpression().toShortString())+" is not streamable, because the predicate function is not statically known"));
                return false;
            } else {
                if (uf.getFunctionName().getURI().Equals(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS)) {
                    Saxon.Hej.expr.instruct.UserFunctionParameter first = uf.getParameterDefinitions()[0];
                    first.setFunctionStreamability(Saxon.Hej.trans.FunctionStreamability.INSPECTION);
                    first.setRequiredType(new Saxon.Hej.value.SequenceType(first.getRequiredType().getPrimaryType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
                    Saxon.Hej.expr.Expression body = uf.getBody();
                    Saxon.Eej.ee.stream.PostureAndSweep bodyPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(body, Saxon.Eej.ee.trans.ContextItemStaticInfoEE.ABSENT, reasons);
                    if (((bodyPS.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED) || (bodyPS.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS))) {
                        reasons.Add("The body of the predicate function must be grounded and motionless");
                        return false;
                    }
                } else {
                    if ((uf.getDeclaredStreamability() != Saxon.Hej.trans.FunctionStreamability.INSPECTION)) {
                        reasons.Add((("Call to "+fn.getParentExpression().toShortString())+" is not streamable, because the predicate function is not declared with streamability='inspection'"));
                        return false;
                    }
                }
            }
            return true;
        }
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            Saxon.Hej.om.Function function = (Saxon.Hej.om.Function)(((Saxon.Hej.expr.SystemFunctionCall)(getExpression())).getArg(1).evaluateItem(context));
            Saxon.Hej.om.Sequence[] args = Saxon.Hej.om.SequenceTool.makeSequenceArray(1);
            return new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => {
                args[0] = item;
                return (((Saxon.Hej.value.BooleanValue)(function.call(context, args).head())).getBooleanValue() ? Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP);
            } );
        }
    
    }
}