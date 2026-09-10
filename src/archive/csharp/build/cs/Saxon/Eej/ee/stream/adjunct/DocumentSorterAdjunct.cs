using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class DocumentSorterAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.sort.DocumentSorter exp = (Saxon.Hej.expr.sort.DocumentSorter)(getExpression());
            Saxon.Hej.expr.Expression @base = exp.getBaseExpression();
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(@base, contextInfo, reasons);
            if (((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (ps.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING))) {
                Saxon.Hej.pattern.Pattern p = toStreamingPattern(getConfiguration());
                if ((p == null)) {
                    reasons.Add(needsSorting());
                    reasons.Add((("The expression "+@base)+" cannot be used as a scanning expression, because it is not valid as a pattern"));
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
                if (p.isMotionless()) {
                    @base.setExtraProperty("scanningPattern", p);
                    return ps;
                } else {
                    reasons.Add(needsSorting());
                    reasons.Add((("The expression "+exp.getBaseExpression())+" cannot be used as a scanning expression, because as a pattern, it is not motionless"));
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            }
            if ((((((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.MOTIONLESS) || (ps.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) || (ps.getPosture() == Saxon.Eej.ee.stream.Posture.CLIMBING)) || (ps.getPosture() == Saxon.Eej.ee.stream.Posture.STRIDING)) || (ps.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING))) {
                return ps;
            } else {
                if ((reasons != null)) {
                    reasons.Add((("Expression "+exp.getBaseExpression().toShortString())+" requires sorting nodes into document order"));
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        private string needsSorting() {
            return (("Expression "+((Saxon.Hej.expr.sort.DocumentSorter)(getExpression())).getBaseExpression().toShortString())+" requires sorting nodes into document order");
        }
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(((Saxon.Hej.expr.sort.DocumentSorter)(getExpression())).getBaseExpression(), config);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression @base = ((Saxon.Hej.expr.sort.DocumentSorter)(getExpression())).getBaseExpression();
            if ((@base.getExtraProperty("scanningPattern") != null)) {
                return /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), @base).getWatchMaker(false);
            } else {
                return null;
            }
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(((Saxon.Hej.expr.sort.DocumentSorter)(getExpression())).getBaseExpression()) == Saxon.Eej.ee.stream.Posture.STRIDING)) {
                return new Saxon.Eej.ee.stream.adjunct.DocumentSorterAdjunct.MixedPostureSorter(@out, context);
            } else {
                return new Saxon.Eej.ee.stream.adjunct.DocumentSorterAdjunct.AncestorArrangingFeed(@out, context);
            }
        }
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : base.getFeedMaker(arg));
        }
        private class AncestorArrangingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            public Saxon.Hej.z.IntHashMap<Saxon.Hej.om.NodeInfo> mostRecentAtDepth = new Saxon.Hej.z.IntHashMap<Saxon.Hej.om.NodeInfo>();
            public System.Collections.Generic.Stack<Saxon.Hej.om.NodeInfo> stack = new System.Collections.Generic.Stack<Saxon.Hej.om.NodeInfo>();
            public AncestorArrangingFeed(Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (@out, context) {
            }
            public override void append(Saxon.Hej.om.Item item) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                if ((!((stack.Count==0)) && Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(node, stack.Peek()))) {
                    stack.Push(node);
                } else {
                    flush();
                    stack = new System.Collections.Generic.Stack<Saxon.Hej.om.NodeInfo>();
                    stack.Push(node);
                }
            }
            public override void close() {
                flush();
                base.close();
            }
            private void flush() {
                Saxon.Hej.@event.Outputter @out = getNextOutputter();
                while (!((stack.Count==0))) {
                    Saxon.Hej.om.NodeInfo n = stack.Pop();
                    int depth = depthOfNode(n);
                    Saxon.Hej.om.NodeInfo prev = mostRecentAtDepth.get(depth);
                    if (((prev == null) || (prev != n))) {
                        @out.append(n);
                        mostRecentAtDepth.put(depth, n);
                    }
                }
            }
            private int depthOfNode(Saxon.Hej.om.NodeInfo n) {
                int depth = 0;
                while (((n = n.getParent()) != null)) {
                    depth++;
                }
                return depth;
            }
        
        }
        private class MixedPostureSorter : Saxon.Eej.ee.stream.feed.ItemFeed {
            public System.Collections.Generic.IList<Saxon.Hej.om.Item> groundedItems = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            public MixedPostureSorter(Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (@out, context) {
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((item is Saxon.Eej.ee.stream.om.FleetingNode)) {
                    getNextOutputter().append(item);
                } else {
                    groundedItems.Add(item);
                }
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                getResultFeed().endSelectedParentNode(locationId);
            }
            public override void close() {
                Saxon.Hej.om.SequenceIterator grounded = new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(groundedItems);
                Saxon.Hej.om.SequenceIterator sorted = new Saxon.Hej.expr.sort.DocumentOrderIterator(grounded, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                Saxon.Hej.@event.Outputter @out = getNextOutputter();
                Saxon.Hej.om.SequenceTool.supply(sorted, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(it => @out.append(it) /* converted from instance method reference */));
                base.close();
            }
        
        }
    
    }
}