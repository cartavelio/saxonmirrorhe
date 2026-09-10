using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class UserFunctionCallAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if ((reasons == null)) {
                reasons = new System.Collections.Generic.List<string>();
            }
            Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.instruct.UserFunction target = exp.getFunction();
            Saxon.Hej.trans.FunctionStreamability category = target.getDeclaredStreamability();
            Saxon.Hej.expr.Operand[] operanda = exp.getOperanda().copy();
            for (int i = 1; (i<operanda.Length); i++) {
                operanda[i].setUsage(Saxon.Hej.expr.Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
            }
            switch (category.innerEnumValue) {
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.UNCLASSIFIED:
                throw new Saxon.Ejava.lang.AssertionError();
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ABSORBING:
                {
                    throw new Saxon.Ejava.lang.AssertionError();
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ASCENT:
                {
                    operanda[0].setUsage(Saxon.Hej.expr.OperandUsage.INSPECTION);
                    Saxon.Eej.ee.stream.PostureAndSweep ps0 = /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, Saxon.Ejava.util.Arrays.asList(operanda), contextInfo, reasons);
                    if (((ps0.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING) || (ps0.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS))) {
                        return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                    }
                    if ((ps0.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                        return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
                    }
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.FILTER:
                {
                    operanda[0].setUsage(Saxon.Hej.expr.OperandUsage.TRANSMISSION);
                    return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, Saxon.Ejava.util.Arrays.asList(operanda), contextInfo, reasons);
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.INSPECTION:
                {
                    operanda[0].setUsage(Saxon.Hej.expr.OperandUsage.INSPECTION);
                    return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, Saxon.Ejava.util.Arrays.asList(operanda), contextInfo, reasons);
                }
              case Saxon.Hej.trans.FunctionStreamability.InnerEnum.SHALLOW_DESCENT:
                throw new Saxon.Ejava.lang.AssertionError();
              default:
                reasons.Add(("Saxon 9.8 does not implement streamability = "+category));
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            return null;
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.UserFunctionCall call = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.instruct.UserFunction function = call.getTargetFunction(context);
            Saxon.Hej.trans.FunctionStreamability ability = function.getDeclaredStreamability();
            if ((ability == Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED)) {
                throw new Saxon.Ejava.lang.AssertionError();
            } else {
                throw new Saxon.Hej.trans.XPathException((("Streamed calls on functions with streamability = "+ability)+" are currently not supported"), "SXST0062");
            }
        }
    
    }
}