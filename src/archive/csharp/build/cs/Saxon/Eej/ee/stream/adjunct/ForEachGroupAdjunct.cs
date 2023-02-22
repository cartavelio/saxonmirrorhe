using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ForEachGroupAdjunct : Saxon.Eej.ee.stream.adjunct.ForEachAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.ForEachGroup exp = (Saxon.Hej.expr.instruct.ForEachGroup)(getExpression());
            Saxon.Hej.expr.Expression select = exp.getSelectExpression();
            Saxon.Hej.expr.Expression action = exp.getActionExpression();
            Saxon.Hej.expr.Expression key = exp.getGroupingKey();
            int algorithm = exp.getAlgorithm();
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(select, contextInfo, reasons);
            if ((ps.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return base.computeStreamability(contextInfo, reasons);
            }
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE info2 = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(select.getItemType(), false, ps.getPosture());
            if ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY)) {
                if (exp.isInFork()) {
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(key, info2, null).getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                        if ((reasons != null)) {
                            reasons.Add("The xsl:for-each-group/@group-by expression is not motionless");
                        }
                        return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                    }
                } else {
                    if ((reasons != null)) {
                        reasons.Add("The xsl:for-each-group instruction has an @group-by attribute and is not within xsl:fork");
                    }
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            }
            if ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ADJACENT)) {
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(key, info2, null).getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                    if ((reasons != null)) {
                        reasons.Add("The xsl:for-each-group/@group-adjacent expression is not motionless");
                    }
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            }
            if (((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_STARTING) || (algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ENDING))) {
                Saxon.Hej.pattern.Pattern pattern = (Saxon.Hej.pattern.Pattern)(key);
                if (!(pattern.isMotionless())) {
                    if ((reasons != null)) {
                        string s = ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_STARTING) ? "starting-with" : "ending-with");
                        reasons.Add((("The xsl:for-each-group/@group-"+s)+" pattern is not motionless"));
                    }
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            }
            if ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BREAKING_WHEN)) {
                reasons.Add("Streaming is not implemented for xsl:for-each-group/@break-when");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (((exp.getSortKeyDefinitions() != null) || (select is Saxon.Hej.expr.sort.SortExpression))) {
                if ((reasons != null)) {
                    reasons.Add("Groups cannot be sorted when streaming");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(action, info2, reasons);
            if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select) == Saxon.Eej.ee.stream.Posture.CRAWLING) && (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING))) {
                if ((reasons != null)) {
                    reasons.Add("The xsl:for-each-group/@select expression has crawling posture (that is, it can select overlapping nodes)");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            return new Saxon.Eej.ee.stream.PostureAndSweep(/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(action), /*134*/Saxon.Eej.ee.stream.Streamability.wider(/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(select), /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action)));
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression())).getSelectExpression();
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select) == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return null;
            }
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, config, null);
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(config, select);
            Saxon.Eej.ee.stream.Inversion actionInversion = getActionInversion(true);
            if ((((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression())).getAlgorithm() == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction actor = new Saxon.Eej.ee.stream.watch.ForEachGroupParallelAction(watchManager, (Saxon.Hej.expr.instruct.ForEachGroup)(getExpression()), @out, context);
                    if ((actionInversion != null)) {
                        actor.setInversion(actionInversion);
                    }
                    if ((selection != null)) {
                        return new Saxon.Eej.ee.stream.watch.Trigger(selection, actor, context);
                    } else {
                        return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(sa)).makeTransmissionFlow(watchManager, select, actor, context);
                    }
                } ;
            } else {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.watch.ForEachGroupPartitionAction actor = new Saxon.Eej.ee.stream.watch.ForEachGroupPartitionAction(watchManager, (Saxon.Hej.expr.instruct.ForEachGroup)(getExpression()), @out, context);
                    if ((actionInversion != null)) {
                        actor.setInversion(actionInversion);
                    }
                    if ((selection != null)) {
                        return new Saxon.Eej.ee.stream.watch.Trigger(selection, actor, context);
                    } else {
                        return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(sa)).makeTransmissionFlow(watchManager, select, actor, context);
                    }
                } ;
            }
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.ForEachGroup instruction = (Saxon.Hej.expr.instruct.ForEachGroup)(getExpression());
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(instruction.getSelectExpression()) == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                if ((instruction.getAlgorithm() == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY)) {
                    return new Saxon.Eej.ee.stream.adjunct.ForEachGroupAdjunct.GroupedGroundedDistributedFeed(instruction, @out, context);
                } else {
                    Saxon.Hej.expr.XPathContext actionContext = context.newMinorContext();
                    Saxon.Hej.tree.iter.ManualIterator actionIter = new Saxon.Hej.tree.iter.ManualIterator();
                    actionContext.setCurrentIterator(actionIter);
                    return new Saxon.Eej.ee.stream.adjunct.ForEachGroupAdjunct.GroupedGroundedFeed(instruction, @out, actionContext);
                }
            } else {
                Saxon.Eej.ee.stream.Inversion actionInversion = getActionInversion(true);
                Saxon.Eej.ee.stream.watch.ForEachGroupPartitionAction actor = new Saxon.Eej.ee.stream.watch.ForEachGroupPartitionAction(watchManager, (Saxon.Hej.expr.instruct.ForEachGroup)(getExpression()), @out, context);
                if ((actionInversion != null)) {
                    actor.setInversion(actionInversion);
                }
                return actor;
            }
        }
        public static Saxon.Eej.ee.stream.adjunct.GroupBoundaryTester makeBoundaryTester(Saxon.Hej.expr.instruct.ForEachGroup instruction, Saxon.Hej.expr.XPathContext populationContext) {
            int algorithm = instruction.getAlgorithm();
            if ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ADJACENT)) {
                return new Saxon.Eej.ee.stream.adjunct.GroupAdjacentTester(instruction, populationContext);
            } else {
                if ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_STARTING)) {
                    return new Saxon.Eej.ee.stream.adjunct.GroupStartingTester(instruction, populationContext);
                } else {
                    if ((algorithm == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ENDING)) {
                        return new Saxon.Eej.ee.stream.adjunct.GroupEndingTester(instruction, populationContext);
                    } else {
                        throw new Saxon.Ejava.lang.AssertionError("Unsupported grouping algorithm for partitioned streaming");
                    }
                }
            }
        }
        private class GroupedGroundedFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> currentGroup = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            private int groupCounter = 0;
            private readonly Saxon.Eej.ee.stream.adjunct.GroupBoundaryTester boundaryTester;
            private Saxon.Hej.om.AtomicSequence currentGroupingKey;
            public GroupedGroundedFeed(Saxon.Hej.expr.instruct.ForEachGroup instruction, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (instruction, result, context) {
                boundaryTester = makeBoundaryTester(instruction, context);
            }
            public override void append(Saxon.Hej.om.Item item) {
                Saxon.Hej.tree.iter.ManualIterator popIter = (Saxon.Hej.tree.iter.ManualIterator)(getContext().getCurrentIterator());
                popIter.setContextItem(item);
                popIter.incrementPosition();
                bool startNewGroup = boundaryTester.notifyItem();
                if (startNewGroup) {
                    processCompletedGroup();
                    currentGroup.Clear();
                    groupCounter++;
                    if ((boundaryTester is Saxon.Eej.ee.stream.adjunct.GroupAdjacentTester)) {
                        currentGroupingKey = ((Saxon.Eej.ee.stream.adjunct.GroupAdjacentTester)(boundaryTester)).getCurrentGroupingKey();
                    }
                }
                currentGroup.Add(item);
            }
            public override void close() {
                processCompletedGroup();
                base.close();
            }
            private void processCompletedGroup() {
                if (!(currentGroup.Count==0)) {
                    Saxon.Hej.expr.XPathContextMajor c2 = getContext().newContext();
                    Saxon.Hej.tree.iter.ManualIterator popIter = new Saxon.Hej.tree.iter.ManualIterator(currentGroup[0], groupCounter);
                    c2.setCurrentIterator(popIter);
                    Saxon.Eej.ee.stream.ManualGroupIterator gi = new Saxon.Eej.ee.stream.ManualGroupIterator(currentGroup[0], 1);
                    gi.setCurrentGroup(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(currentGroup));
                    gi.setCurrentGroupingKey(currentGroupingKey);
                    c2.setCurrentGroupIterator(gi);
                    ((Saxon.Hej.expr.instruct.ForEachGroup)(getExpression())).getActionExpression().process(getNextOutputter(), c2);
                }
            }
        
        }
        private class GroupedGroundedDistributedFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> population = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            public GroupedGroundedDistributedFeed(Saxon.Hej.expr.instruct.ForEachGroup instruction, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (instruction, result, context) {
            }
            public override void append(Saxon.Hej.om.Item item) {
                population.Add(item);
            }
            public override void close() {
                Saxon.Hej.expr.instruct.ForEachGroup inst = (Saxon.Hej.expr.instruct.ForEachGroup)(getExpression());
                Saxon.Hej.expr.Literal pop = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(population), inst);
                Saxon.Hej.expr.sort.GroupIterator groupIterator = inst.getGroupIterator(context => pop.iterate(), getContext());
                Saxon.Hej.expr.XPathContextMajor c2 = getContext().newContext();
                Saxon.Hej.om.FocusIterator focusIterator = c2.trackFocus(groupIterator);
                c2.setCurrentGroupIterator(groupIterator);
                c2.setCurrentTemplateRule(null);
                Saxon.Hej.expr.elab.PushEvaluator action = inst.getActionExpression().makeElaborator().elaborateForPush();
                Saxon.Hej.@event.Outputter output = getNextOutputter();
                while ((focusIterator.next() != null)) {
                    Saxon.Hej.expr.Expression.dispatchTailCall(/*Delegate.processLeavingTail()*/action(output, c2));
                }
                base.close();
            }
        
        }
    
    }
}