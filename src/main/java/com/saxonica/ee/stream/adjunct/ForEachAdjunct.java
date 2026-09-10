package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.BufferingForEachExpressionFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.ForEachAction;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.sort.SortExpression;
import net.sf.saxon.functions.CurrentGroupCall;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.pattern.*;
import net.sf.saxon.trans.FunctionStreamability;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * Compile an xsl:for-each instruction to streaming code. Also handles a path expression (SlashExpression).
 */

public class ForEachAdjunct extends TransmissionAdjunct implements FeedMaker {

    private boolean isBuffering;

    /**
     * Set the expression being processed by this StreamingAdjunct
     *
     * @param expression the expression being processed
     */
    @Override
    public void setExpression(Expression expression) {
        super.setExpression(expression);
        if (expression instanceof ForEach) {
            isBuffering =
                    (((ForEach) getExpression()).getActionExpression().getDependencies() & StaticProperty.DEPENDS_ON_LAST) != 0;
        }
    }

    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     * @param config the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */
    @Override
    public Pattern toStreamingPattern(Configuration config) {
        if (getExpression() instanceof SlashExpression) {
            SlashExpression exp = (SlashExpression) getExpression();

            Expression head = exp.getLeadingSteps();
            if (head instanceof RootExpression) {
                return null;
            }
            if (head instanceof CurrentGroupCall) {
                return null;
            }
            if (head instanceof LocalVariableReference) {
                boolean fail = true;
                Binding binding = ((LocalVariableReference)head).getBinding();
                if (binding instanceof UserFunctionParameter) {
                    FunctionStreamability fs = ((UserFunctionParameter) binding).getFunctionStreamability();
                    if (fs == FunctionStreamability.ABSORBING || fs == FunctionStreamability.SHALLOW_DESCENT) {
                        fail = false;
                    }
                }
                if (fail) {
                    return null;
                }
            } else if (head instanceof VariableReference) {
                return null;
            }
            Expression headPredicate = null;
            if (head instanceof ItemChecker) {
                ItemType check = ((ItemChecker) head).getRequiredType();
                head = ((ItemChecker) head).getBaseExpression();
                if (!(check instanceof AnyNodeTest)) {
                    if (check instanceof DocumentNodeTest) {
                        // document-node(element(X)) is not streamable
                        return null;
                    }
                    headPredicate = new InstanceOfExpression(
                            new ContextItemExpression(), SequenceType.makeSequenceType(check, StaticProperty.ALLOWS_ZERO_OR_MORE));
                    ExpressionTool.copyLocationInfo(exp, headPredicate);
                }
            }
            Expression tail = exp.getLastStep();
            Pattern tailPattern = Streamability.toStreamingPattern(tail, config);
            if (tailPattern instanceof AncestorQualifiedPattern &&
                    ((AncestorQualifiedPattern) tailPattern).getUpperPattern() instanceof AnchorPattern) {
                tailPattern = ((AncestorQualifiedPattern) tailPattern).getBasePattern();
            }

            int axis;
            try {
                if (tailPattern instanceof SimplePositionalPattern) {
                    axis = AxisInfo.PARENT;
                } else {
                    axis = PatternMaker.getAxisForPathStep(tail);
                }
            } catch (XPathException e) {
                return null;
            }
            Pattern headPattern = Streamability.toStreamingPattern(head, config);
            if (headPattern == null || tailPattern == null) {
                return null;
            }
            if (headPredicate != null) {
                headPattern = new BasePatternWithPredicate(headPattern, headPredicate);
            }
            return new AncestorQualifiedPattern(tailPattern, headPattern, axis);
        } else {
            return null;
        }
    }


    Inversion actionInversion = null;

    protected Inversion getActionInversion(boolean forGrouping) throws XPathException {
        if (actionInversion == null) {
            Expression action = ((ContextSwitchingExpression) getExpression()).getActionExpression();
            if (Streamability.getSweep(action) == Sweep.CONSUMING) {
                actionInversion = Inversion.invertExpression(action, forGrouping);
            }
        }
        return actionInversion;
    }

    /*@Nullable*/
    @Override
    public WatchMaker getWatchMaker(final boolean forGrouping) throws XPathException {

        // If the expression in its entirety is a streamable pattern, then return a CopyWatch on this expression. (This is more
        // likely to apply to a path expression a/b/c than to an actual xsl:for-each instruction, and it happens when the path
        // appears in an unusual context - see for example strmode089

        Configuration config = getConfiguration();
        List<String> failures = new ArrayList<String>();

        if (!forGrouping) {
            final Pattern pattern = StreamingPatternMaker.makeStreamingPattern(getExpression(), config, failures);
            if (pattern != null) {
                return (watchManager, out, context) -> new Trigger(pattern, out, context);
            }
        }

        final Expression select = ((ContextSwitchingExpression) getExpression()).getSelectExpression();

        // If the select expression is grounded (but consuming), return null; so we end up with a ItemFeed rather than a Watch.
        if (Streamability.getPosture(select) == Posture.GROUNDED) {
            return null;
        }

        final Inversion actionInversion = getActionInversion(false);

        final Pattern selection = StreamingPatternMaker.makeStreamingPattern(select, config, null);
        final StreamingAdjunct sa = StreamingAdjunct.makeStreamingAdjunct(config, select);
        if (selection != null) {
            return (watchManager, out, context) -> {
                ForEachAction actor = new ForEachAction(watchManager, getExpression(), out, context);
                if (actionInversion != null) {
                    actor.setInversion(actionInversion);
                }
                return new Trigger(selection, actor, context);
            };
        } else if (sa instanceof TransmissionAdjunct) {
            return (watchManager, out, context) -> {
                ForEachAction actor = new ForEachAction(watchManager, getExpression(), out, context);
                if (actionInversion != null) {
                    actor.setInversion(actionInversion);
                }
                return ((TransmissionAdjunct) sa).makeTransmissionFlow(watchManager, select, actor, context);
            };
        } else {
            return sa.getWatchMaker(forGrouping);
        }
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        if (isBuffering) {
            return new BufferingForEachExpressionFeed(((ForEach)getExpression()), out, context);
        } 
        final Inversion actionInversion = getActionInversion(false);
        ForEachAction actor = new ForEachAction(watchManager, getExpression(), out, context);
        if (getActionInversion(false) != null) {
            actor.setInversion(actionInversion);
        }
        return actor;
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        return ForEachAdjunct.getStreamability((ContextSwitchingExpression) getExpression(), contextInfo, reasons);
    }

    public static PostureAndSweep getStreamability(
            ContextSwitchingExpression expression,
            ContextItemStaticInfoEE contextInfo, List<String> reasons) {

        // See XSLT 3.0 19.8.4.18

        Expression select = expression.getSelectExpression();
        Expression action = expression.getActionExpression();

        // Rule 1

        PostureAndSweep selectPS = Streamability.getStreamability(select, contextInfo, reasons);
        if (selectPS.getPosture() == Posture.GROUNDED) {
            return Streamability.generalStreamabilityRules(
                    (Expression) expression,
                    ((Expression) expression).operands(), contextInfo, reasons);
        }
        // Rule 2

        if (select instanceof SortExpression) {
            if (reasons != null) {
                reasons.add("Sorting is not possible during streamed evaluation");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        if (expression instanceof ForEach && ((ForEach)expression).getSeparatorExpression() != null) {
            if (reasons != null) {
                reasons.add("Streaming is not supported for xsl:for-each/@separator");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 3

        ContextItemStaticInfoEE info = new ContextItemStaticInfoEE(select.getItemType(), false, selectPS.getPosture());
        info.setContextSettingExpression(select);
        info.setStrictStreamabilityRules(contextInfo.isStrictStreamabilityRules());
        PostureAndSweep actionPS = Streamability.getStreamability(action, info, reasons);

        // Temporary rule, see comment 12 of bug 25185
        if (!(expression instanceof SlashExpression) &&
                selectPS.getPosture() == Posture.CRAWLING && actionPS.getSweep() == Sweep.CONSUMING) {
            if (reasons != null) {
                reasons.add("Expression " + ((Expression) expression).toShortString() +
                                    " is not streamable because the select expression is crawling and the body is consuming");
            }
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 4
        Sweep s = Streamability.wider(selectPS.getSweep(), actionPS.getSweep());
        PostureAndSweep provisional = new PostureAndSweep(s == Sweep.FREE_RANGING ? Posture.ROAMING : actionPS.getPosture(), s);

        if (provisional.getPosture() == Posture.ROAMING && expression instanceof SlashExpression) {
            // Try the fallback of using a "scanning expression". Note that if we have been through optimization,
            //, the expression //A/B will have been turned into //B[parent::A] making this unnecessary. But for
            // strict streamability checking on an unoptimized tree, we need to handle this case. Test streamable-030.
            SlashExpression exp = (SlashExpression)expression;
            Pattern p = Streamability.toStreamingPattern(exp, exp.getConfiguration());
            if (p != null) {
                return new PostureAndSweep(Posture.CRAWLING, Sweep.CONSUMING);
            }
        }

        return provisional;

    }


}

// Copyright (c) 2009-2023 Saxonica Limited