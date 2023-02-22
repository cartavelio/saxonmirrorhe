using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ForkAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        private Saxon.Eej.ee.stream.Inversion[] actionInversions;
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.Fork exp = (Saxon.Hej.expr.instruct.Fork)(getExpression());
            if ((exp.getSize() == 0)) {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            } else {
                Saxon.Eej.ee.stream.PostureAndSweep eccentric = null;
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(o.getChildExpression(), contextInfo, reasons);
                    if (((ps.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING) || (ps.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                        return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                    }
                    if (((ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED) && (ps.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS))) {
                        if ((eccentric != null)) {
                            reasons.Add("At most one branch of the xsl:fork may return streamed nodes");
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            eccentric = ps;
                        }
                    } else {
                        if ((ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        }
                    }
                }
                if ((eccentric != null)) {
                    return eccentric;
                } else {
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                }
            }
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.instruct.Fork fork = (Saxon.Hej.expr.instruct.Fork)(getExpression());
            if ((actionInversions == null)) {
                actionInversions = new Saxon.Eej.ee.stream.Inversion[fork.getSize()];
                int i = 0;
                foreach (Saxon.Hej.expr.Operand o in fork.operands()) {
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        actionInversions[i] = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(o.getChildExpression(), forGrouping);
                    } else {
                        actionInversions[i] = null;
                    }
                    i++;
                }
            }
            return (watchManager, @out, context) => new Saxon.Eej.ee.stream.watch.Trigger(Saxon.Hej.pattern.AnchorPattern.getInstance(), new Saxon.Eej.ee.stream.adjunct.ForkAdjunct.ForkFeed(getExpression(), actionInversions, @out, context, watchManager), context);
        }
        private class ForkFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            private Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer[] writers;
            private Saxon.Eej.ee.stream.Inversion[] actionInversions;
            public ForkFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.Inversion[] actionInversions, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.watch.WatchManager watchManager) : base (exp, result, context) {
                this.actionInversions = actionInversions;
                this.watchManager = watchManager;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.instruct.Fork fork = (Saxon.Hej.expr.instruct.Fork)(getExpression());
                writers = new Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer[fork.getSize()];
                int i = 0;
                foreach (Saxon.Hej.expr.Operand o in fork.operands()) {
                    Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer writer = new Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer(getNextOutputter(), this, getContext());
                    writer.setPipelineConfiguration(getPipelineConfiguration());
                    writers[i] = writer;
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        Saxon.Eej.ee.stream.feed.ItemFeed feed = new Saxon.Eej.ee.stream.feed.DecomposingFeed(writer, getContext());
                        if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(o.getChildExpression()) != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                            feed = new Saxon.Eej.ee.stream.feed.CopyOfFeed(watchManager, feed, getContext());
                        }
                        watchManager.addWatch(actionInversions[i].getWatch(watchManager, feed, getContext()), true);
                    } else {
                        o.getChildExpression().process(writer, getContext());
                    }
                    i++;
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return null;
            }
            public override void append(Saxon.Hej.om.Item item) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            }
            public override void close() {
                if (!(hasFailed())) {
                    Saxon.Hej.@event.Outputter @out = getNextOutputter();
                    foreach (Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer writer in writers) {
                        writer.replay(@out);
                    }
                    @out.close();
                }
                writers = null;
            }
        
        }
    
    }
}