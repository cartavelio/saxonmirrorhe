using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class AxisExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.AxisExpression ae = (Saxon.Hej.expr.AxisExpression)(getExpression());
            int axis = ae.getAxis();
            try {
                if (Saxon.Hej.om.AxisInfo.isSubtreeAxis[axis]) {
                    Saxon.Hej.expr.SlashExpression s = new Saxon.Hej.expr.SlashExpression(new Saxon.Hej.expr.ContextItemExpression(), ae.copy(new Saxon.Hej.expr.parser.RebindingMap()));
                    return s.toPattern(config);
                } else {
                    return null;
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.Configuration config = getConfiguration();
            System.Collections.Generic.IList<string> failures = new System.Collections.Generic.List<string>();
            Saxon.Hej.pattern.Pattern pattern = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(getExpression(), config, failures);
            if ((pattern != null)) {
                return (watchManager, @out, context) => {
                    if (forGrouping) {
                        @out = new Saxon.Eej.ee.stream.feed.FilteringFeed(@out, context, new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => ((position == 1) ? Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE)));
                    }
                    return new Saxon.Eej.ee.stream.watch.Trigger(pattern, @out, context);
                } ;
            }
            return null;
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.AxisExpression exp = (Saxon.Hej.expr.AxisExpression)(getExpression());
            Saxon.Eej.ee.stream.Posture contextPosture = contextInfo.getContextItemPosture();
            int axis = exp.getAxis();
            Saxon.Hej.pattern.NodeTest test = exp.getNodeTest();
            switch (contextPosture) {
              case Saxon.Eej.ee.stream.Posture.GROUNDED:
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
              case Saxon.Eej.ee.stream.Posture.ROAMING:
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
              case Saxon.Eej.ee.stream.Posture.CLIMBING:
                switch (axis) {
                  case Saxon.Hej.om.AxisInfo.SELF:
                  case Saxon.Hej.om.AxisInfo.PARENT:
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  default:
                    addFreeRangingMessage(exp, axis, contextPosture, reasons);
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
              case Saxon.Eej.ee.stream.Posture.STRIDING:
                switch (axis) {
                  case Saxon.Hej.om.AxisInfo.PARENT:
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                  case Saxon.Hej.om.AxisInfo.SELF:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  case Saxon.Hej.om.AxisInfo.CHILD:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                    if (/*134*/Saxon.Eej.ee.stream.Streamability.isChildlessNodeKind(test)) {
                        return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                    } else {
                        return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CRAWLING, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                    }
                  default:
                    addFreeRangingMessage(exp, axis, contextPosture, reasons);
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
              case Saxon.Eej.ee.stream.Posture.CRAWLING:
                switch (axis) {
                  case Saxon.Hej.om.AxisInfo.PARENT:
                  case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                  case Saxon.Hej.om.AxisInfo.NAMESPACE:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  case Saxon.Hej.om.AxisInfo.SELF:
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CRAWLING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                  case Saxon.Hej.om.AxisInfo.CHILD:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                  case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
                    if (/*134*/Saxon.Eej.ee.stream.Streamability.isChildlessNodeKind(test)) {
                        return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                    }
                    goto default;
                  default:
                    addFreeRangingMessage(exp, axis, contextPosture, reasons);
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
              default:
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        private static void addFreeRangingMessage(Saxon.Hej.expr.Expression exp, int axis, Saxon.Eej.ee.stream.Posture contextPosture, System.Collections.Generic.IList<string> reasons) {
            if ((reasons != null)) {
                reasons.Add((((((("Cannot use the "+Saxon.Hej.om.AxisInfo.axisName[axis])+" axis when context posture is ")+contextPosture)+" (line ")+exp.getLocation().getLineNumber())+")"));
            }
        }
    
    }
}