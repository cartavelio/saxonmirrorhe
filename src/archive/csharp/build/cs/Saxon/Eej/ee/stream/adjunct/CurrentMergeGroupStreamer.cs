using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CurrentMergeGroupStreamer : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return Saxon.Hej.pattern.AnchorPattern.getInstance();
        }
        public override Saxon.Eej.ee.stream.watch.Trigger makeTransmissionFlow(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.pattern.Pattern selection = Saxon.Hej.pattern.AnchorPattern.getInstance();
            return new Saxon.Eej.ee.stream.watch.Trigger(selection, makeItemFeed(watchManager, @out, context), context);
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(getExpression()) == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return new Saxon.Eej.ee.stream.adjunct.CurrentMergeGroupStreamer.GroupingFeed(@out, context);
            } else {
                return @out;
            }
        }
        private class GroupingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private System.Collections.Generic.IList<Saxon.Hej.om.Item> group;
            public GroupingFeed(Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (@out, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                group = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            }
            public override void append(Saxon.Hej.om.Item item) {
                group.Add(item);
            }
            public override void close() {
                getResultFeed().open(getTerminator());
                getNextOutputter().append(group[0]);
                getNextOutputter().close();
            }
        
        }
    
    }
}