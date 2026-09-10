package com.saxonica.ee.stream;

import com.saxonica.ee.bytecode.ByteCodeCandidate;
import com.saxonica.ee.stream.adjunct.StreamingAdjunct;
import com.saxonica.ee.stream.feed.CopyOfFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.MotionlessFeed;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.functions.CurrentGroupCall;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ErrorType;

import java.util.ArrayList;
import java.util.List;

/**
 * This class represents an expression that has been "inverted" (in the Jackson sense) so that its
 * execution is push-driven, that is, driven by events from the streamed document. The route taken
 * by these events through the expressions in the expression tree is the "streaming route". Currently
 * this class is simply a wrapper around the {@link Inversion} object that represents the
 * path through the expression tree containing expressions that are evaluated in push mode, that is,
 * those that perform downward navigation in the streamed document input.
 * <p>The expression inversion is represented by a "streaming route", which is
 * the path of nodes in the expression tree, from the root to a leaf, being
 * evaluated in streaming mode. The streaming route starts with a Watch, which is notified
 * by the parser of interesting events. It then proceeds through a series of Feeds which
 * process these events (or items composed from these events) before finally being output
 * to a Receiver which handles the transformation result.</p>
 * <p>The list of actions is "outermost expression first". This is the opposite of the order
 * in which the events actually pass through the pipeline.</p>
 */
public class Inversion {

    private final List<PushAction> route = new ArrayList<>();

    /**
     * Compute the Jackson inversion of an expression. The effect is (a) to determine whether the
     * expression is streamable (throwing an exception if not), and (b) to construct the "Streaming route",
     * that is, the path through the expression tree containing expressions that perform downward navigation
     * within the streamed document
     *
     *
     *
     * @param exp the expression to be inverted
     * @param forGrouping true if the expression is the body of a for-each-group that is to be
     * inverted with respect to the grouping variable rather than the context item
     * @return the inverted expression
     * @throws net.sf.saxon.trans.XPathException
     *          if the expression is not streamable
     */

    public static Inversion invertExpression(final Expression exp, boolean forGrouping) throws XPathException {
        Inversion route = (Inversion)exp.getExtraProperty("inversion");
        if (route != null) {
            return route;
        }
        route = new Inversion();
        if (Streamability.getSweep(exp) == Sweep.CONSUMING) {
            PushAction action = makePushAction(exp, forGrouping);
            route.append(action);
            if (action.watchMaker == null) {
                processChildExpressions(exp, action, route, forGrouping);
            }

            // The last thing on the route must be a Watch
            List<PushAction> r = route.getRoute();
            if (r.get(r.size() - 1).pushMethod != Expression.WATCH_METHOD) {
                XPathException err = new XPathException("No streamable path found in expression " + exp.toShortString(), SaxonErrorCode.SXST0060);
                err.setLocator(exp.getLocation());
                throw err;
            }

        } else if (Streamability.getSweep(exp) == Sweep.MOTIONLESS) {
            if (route.getRoute().isEmpty()) {
                PushAction action = new PushAction();
                action.watchMaker = (watchManager, out, context) -> {
                    // return a Watch that matches the anchor node only
                    MotionlessFeed feed = new MotionlessFeed(exp, out, context);
                    return new Trigger(AnchorPattern.getInstance(), feed, context);
                };
                action.pushMethod = Expression.WATCH_METHOD;
                action.expression = Literal.makeEmptySequence();
                route.append(action);
            }
        } else {
            throw new IllegalStateException();
        }
        exp.setExtraProperty("inversion", route);
        //route.explain(exp.getConfiguration());
        return route;
    }

    /**
     * Internal recursive method to walk down the expression tree finding the streaming route.
     * This method detects when an expression has more than one child that performs downward
     * selection, reporting a streamability exception in this case.
     *
     *
     * @param parent       the parent expression
     * @param parentAction the push implementation of the parent expression
     * @param route        the streaming route, to which this method appends
     * @param forGrouping  true if this is a grouping operation
     * @throws net.sf.saxon.trans.XPathException if a non-streamable expression is found, in particular, if an expression
     *                        has more than one subexpression that does downward selection.
     */

    private static void processChildExpressions(Expression parent, PushAction parentAction, Inversion route, final boolean forGrouping)
            throws XPathException {
        int i = 0;
        Expression firstStreamingChild = null;
        for (Operand info : parent.operands()) {
            Expression child = info.getChildExpression();
            if (child instanceof ByteCodeCandidate) {
                child = ((ByteCodeCandidate)child).getBaseExpression();
            }
            if (info.hasSameFocus()) {
                Sweep sweep = Streamability.getSweep(child);
                if (sweep == Sweep.CONSUMING) {
                    // this is a consuming expression
                    if (firstStreamingChild != null) {
                        XPathException err = new XPathException(
                                "Expression " + parent.getExpressionName() +
                                        " has more than one subexpression that reads descendants. " +
                                        "The first is " + firstStreamingChild + "; the second is " +
                                        child, SaxonErrorCode.SXST0060);
                        err.setLocator(parent.getLocation());
                        throw err;
                    }

                    PushAction childAction = makePushAction(child, forGrouping);
                    route.append(childAction);
                    parentAction.feedMaker = parentAction.adjunct.getFeedMaker(i);
                    parentAction.streamingChildSequence = i;
                    if (childAction.watchMaker == null) {
                        processChildExpressions(child, childAction, route, forGrouping);
                    }
                    firstStreamingChild = child;
                } else if (child instanceof ContextItemExpression) {
                    PushAction childAction = new PushAction();
                    childAction.expression = child;
                    childAction.pushMethod = Expression.WATCH_METHOD;
                    childAction.watchMaker = (watchManager, out, context) -> {
                        CopyOfFeed copy = new CopyOfFeed(watchManager, out, context);
                        return new Trigger(AnchorPattern.getInstance(), copy, context);
                    };
                    route.append(childAction);
                    parentAction.feedMaker = parentAction.adjunct.getFeedMaker(i);
                    parentAction.streamingChildSequence = i;
                } else if (sweep == Sweep.MOTIONLESS) {
                    // a non-consuming expression; no action
                } else {
                    throw new IllegalStateException();
                }
            }
            i++;
        }

    }

    /**
     * Make a PushAction object representing the strategy for evaluating an expression in push mode
     * @param exp the expression to be evaluated
     * @param forGrouping true if this is a grouping operation
     * @return the corresponding PushAction object, or null if the expression is motioness
     * @throws net.sf.saxon.trans.XPathException if any error occurs, for example if the expression is free-ranging
     */

    /*@Nullable*/
    private static PushAction makePushAction(Expression exp, boolean forGrouping) throws XPathException {
        Configuration config = exp.getConfiguration();
        Operand consumingChild = Streamability.getConsumingOperand(exp);
        PushAction action = new PushAction();
        action.expression = exp;
        action.adjunct = StreamingAdjunct.makeStreamingAdjunct(config, exp);
        action.watchMaker = action.adjunct.getWatchMaker(forGrouping);
        if (action.watchMaker != null) {
            // evaluate this expression as a Watch (push evaluation, event by event)
            exp.setEvaluationMethod(Expression.WATCH_METHOD);
            action.pushMethod = Expression.WATCH_METHOD;
            return action;
        } else if (consumingChild != null) {
            action.pushMethod = Expression.ITEM_FEED_METHOD;
            exp.setEvaluationMethod(Expression.ITEM_FEED_METHOD);
            return action;
        } else {
            // This happens with grouping use cases
            // See if the expression is a valid pattern
            List<String> failures = new ArrayList<>();
            final Pattern selection = StreamingPatternMaker.makeStreamingPattern(exp, config, failures);
            if (selection != null) {
                // Failure here is normal; catch the error and recurse down the expression tree.
                // It's when we succeed that there is a problem: we have a streamable expression with
                // no watch available.
                if (!forGrouping) {
                    // When grouping, we generate a CopyWatch for the current-group() expression, but never use
                    // it; it is generated while inverting the body of the xsl:for-each-group instruction,
                    // and we detach the feed from this watch and feed in events from the matched select expression
                    // instead.
                    config.getLogger().warning("** Warning: implicit copy generated for " + exp.toShortString() +
                            " at line " + exp.getLocation().getLineNumber() + " **");
                }
                final boolean matchWholeGroup = exp instanceof CurrentGroupCall && forGrouping;
                action.watchMaker = (watchManager, out, context) -> {
                    Trigger trigger = CopyOfFeed.getWatch(out, selection, watchManager, context);
                    trigger.setMatchCurrentGroup(matchWholeGroup);
                    return trigger;
                };
                action.pushMethod = Expression.WATCH_METHOD;
                return action;
            } else {
                throw new XPathException("No watch found for " + exp.toShortString());
            }
        }

    }

    /**
     * Create a copy of this inverted expression
     *
     * @return the copy
     */

    public Inversion copy() {
        Inversion r2 = new Inversion();
        r2.route.addAll(route);
        return r2;
    }

    /**
     * Add an action to the end of the streaming route (a subexpression of the currently-last expression,
     * which is therefore first to receive events from the parser)
     *
     * @param action the action to be appended
     */

    public void append(PushAction action) {
        route.add(action);
    }

    /**
     * Add an action to the start of this streaming route (a parent of the currently-first expression,
     * which is therefore last to receive events from the parser)
     *
     * @param action the action to be prepended
     */

    public void prepend(PushAction action) {
        route.add(0, action);
    }

    /**
     * Get the list of push actions making up the streaming route
     *
     * @return the list of push actions
     */

    public List<PushAction> getRoute() {
        return route;
    }

    /**
     * Make a Watch to represent this streaming route. The Watch is registered with the WatchManager so it can be
     * notified of events that cause it to be activated
     *
     * @param watchManager the WatchManager (which is known to each Watch under its control)
     * @param parentFeed   Maybe null. The destination of events after processing by this Watch
     * @param context XPath context object
     * @return the Watch
     * @throws XPathException if an error occurs, e.g. if streaming is not possible.
     */

    public Trigger getWatch(WatchManager watchManager, ItemFeed parentFeed, XPathContext context) throws XPathException {
        List<PushAction> route = getRoute();
        for (PushAction action : route) {
            int pushMethod = action.pushMethod;
            FeedMaker feedMaker = action.feedMaker;
            Expression exp = action.getExpression();
            if (pushMethod == Expression.ITEM_FEED_METHOD) {
                if (feedMaker == null) {
                    XPathException err = new XPathException("Streaming is not possible for " + exp.getExpressionName() +
                            " expression. (No feedMaker available for item feed)", SaxonErrorCode.SXST0060);
                    err.setLocator(exp.getLocation());
                    throw err;
                }
                parentFeed = feedMaker.makeItemFeed(watchManager, parentFeed, context);

            } else if (pushMethod == Expression.WATCH_METHOD) {
                WatchMaker watchMaker = action.watchMaker;
                if (watchMaker == null) {
                    XPathException err = new XPathException("Streaming is not possible for " + exp.getExpressionName() +
                            "expression. (Internal error: watch action with no watch maker)", SaxonErrorCode.SXST0060);
                    err.setLocator(exp.getLocation());
                    throw err;
                }
                return watchMaker.makeWatch(watchManager, parentFeed, context);
            } else {
                throw new IllegalStateException("unknown push method " + pushMethod);
            }
        }
        // return a Watch that never matches anything
        return new Trigger(new NodeTestPattern(ErrorType.getInstance()), parentFeed, context);
    }


    /**
     * A PushAction is a collection of data that together represents the strategy for how an expression should be
     * evaluated in push mode. The StreamingRoute is effectively a list of PushAction objects, one for each expression
     * that needs to be processed in push mode.
     */
    public static class PushAction {
        public Expression expression;
        public int streamingChildSequence;
        public StreamingAdjunct adjunct;
        public int pushMethod;
        public FeedMaker feedMaker;
        /*@Nullable*/ public WatchMaker watchMaker;

        public Expression getExpression() {
            return expression;
        }
    }

    public void explain(Configuration config) {
        ExpressionPresenter out = new ExpressionPresenter(config);
        explain(out);
        out.close();
    }

    /**
     * Display the streaming route for diagnostic purposes
     *
     * @param out the display destination
     */

    public void explain(ExpressionPresenter out) {
        out.startElement("streamingRoute");
        for (int i = route.size() - 1; i >= 0; i--) {
            PushAction action = route.get(i);
            out.startSubsidiaryElement("expression");
            out.emitAttribute("name", action.getExpression() == null ? "*unknown*" : action.getExpression().getExpressionName());
            out.emitAttribute("method", getMethodName(action.pushMethod));
            out.endSubsidiaryElement();
        }
        out.endElement();
    }

    private String getMethodName(int pushMethod) {
        switch (pushMethod) {
            case Expression.WATCH_METHOD:
                return "watch";
            case Expression.ITEM_FEED_METHOD:
                return "itemFeed";
            default:
                return "" + pushMethod;
        }
    }
}

// Copyright (c) 2009-2022 Saxonica Limited


