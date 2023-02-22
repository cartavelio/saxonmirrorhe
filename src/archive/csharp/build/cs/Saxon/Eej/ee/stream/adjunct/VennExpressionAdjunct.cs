using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class VennExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.VennExpression exp = (Saxon.Hej.expr.VennExpression)(getExpression());
            Saxon.Hej.expr.Expression e0 = exp.getLhsExpression();
            Saxon.Hej.expr.Expression e1 = exp.getRhsExpression();
            int @operator = exp.getOperator();
            Saxon.Hej.pattern.Pattern p0 = makeStreamingPattern(e0, config);
            if ((p0 == null)) {
                return null;
            }
            Saxon.Hej.pattern.Pattern p1 = makeStreamingPattern(e1, config);
            if ((p1 == null)) {
                return null;
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                return new Saxon.Hej.pattern.UnionPattern(p0, p1);
            } else {
                if ((@operator == Saxon.Hej.expr.parser.Token.EXCEPT)) {
                    return new Saxon.Hej.pattern.ExceptPattern(p0, p1);
                } else {
                    return new Saxon.Hej.pattern.IntersectPattern(p0, p1);
                }
            }
        }
        private static Saxon.Hej.pattern.Pattern makeStreamingPattern(Saxon.Hej.expr.Expression e, Saxon.Hej.Configuration config) {
            if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(e) != null) && (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(e) == Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                return null;
            }
            return /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(e, config);
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.VennExpression exp = (Saxon.Hej.expr.VennExpression)(getExpression());
            Saxon.Eej.ee.stream.PostureAndSweep ps0 = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getLhsExpression(), contextInfo, reasons);
            Saxon.Eej.ee.stream.PostureAndSweep ps1 = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getRhsExpression(), contextInfo, reasons);
            if (((ps0.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) || (ps1.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (ps0.Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                return ps1;
            }
            if (ps1.Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                return ps0;
            }
            if ((ps0.Equals(Saxon.Eej.ee.stream.PostureAndSweep.CLIMBING_AND_MOTIONLESS) && ps1.Equals(Saxon.Eej.ee.stream.PostureAndSweep.CLIMBING_AND_MOTIONLESS))) {
                return ps0;
            }
            Saxon.Eej.ee.stream.Posture p0 = ps0.getPosture();
            Saxon.Eej.ee.stream.Posture p1 = ps1.getPosture();
            if (((((p0 == Saxon.Eej.ee.stream.Posture.STRIDING) || (p0 == Saxon.Eej.ee.stream.Posture.CRAWLING))) && (((p1 == Saxon.Eej.ee.stream.Posture.STRIDING) || (p1 == Saxon.Eej.ee.stream.Posture.CRAWLING))))) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CRAWLING, /*134*/Saxon.Eej.ee.stream.Streamability.wider(ps0.getSweep(), ps1.getSweep()));
            }
            if ((reasons != null)) {
                reasons.Add((((("Operands of "+Saxon.Hej.expr.parser.Token.tokens[exp.getOperator()])+" expression on line ")+exp.getLocation().getLineNumber())+" have mixed posture"));
            }
            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.VennExpression exp = (Saxon.Hej.expr.VennExpression)(getExpression());
            Saxon.Eej.ee.stream.Sweep s0 = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp.getLhsExpression());
            Saxon.Eej.ee.stream.Sweep s1 = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp.getRhsExpression());
            bool a0consuming = (s0 == Saxon.Eej.ee.stream.Sweep.CONSUMING);
            int @operator = exp.getOperator();
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                if (a0consuming) {
                    return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.UnionFeed(exp, exp.getRhsExpression(), @out, context);
                } else {
                    return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.UnionFeed(exp, exp.getLhsExpression(), @out, context);
                }
            } else {
                if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                    return new Saxon.Eej.ee.stream.feed.FilteringFeed(@out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.OpaqueFilter);
                } else {
                    if ((@operator == Saxon.Hej.expr.parser.Token.EXCEPT)) {
                        if (a0consuming) {
                            return new Saxon.Eej.ee.stream.feed.FilteringFeed(@out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.TransparentFilter);
                        } else {
                            return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.FirstOperandOnlyFeed(@out, context, exp.getLhsExpression());
                        }
                    } else {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException();
                    }
                }
            }
        }
        private class FirstOperandOnlyFeed : Saxon.Eej.ee.stream.feed.FilteringFeed {
            internal Saxon.Hej.expr.Expression lhs;
            public FirstOperandOnlyFeed(Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Expression lhs) : base (@out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.OpaqueFilter) {
                this.lhs = lhs;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                processItems(lhs.iterate(getContext()), getNextOutputter());
            }
        
        }
        private class UnionFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal Saxon.Hej.expr.Expression nonConsumingOperand;
            internal Saxon.Hej.om.SequenceIterator nonConsumingIterator;
            internal bool atStart = true;
            internal bool emitNonConsumingOperandAtEnd = true;
            public UnionFeed(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Expression nonConsumingOperand, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
                this.nonConsumingOperand = nonConsumingOperand;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                nonConsumingIterator = nonConsumingOperand.iterate(getContext());
            }
            public override void close() {
                if (emitNonConsumingOperandAtEnd) {
                    processItems(nonConsumingIterator, getNextOutputter());
                }
                getNextOutputter().close();
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (atStart) {
                    Saxon.Hej.om.NodeInfo first = (Saxon.Hej.om.NodeInfo)(nonConsumingIterator.next());
                    if ((first != null)) {
                        if ((Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare((Saxon.Hej.om.NodeInfo)(item), first)<0)) {
                            nonConsumingIterator = new Saxon.Hej.tree.iter.PrependSequenceIterator(first, nonConsumingIterator);
                            emitNonConsumingOperandAtEnd = true;
                        } else {
                            getNextOutputter().append(first);
                            processItems(nonConsumingIterator, getNextOutputter());
                            emitNonConsumingOperandAtEnd = false;
                        }
                    } else {
                        emitNonConsumingOperandAtEnd = false;
                    }
                }
                atStart = false;
                getNextOutputter().append(item);
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if (atStart) {
                    Saxon.Hej.om.NodeInfo first = (Saxon.Hej.om.NodeInfo)(nonConsumingIterator.next());
                    if ((first != null)) {
                        if ((Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance().compare(node, first)<0)) {
                            nonConsumingIterator = new Saxon.Hej.tree.iter.PrependSequenceIterator(first, nonConsumingIterator);
                            emitNonConsumingOperandAtEnd = true;
                        } else {
                            getNextOutputter().append(first);
                            processItems(nonConsumingIterator, getNextOutputter());
                            emitNonConsumingOperandAtEnd = false;
                        }
                    } else {
                        emitNonConsumingOperandAtEnd = false;
                    }
                }
                atStart = false;
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                getResultFeed().endSelectedParentNode(locationId);
            }
        
        }
    
    }
}