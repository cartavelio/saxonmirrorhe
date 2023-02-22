using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public abstract class TransmissionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Eej.ee.stream.Posture posture = /*134*/Saxon.Eej.ee.stream.Streamability.getPosture(getExpression());
            if (/*134*/Saxon.Eej.ee.stream.Streamability.isIncrementalPosture(posture)) {
                return (watchManager, @out, context) => makeTransmissionFlow(watchManager, getExpression(), @out, context);
            } else {
                return null;
            }
        }
        public virtual Saxon.Eej.ee.stream.watch.Trigger makeTransmissionFlow(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression select = null;
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                if (((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING) && o.hasSameFocus())) {
                    select = o.getChildExpression();
                    break ;
                }
            }
            if ((select == null)) {
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    if ((o.hasSameFocus() && (o.getUsage() == Saxon.Hej.expr.OperandUsage.TRANSMISSION))) {
                        select = o.getChildExpression();
                        break ;
                    }
                }
            }
            Saxon.Hej.pattern.Pattern selection = null;
            if (((select == null) && (exp is Saxon.Hej.expr.SimpleStepExpression))) {
                Saxon.Hej.expr.SimpleStepExpression sse = (Saxon.Hej.expr.SimpleStepExpression)(exp);
                Saxon.Hej.expr.Expression lhs = sse.getLhsExpression();
                if (((lhs is Saxon.Hej.expr.LocalVariableReference) && (((Saxon.Hej.expr.LocalVariableReference)(lhs)).getBinding() is Saxon.Hej.expr.instruct.UserFunctionParameter))) {
                    Saxon.Hej.expr.AxisExpression ae = (Saxon.Hej.expr.AxisExpression)(sse.getRhsExpression());
                    int axis = ae.getAxis();
                    if ((axis == Saxon.Hej.om.AxisInfo.CHILD)) {
                        selection = new Saxon.Hej.pattern.AncestorQualifiedPattern(new Saxon.Hej.pattern.NodeTestPattern(ae.getNodeTest()), Saxon.Hej.pattern.AnchorPattern.getInstance(), Saxon.Hej.om.AxisInfo.PARENT);
                    } else {
                        if ((axis == Saxon.Hej.om.AxisInfo.DESCENDANT)) {
                            selection = new Saxon.Hej.pattern.AncestorQualifiedPattern(new Saxon.Hej.pattern.NodeTestPattern(ae.getNodeTest()), Saxon.Hej.pattern.AnchorPattern.getInstance(), Saxon.Hej.om.AxisInfo.ANCESTOR);
                        }
                    }
                }
            }
            if (((select == null) && (selection == null))) {
                throw new Saxon.Hej.trans.XPathException(("No transmission watch for "+exp.toShortString()), Saxon.Hej.trans.SaxonErrorCode.SXST0067, exp.getLocation());
            }
            if (((select != null) && (selection == null))) {
                selection = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(select, context.getConfiguration());
            }
            Saxon.Eej.ee.stream.feed.ItemFeed fw = makeItemFeed(watchManager, @out, context);
            if ((selection != null)) {
                return new Saxon.Eej.ee.stream.watch.Trigger(selection, fw, context);
            } else {
                Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
                if ((sa is Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)) {
                    return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(sa)).makeTransmissionFlow(watchManager, select, fw, context);
                }
            }
            throw new Saxon.Hej.trans.XPathException(("No transmission filter available for "+select.toShortString()), Saxon.Hej.trans.SaxonErrorCode.SXST0067, exp.getLocation());
        }
    
        /* Creating stub method makeItemFeed to satisfy interface FeedMaker */
        public virtual Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager arg1, Saxon.Eej.ee.stream.feed.ItemFeed arg2, Saxon.Hej.expr.XPathContext arg3) {
            throw new System.NotImplementedException();
        }

    }
}