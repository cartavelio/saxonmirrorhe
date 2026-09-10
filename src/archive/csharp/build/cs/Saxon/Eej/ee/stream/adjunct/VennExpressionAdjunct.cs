using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class VennExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        private Saxon.Eej.ee.stream.Inversion lhsInversion;
        private Saxon.Eej.ee.stream.Inversion rhsInversion;
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
            Saxon.Eej.ee.stream.Posture p0 = ps0.getPosture();
            Saxon.Eej.ee.stream.Posture p1 = ps1.getPosture();
            if (((ps0.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) || (ps1.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING))) {
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (ps0.Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                return ps1;
            }
            if (ps1.Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)) {
                return ps0;
            }
            if ((p0.Equals(Saxon.Eej.ee.stream.Posture.CLIMBING) && p1.Equals(Saxon.Eej.ee.stream.Posture.CLIMBING))) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(p0, /*134*/Saxon.Eej.ee.stream.Streamability.wider(ps0.getSweep(), ps1.getSweep()));
            }
            if (((((p0 == Saxon.Eej.ee.stream.Posture.STRIDING) || (p0 == Saxon.Eej.ee.stream.Posture.CRAWLING))) && (((p1 == Saxon.Eej.ee.stream.Posture.STRIDING) || (p1 == Saxon.Eej.ee.stream.Posture.CRAWLING))))) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CRAWLING, /*134*/Saxon.Eej.ee.stream.Streamability.wider(ps0.getSweep(), ps1.getSweep()));
            }
            if ((reasons != null)) {
                reasons.Add((((((((("Operands of "+Saxon.Hej.expr.parser.Token.tokens[exp.getOperator()])+" expression on line ")+exp.getLocation().getLineNumber())+" have mixed posture (")+p0)+" | ")+p1)+")"));
            }
            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.VennExpression exp = (Saxon.Hej.expr.VennExpression)(getExpression());
            Saxon.Eej.ee.stream.Sweep s0 = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp.getLhsExpression());
            Saxon.Eej.ee.stream.Sweep s1 = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp.getRhsExpression());
            bool bothConsuming = ((s0 == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (s1 == Saxon.Eej.ee.stream.Sweep.CONSUMING));
            if (bothConsuming) {
                if ((lhsInversion == null)) {
                    lhsInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(exp.getLhsExpression(), forGrouping);
                    rhsInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(exp.getRhsExpression(), forGrouping);
                }
                return (watchManager, @out, context) => new Saxon.Eej.ee.stream.watch.Trigger(Saxon.Hej.pattern.AnchorPattern.getInstance(), new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.VennTwoWayFeed(exp, this, @out, context, watchManager), context);
            } else {
                return base.getWatchMaker(forGrouping);
            }
        }
        private class LabellingFeed : Saxon.Eej.ee.stream.feed.NoCloseFeed {
            private readonly int label;
            public LabellingFeed(Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed resultFeed, Saxon.Hej.expr.XPathContext context, int label) : base (resultFeed, context) {
                this.label = label;
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                ((Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed)(getResultFeed())).setLabel(label);
                return base.startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                ((Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed)(getResultFeed())).setLabel(label);
                base.endSelectedParentNode(locationId);
            }
            public override void append(Saxon.Hej.om.Item item) {
                ((Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed)(getResultFeed())).setLabel(label);
                getResultFeed().append(item);
            }
        
        }
        private class VennTwoWayFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct streamer;
            private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            private Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed combiner;
            internal VennTwoWayFeed(Saxon.Hej.expr.VennExpression exp, Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct streamer, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.watch.WatchManager watchManager) : base (exp, result, context) {
                this.streamer = streamer;
                this.watchManager = watchManager;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.VennExpression exp = ((Saxon.Hej.expr.VennExpression)(getExpression()));
                switch (exp.getOperator()) {
                  case Saxon.Hej.expr.parser.Token.UNION:
                    combiner = new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningUnionFeed(exp, getResultFeed(), getContext());
                    break ;
                  case Saxon.Hej.expr.parser.Token.INTERSECT:
                    combiner = new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningIntersectFeed(exp, getResultFeed(), getContext());
                    break ;
                  case Saxon.Hej.expr.parser.Token.EXCEPT:
                    combiner = new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningExceptFeed(exp, getResultFeed(), getContext(), watchManager);
                    break ;
                  default:
                    combiner = null;
                    break ;
                }
                watchManager.addWatch(streamer.lhsInversion.getWatch(watchManager, new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.LabellingFeed(combiner, getContext(), 0), getContext()), true);
                watchManager.addWatch(streamer.rhsInversion.getWatch(watchManager, new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.LabellingFeed(combiner, getContext(), 1), getContext()), true);
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return null;
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            }
            public override void append(Saxon.Hej.om.Item item) {
            }
            public override void close() {
                combiner.close();
            }
        
        }
        abstract class CombiningFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            protected internal int currentLabel;
            public CombiningFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            internal void setLabel(int label) {
                currentLabel = label;
            }
        
        }
        private class CombiningUnionFeed : Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed {
            private Saxon.Hej.om.NodeInfo previousNode = null;
            private readonly System.Collections.Generic.Stack<bool> duplicates = new System.Collections.Generic.Stack<bool>();
            private readonly System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> lhsBuffer = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            private readonly System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> rhsBuffer = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            internal CombiningUnionFeed(Saxon.Hej.expr.VennExpression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                bool duplicate = ((previousNode != null) && node.isSameNodeInfo(previousNode));
                if (!(duplicate)) {
                    getResultFeed().startSelectedParentNode(node, locationId);
                    previousNode = node;
                }
                duplicates.Push(duplicate);
                return null;
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                bool duplicate = duplicates.Pop();
                if (!(duplicate)) {
                    getResultFeed().endSelectedParentNode(locationId);
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> thisBuffer = ((currentLabel == 0) ? lhsBuffer : rhsBuffer);
                System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> thatBuffer = ((currentLabel == 0) ? rhsBuffer : lhsBuffer);
                while (!(thatBuffer.Count==0)) {
                    Saxon.Hej.om.NodeInfo next = thatBuffer.First.Value;
                    int c = next.compareOrder((Saxon.Hej.om.NodeInfo)(item));
                    if ((c<0)) {
                        getResultFeed().append(Saxon.Impl.Helpers.LinkedListUtils.PollFirst(thatBuffer));
                    } else {
                        if ((c == 0)) {
                            Saxon.Impl.Helpers.LinkedListUtils.PollFirst(thatBuffer);
                        } else {
                            break ;
                        }
                    }
                }
                thisBuffer.AddLast((Saxon.Hej.om.NodeInfo)(item));
            }
            public override void close() {
                Saxon.Hej.expr.UnionEnumeration union = new Saxon.Hej.expr.UnionEnumeration(linkedListIterator(lhsBuffer), linkedListIterator(rhsBuffer), Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
                processItems(union, getResultFeed());
                base.close();
            }
        
        }
        private class CombiningIntersectFeed : Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed {
            private Saxon.Hej.om.NodeInfo previousNode = null;
            private readonly System.Collections.Generic.Stack<bool> duplicates = new System.Collections.Generic.Stack<bool>();
            private readonly System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> lhsBuffer = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            private readonly System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> rhsBuffer = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            internal CombiningIntersectFeed(Saxon.Hej.expr.VennExpression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                bool duplicate = ((previousNode != null) && node.isSameNodeInfo(previousNode));
                previousNode = node;
                duplicates.Push(duplicate);
                if (duplicate) {
                    return getResultFeed().startSelectedParentNode(node, locationId);
                } else {
                    return null;
                }
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                bool duplicate = duplicates.Pop();
                if (duplicate) {
                    getResultFeed().endSelectedParentNode(locationId);
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> thisBuffer = ((currentLabel == 0) ? lhsBuffer : rhsBuffer);
                System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> thatBuffer = ((currentLabel == 0) ? rhsBuffer : lhsBuffer);
                while (!(thatBuffer.Count==0)) {
                    Saxon.Hej.om.NodeInfo next = thatBuffer.First.Value;
                    int c = next.compareOrder((Saxon.Hej.om.NodeInfo)(item));
                    if ((c<0)) {
                        Saxon.Impl.Helpers.LinkedListUtils.PollFirst(thatBuffer);
                    } else {
                        if ((c == 0)) {
                            getResultFeed().append(item);
                            Saxon.Impl.Helpers.LinkedListUtils.PollFirst(thatBuffer);
                        } else {
                            break ;
                        }
                    }
                }
                thisBuffer.AddLast((Saxon.Hej.om.NodeInfo)(item));
            }
            public override void close() {
                Saxon.Hej.expr.IntersectionIterator union = new Saxon.Hej.expr.IntersectionIterator(linkedListIterator(lhsBuffer), linkedListIterator(rhsBuffer), Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
                processItems(union, getResultFeed());
                base.close();
            }
        
        }
        private class CombiningExceptFeed : Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.CombiningFeed {
            private readonly System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> lhsBuffer = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            private readonly System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> rhsBuffer = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            public Saxon.Hej.om.NodeInfo latestRhsNode = null;
            public bool emitted;
            public readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            internal CombiningExceptFeed(Saxon.Hej.expr.VennExpression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.watch.WatchManager watchManager) : base (exp, result, context) {
                this.watchManager = watchManager;
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location location) {
                if ((currentLabel == 0)) {
                    Saxon.Eej.ee.stream.watch.Watch lastChanceWatch = new Inner_AbstractWatch_42(this, node);
                    watchManager.addWatch(lastChanceWatch, true);
                } else {
                    latestRhsNode = node;
                }
                return null;
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                if (((currentLabel == 0) && emitted)) {
                    getResultFeed().endSelectedParentNode(locationId);
                    emitted = false;
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((currentLabel == 0)) {
                    bool exclude = false;
                    while (!(rhsBuffer.Count==0)) {
                        Saxon.Hej.om.NodeInfo next = rhsBuffer.First.Value;
                        int c = next.compareOrder((Saxon.Hej.om.NodeInfo)(item));
                        if ((c<0)) {
                            Saxon.Impl.Helpers.LinkedListUtils.PollFirst(rhsBuffer);
                        } else {
                            if ((c == 0)) {
                                exclude = true;
                                break ;
                            } else {
                                break ;
                            }
                        }
                    }
                    if (!(exclude)) {
                        lhsBuffer.AddLast((Saxon.Hej.om.NodeInfo)(item));
                    }
                } else {
                    while (!(lhsBuffer.Count==0)) {
                        Saxon.Hej.om.NodeInfo next = lhsBuffer.First.Value;
                        int c = next.compareOrder((Saxon.Hej.om.NodeInfo)(item));
                        if ((c<0)) {
                            getResultFeed().append(Saxon.Impl.Helpers.LinkedListUtils.PollFirst(lhsBuffer));
                        } else {
                            if ((c == 0)) {
                                Saxon.Impl.Helpers.LinkedListUtils.PollFirst(lhsBuffer);
                                break ;
                            } else {
                                break ;
                            }
                        }
                    }
                    rhsBuffer.AddLast((Saxon.Hej.om.NodeInfo)(item));
                }
            }
            public override void close() {
                Saxon.Hej.expr.DifferenceIterator union = new Saxon.Hej.expr.DifferenceIterator(linkedListIterator(lhsBuffer), linkedListIterator(rhsBuffer), Saxon.Hej.expr.sort.LocalOrderComparer.getInstance());
                processItems(union, getResultFeed());
                base.close();
            }
        
            private class Inner_AbstractWatch_42 : Saxon.Eej.ee.stream.watch.AbstractWatch {
                private readonly CombiningExceptFeed outerInstance;
                public Inner_AbstractWatch_42(CombiningExceptFeed outerInstance, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
                    this.outerInstance = outerInstance;
                    this.node = node;
                }
                private Saxon.Eej.ee.stream.om.FleetingParentNode node;
                public override  /* REPLACING public */ bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.expr.XPathContext context) {
                    return true;
                }
                public override  /* REPLACING public */ bool matchesNodesOfKind(Saxon.Hej.type.UType kind) {
                    return true;
                }
                public override  /* REPLACING public */ void setAnchorNode(Saxon.Eej.ee.stream.om.FleetingParentNode anchor) {
                }
                public override  /* REPLACING public */ Saxon.Eej.ee.stream.om.FleetingParentNode getAnchorNode() {
                    return /*extraParam*/node;
                }
                public override  /* REPLACING public override */ Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                    outerInstance.emitted = false;
                    outerInstance.watchManager.removeWatch(this);
                    if ((/*localparam*/node != outerInstance.latestRhsNode)) {
                        outerInstance.emitted = true;
                        return /*guess*/outerInstance.getResultFeed().startSelectedParentNode(/*localparam*/node, /*localparam*/locationId);
                    }
                    return null;
                }
                public override  /* REPLACING public override */ void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                }
            }

        }
        private static Saxon.Hej.om.SequenceIterator linkedListIterator(System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> nodes) {
            return Saxon.Impl.Helpers.LinkedListUtils.GetSequenceIterator(nodes);
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.VennExpression exp = (Saxon.Hej.expr.VennExpression)(getExpression());
            Saxon.Eej.ee.stream.Sweep s0 = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp.getLhsExpression());
            Saxon.Eej.ee.stream.Sweep s1 = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp.getRhsExpression());
            bool bothConsuming = ((s0 == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (s1 == Saxon.Eej.ee.stream.Sweep.CONSUMING));
            bool a0consuming = (s0 == Saxon.Eej.ee.stream.Sweep.CONSUMING);
            if (bothConsuming) {
                if ((lhsInversion == null)) {
                    lhsInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(exp.getLhsExpression(), false);
                    rhsInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(exp.getRhsExpression(), false);
                }
                return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.VennTwoWayFeed(exp, this, @out, context, watchManager);
            }
            int @operator = exp.getOperator();
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                if (a0consuming) {
                    return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.SemiStreamedUnionFeed(exp, exp.getRhsExpression(), @out, context);
                } else {
                    return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.SemiStreamedUnionFeed(exp, exp.getLhsExpression(), @out, context);
                }
            } else {
                if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                    return new Saxon.Eej.ee.stream.feed.FilteringFeed(watchManager, @out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.OpaqueFilter);
                } else {
                    if ((@operator == Saxon.Hej.expr.parser.Token.EXCEPT)) {
                        if (a0consuming) {
                            return new Saxon.Eej.ee.stream.feed.FilteringFeed(watchManager, @out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.TransparentFilter);
                        } else {
                            return new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct.FirstOperandOnlyFeed(watchManager, @out, context, exp.getLhsExpression());
                        }
                    } else {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException();
                    }
                }
            }
        }
        private class FirstOperandOnlyFeed : Saxon.Eej.ee.stream.feed.FilteringFeed {
            internal Saxon.Hej.expr.Expression lhs;
            public FirstOperandOnlyFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Expression lhs) : base (watchManager, @out, context, Saxon.Eej.ee.stream.feed.FilteringFeed.OpaqueFilter) {
                this.lhs = lhs;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                processItems(lhs.iterate(getContext()), getNextOutputter());
            }
        
        }
        private class SemiStreamedUnionFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal Saxon.Hej.expr.Expression nonConsumingOperand;
            internal Saxon.Hej.om.SequenceIterator nonConsumingIterator;
            internal bool atStart = true;
            internal bool emitNonConsumingOperandAtEnd = true;
            public SemiStreamedUnionFeed(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Expression nonConsumingOperand, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
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
    
        private class Inner_AbstractWatch_42 : Saxon.Eej.ee.stream.watch.AbstractWatch {
            private readonly CombiningExceptFeed outerInstance;
            public Inner_AbstractWatch_42(CombiningExceptFeed outerInstance, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
                this.outerInstance = outerInstance;
                this.node = node;
            }
            private Saxon.Eej.ee.stream.om.FleetingParentNode node;
            public override  /* REPLACING public */ bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.expr.XPathContext context) {
                return true;
            }
            public override  /* REPLACING public */ bool matchesNodesOfKind(Saxon.Hej.type.UType kind) {
                return true;
            }
            public override  /* REPLACING public */ void setAnchorNode(Saxon.Eej.ee.stream.om.FleetingParentNode anchor) {
            }
            public override  /* REPLACING public */ Saxon.Eej.ee.stream.om.FleetingParentNode getAnchorNode() {
                return /*extraParam*/node;
            }
            public override  /* REPLACING public override */ Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                outerInstance.emitted = false;
                outerInstance.watchManager.removeWatch(this);
                if ((/*localparam*/node != outerInstance.latestRhsNode)) {
                    outerInstance.emitted = true;
                    return /*guess*/outerInstance.getResultFeed().startSelectedParentNode(/*localparam*/node, /*localparam*/locationId);
                }
                return null;
            }
            public override  /* REPLACING public override */ void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            }
        }

    }
}