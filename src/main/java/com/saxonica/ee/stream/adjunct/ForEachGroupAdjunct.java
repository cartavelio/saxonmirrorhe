package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.*;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.expr.sort.GroupIterator;
import net.sf.saxon.expr.sort.SortExpression;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * Compile an xsl:for-each-group instruction to streaming code.
 */

public class ForEachGroupAdjunct extends ForEachAdjunct  {

    /**
     * Get the "sweep" of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo      whatever is known statically about the context item
     * @param reasons          return parameter to hold reasons for non-streamability  @return the posture and sweep
     */

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        ForEachGroup exp = (ForEachGroup)getExpression();
        Expression select = exp.getSelectExpression();
        Expression action = exp.getActionExpression();
        Expression key = exp.getGroupingKey();
        int algorithm = exp.getAlgorithm();
        PostureAndSweep ps = Streamability.getStreamability(select, contextInfo, reasons);

//        From the spec (15 Jan 2015)

//        If the select expression is grounded, then the posture and sweep of the xsl:for-each-group
//        instruction follow the general streamability rules, with the operand roles and their usages
//        as follows:
//        The select expression(the operand usage is irrelevant, but can be taken as inspection)
//        The collation attribute value template (usage absorption)
//        Any attribute value templates appearing in attributes of a child xsl:sort instruction (usage absorption)
//        The group-by or group-adjacent expression, assessed with a context posture of grounded (usage absorption).
//        The select expression or contained sequence constructor of any xsl:sort children, assessed with a
//        context posture of grounded (usage absorption).
//        The group-starting-with or group-ending-with patterns if present;
//        these are higher-order operands with usage inspection.

        if (ps.getPosture() == Posture.GROUNDED) {
            return super.computeStreamability(contextInfo, reasons);
        }

//        If there is a group-by attribute and the instruction is not a child of xsl:fork, then roaming and
//        free-ranging.

//        If there is a group-by or group-adjacent attribute that is not motionless, then roaming and
//        free-ranging.

//        If there is an xsl:sort child element, then roaming and free - ranging.
//        If the posture of the select expression is crawling and the sweep of the contained sequence constructor
//        is consuming, then roaming and free-ranging.
//            Otherwise:
//        The posture of the instruction is the posture of the contained sequence constructor, assessed with the
//        context posture and context item type set to the posture and type of the select expression.
//        The sweep of the instruction is the wider of the sweeps of the select expression and the contained
//        sequence constructor, where the ordering of increasing width is motionless, consuming, free - ranging.
//

        ContextItemStaticInfoEE info2 =
                new ContextItemStaticInfoEE(select.getItemType(), false, ps.getPosture());

        if (algorithm == ForEachGroup.GROUP_BY) {
            if (exp.isInFork()) {
                if (Streamability.getStreamability(key, info2, null).getSweep() != Sweep.MOTIONLESS) {
                    if (reasons != null) {
                        reasons.add("The xsl:for-each-group/@group-by expression is not motionless");
                    }
                    return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }

            } else {
                if (reasons != null) {
                    reasons.add("The xsl:for-each-group instruction has an @group-by attribute and is not within xsl:fork");
                }
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        if (algorithm == ForEachGroup.GROUP_ADJACENT) {
            if (Streamability.getStreamability(key, info2, null).getSweep() != Sweep.MOTIONLESS) {
                if (reasons != null) {
                    reasons.add("The xsl:for-each-group/@group-adjacent expression is not motionless");
                }
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }

        }
        if (algorithm == ForEachGroup.GROUP_STARTING || algorithm == ForEachGroup.GROUP_ENDING) {
            Pattern pattern = (Pattern)key;
            if (!pattern.isMotionless()) {
                if (reasons != null) {
                    String s = algorithm == ForEachGroup.GROUP_STARTING ? "starting-with" : "ending-with";
                    reasons.add("The xsl:for-each-group/@group-" + s + " pattern is not motionless");
                }
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        }
        if (algorithm == ForEachGroup.GROUP_BREAKING_WHEN) {
            reasons.add("Streaming is not implemented for xsl:for-each-group/@break-when");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        if (exp.getSortKeyDefinitions() != null || select instanceof SortExpression) {
            if (reasons != null) {
                reasons.add("Groups cannot be sorted when streaming");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        Streamability.getStreamability(action, info2, reasons);

        if (Streamability.getPosture(select) == Posture.CRAWLING && Streamability.getSweep(action) == Sweep.CONSUMING) {
            if (reasons != null) {
                reasons.add("The xsl:for-each-group/@select expression has crawling posture (that is, it can select overlapping nodes)");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }
        return new PostureAndSweep(Streamability.getPosture(action),
                                   Streamability.wider(Streamability.getSweep(select), Streamability.getSweep(action)));
    }


    /*
    The general approach to streaming of xsl:for-each-group is as follows:

    (a) The select expression of the for-each-group instruction determines the
    pattern registered with the WatchManager.

    (b) The events fired when this pattern is matched go first to the Feed representing
    the current-group() call, and then filter their way up the expression tree.

    (c) When a group starts or ends, the open() and close() events for the outer expression are
    called; these calls are suppressed at inner levels.
     */

    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {

        final Configuration config = getConfiguration();
        final Expression select = ((ForEachGroup) getExpression()).getSelectExpression();

        // If the select expression is grounded (but consuming), return null; so we end up with a Feed rather than a Watch.
        if (Streamability.getPosture(select) == Posture.GROUNDED /*|| action.getSweep() != Sweep.CONSUMING*/) {
            return null;
        }

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, config, null);
        final StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(config, select);
        final Inversion actionInversion = getActionInversion(true);

        if (((ForEachGroup) getExpression()).getAlgorithm() == ForEachGroup.GROUP_BY) {
            return (watchManager, out, context) -> {
                ForEachGroupParallelAction actor = new ForEachGroupParallelAction(watchManager, (ForEachGroup) getExpression(), out, context);
                if (actionInversion != null) {
                    actor.setInversion(actionInversion);
                }
                if (selection != null) {
                    return new Trigger(selection, actor, context);
                } else {
                    return ((TransmissionAdjunct) sa).makeTransmissionFlow(watchManager, select, actor, context);
                }
            };
        } else {
            return (watchManager, out, context) -> {
                ForEachGroupPartitionAction actor = new ForEachGroupPartitionAction(watchManager, (ForEachGroup) getExpression(), out, context);
                if (actionInversion != null) {
                    actor.setInversion(actionInversion);
                }
                if (selection != null) {
                    return new Trigger(selection, actor, context);
                } else {
                    return ((TransmissionAdjunct) sa).makeTransmissionFlow(watchManager, select, actor, context);
                }
            };
        }


    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        ForEachGroup instruction = (ForEachGroup)getExpression();
        if (Streamability.getPosture(instruction.getSelectExpression()) == Posture.GROUNDED) {
            if (instruction.getAlgorithm() == ForEachGroup.GROUP_BY) {
                return new GroupedGroundedDistributedFeed(instruction, out, context);
            } else {
                XPathContext actionContext = context.newMinorContext();
                ManualIterator actionIter = new ManualIterator();
                actionContext.setCurrentIterator(actionIter);
                return new GroupedGroundedFeed(instruction, out, actionContext);
            }
        } else {
            final Inversion actionInversion = getActionInversion(true);
            ForEachGroupPartitionAction actor = new ForEachGroupPartitionAction(watchManager, (ForEachGroup)getExpression(), out, context);
            if (actionInversion != null) {
                actor.setInversion(actionInversion);
            }
            return actor;
        }
    }

    public static GroupBoundaryTester makeBoundaryTester(ForEachGroup instruction, XPathContext populationContext) {
        final int algorithm = instruction.getAlgorithm();
        if (algorithm == ForEachGroup.GROUP_ADJACENT) {
            return new GroupAdjacentTester(instruction, populationContext);
        } else if (algorithm == ForEachGroup.GROUP_STARTING) {
            return new GroupStartingTester(instruction, populationContext);
        } else if (algorithm == ForEachGroup.GROUP_ENDING) {
            return new GroupEndingTester(instruction, populationContext);
        } else {
            throw new AssertionError("Unsupported grouping algorithm for partitioned streaming");
        }
    }

    /**
     * Feed for use when the select expression of the xsl:for-each-group is grounded (but consuming),
     * and the algorithm is partitioning (adjacent, starts-with, or ends-with)
     * Because the action (body) of the for-each-group can use the current group in arbitrary ways,
     * we build each group in memory and then process it in a non-streaming manner. The only concession to
     * streaming is that we don't build the whole population in memory.
     */

    private static class GroupedGroundedFeed extends ItemFeed {

        private final List<Item> currentGroup = new ArrayList<>();
        private int groupCounter = 0;
        private final GroupBoundaryTester boundaryTester;
        private AtomicSequence currentGroupingKey;

        public GroupedGroundedFeed(ForEachGroup instruction, ItemFeed result, XPathContext context) {
            super(instruction, result, context);
            boundaryTester = makeBoundaryTester(instruction, context);
        }

        @Override
        public void append(Item item) throws XPathException {
            ManualIterator popIter = (ManualIterator)getContext().getCurrentIterator();
            popIter.setContextItem(item);
            popIter.incrementPosition();
            boolean startNewGroup = boundaryTester.notifyItem();
            if (startNewGroup) {
                processCompletedGroup();
                currentGroup.clear();
                groupCounter++;
                if (boundaryTester instanceof GroupAdjacentTester) {
                    currentGroupingKey = ((GroupAdjacentTester)boundaryTester).getCurrentGroupingKey();
                }
            }
            currentGroup.add(item);
        }

        @Override
        public void close() throws XPathException {
            processCompletedGroup();
            super.close();
        }

        private void processCompletedGroup() throws XPathException {
            if (!currentGroup.isEmpty()) {
                XPathContextMajor c2 = getContext().newContext();
                ManualIterator popIter = new ManualIterator(currentGroup.get(0), groupCounter);
                c2.setCurrentIterator(popIter);
                ManualGroupIterator gi = new ManualGroupIterator(currentGroup.get(0), 1);
                gi.setCurrentGroup(SequenceExtent.makeSequenceExtent(currentGroup));
                gi.setCurrentGroupingKey(currentGroupingKey);
                c2.setCurrentGroupIterator(gi);
                //c2.setReceiver(getOutputter());
                ((ForEachGroup)getExpression()).getActionExpression().process(getNextOutputter(), c2);
            }
        }
    }

    /**
     * Feed for use when the select expression of the xsl:for-each-group is grounded (but consuming),
     * and group-by is used
     */

    private static class GroupedGroundedDistributedFeed extends ItemFeed {

        // Note: we build the whole population in memory, so this isn't really streamed at all.
        // In some cases we have no choice. But there are cases where we could do better, notably
        // if the for-each-group action is to create a new result document.

        private final List<Item> population = new ArrayList<>();

        public GroupedGroundedDistributedFeed(ForEachGroup instruction, ItemFeed result, XPathContext context) {
            super(instruction, result, context);
        }

        @Override
        public void append(Item item) throws XPathException {
            population.add(item);
        }

        @Override
        public void close() throws XPathException {
            ForEachGroup inst = (ForEachGroup) getExpression();
            Literal pop = Literal.makeLiteral(SequenceExtent.makeSequenceExtent(population), inst);
            GroupIterator groupIterator = inst.getGroupIterator(pop, getContext());
            XPathContextMajor c2 = getContext().newContext();
            FocusIterator focusIterator = c2.trackFocus(groupIterator);
            c2.setCurrentGroupIterator(groupIterator);
            c2.setCurrentTemplateRule(null);
            //c2.setReceiver(getResult().getOutputter());

            while (focusIterator.next() != null) {
                inst.getActionExpression().process(getNextOutputter(), c2);
            }

            super.close();

        }

    }



}

// Copyright (c) 2009-2022 Saxonica Limited