using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class Inversion {
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.stream.Inversion.PushAction> route = new System.Collections.Generic.List<Saxon.Eej.ee.stream.Inversion.PushAction>();
        public static Saxon.Eej.ee.stream.Inversion invertExpression(Saxon.Hej.expr.Expression exp, bool forGrouping) {
            Saxon.Eej.ee.stream.Inversion route = (Saxon.Eej.ee.stream.Inversion)(exp.getExtraProperty("inversion"));
            if ((route != null)) {
                return route;
            }
            route = new Saxon.Eej.ee.stream.Inversion();
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                Saxon.Eej.ee.stream.Inversion.PushAction action = makePushAction(exp, forGrouping);
                route.append(action);
                if ((action.watchMaker == null)) {
                    processChildExpressions(exp, action, route, forGrouping);
                }
                System.Collections.Generic.IList<Saxon.Eej.ee.stream.Inversion.PushAction> r = route.getRoute();
                if ((r[(r.Count-1)].pushMethod != Saxon.Hej.expr.Expression.WATCH_METHOD)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("No streamable path found in expression "+exp.toShortString()), Saxon.Hej.trans.SaxonErrorCode.SXST0060);
                    err.setLocator(exp.getLocation());
                    throw err;
                }
            } else {
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp) == Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                    if (route.getRoute().Count==0) {
                        Saxon.Eej.ee.stream.Inversion.PushAction action = new Saxon.Eej.ee.stream.Inversion.PushAction();
                        action.watchMaker = (watchManager, @out, context) => {
                            Saxon.Eej.ee.stream.feed.MotionlessFeed feed = new Saxon.Eej.ee.stream.feed.MotionlessFeed(exp, @out, context);
                            return new Saxon.Eej.ee.stream.watch.Trigger(Saxon.Hej.pattern.AnchorPattern.getInstance(), feed, context);
                        } ;
                        action.pushMethod = Saxon.Hej.expr.Expression.WATCH_METHOD;
                        action.expression = Saxon.Hej.expr.Literal.makeEmptySequence();
                        route.append(action);
                    }
                } else {
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
            exp.setExtraProperty("inversion", route);
            return route;
        }
        private static void processChildExpressions(Saxon.Hej.expr.Expression parent, Saxon.Eej.ee.stream.Inversion.PushAction parentAction, Saxon.Eej.ee.stream.Inversion route, bool forGrouping) {
            int i = 0;
            Saxon.Hej.expr.Expression firstStreamingChild = null;
            foreach (Saxon.Hej.expr.Operand info in parent.operands()) {
                Saxon.Hej.expr.Expression child = info.getChildExpression();
                if (info.hasSameFocus()) {
                    Saxon.Eej.ee.stream.Sweep sweep = /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(child);
                    if ((sweep == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        if ((firstStreamingChild != null)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((((("Expression "+parent.getExpressionName())+" has more than one subexpression that reads descendants. ")+"The first is ")+firstStreamingChild)+"; the second is ")+child), Saxon.Hej.trans.SaxonErrorCode.SXST0060);
                            err.setLocator(parent.getLocation());
                            throw err;
                        }
                        Saxon.Eej.ee.stream.Inversion.PushAction childAction = makePushAction(child, forGrouping);
                        route.append(childAction);
                        parentAction.feedMaker = parentAction.adjunct.getFeedMaker(i);
                        parentAction.streamingChildSequence = i;
                        if ((childAction.watchMaker == null)) {
                            processChildExpressions(child, childAction, route, forGrouping);
                        }
                        firstStreamingChild = child;
                    } else {
                        if ((child is Saxon.Hej.expr.ContextItemExpression)) {
                            Saxon.Eej.ee.stream.Inversion.PushAction childAction = new Saxon.Eej.ee.stream.Inversion.PushAction();
                            childAction.expression = child;
                            childAction.pushMethod = Saxon.Hej.expr.Expression.WATCH_METHOD;
                            childAction.watchMaker = (watchManager, @out, context) => {
                                Saxon.Eej.ee.stream.feed.CopyOfFeed copy = new Saxon.Eej.ee.stream.feed.CopyOfFeed(watchManager, @out, context);
                                return new Saxon.Eej.ee.stream.watch.Trigger(Saxon.Hej.pattern.AnchorPattern.getInstance(), copy, context);
                            } ;
                            route.append(childAction);
                            parentAction.feedMaker = parentAction.adjunct.getFeedMaker(i);
                            parentAction.streamingChildSequence = i;
                        } else {
                            if ((sweep == Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                            } else {
                                throw new Saxon.Ejava.lang.IllegalStateException();
                            }
                        }
                    }
                }
                i++;
            }
        }
        private static Saxon.Eej.ee.stream.Inversion.PushAction makePushAction(Saxon.Hej.expr.Expression exp, bool forGrouping) {
            Saxon.Hej.Configuration config = exp.getConfiguration();
            Saxon.Hej.expr.Operand consumingChild = /*134*/Saxon.Eej.ee.stream.Streamability.getConsumingOperand(exp);
            Saxon.Eej.ee.stream.Inversion.PushAction action = new Saxon.Eej.ee.stream.Inversion.PushAction();
            action.expression = exp;
            action.adjunct = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(config, exp);
            action.watchMaker = action.adjunct.getWatchMaker(forGrouping);
            if ((action.watchMaker != null)) {
                exp.setEvaluationMethod(Saxon.Hej.expr.Expression.WATCH_METHOD);
                action.pushMethod = Saxon.Hej.expr.Expression.WATCH_METHOD;
                return action;
            } else {
                if ((consumingChild != null)) {
                    action.pushMethod = Saxon.Hej.expr.Expression.ITEM_FEED_METHOD;
                    exp.setEvaluationMethod(Saxon.Hej.expr.Expression.ITEM_FEED_METHOD);
                    return action;
                } else {
                    System.Collections.Generic.IList<string> failures = new System.Collections.Generic.List<string>();
                    Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(exp, config, failures);
                    if ((selection != null)) {
                        if (!(forGrouping)) {
                            config.getLogger().warning((((("** Warning: implicit copy generated for "+exp.toShortString())+" at line ")+exp.getLocation().getLineNumber())+" **"));
                        }
                        bool matchWholeGroup = ((exp is Saxon.Hej.functions.CurrentGroupCall) && forGrouping);
                        action.watchMaker = (watchManager, @out, context) => {
                            Saxon.Eej.ee.stream.watch.Trigger trigger = /*134*/Saxon.Eej.ee.stream.feed.CopyOfFeed.getWatch(@out, selection, watchManager, context);
                            trigger.setMatchCurrentGroup(matchWholeGroup);
                            return trigger;
                        } ;
                        action.pushMethod = Saxon.Hej.expr.Expression.WATCH_METHOD;
                        return action;
                    } else {
                        throw new Saxon.Hej.trans.XPathException(("No watch found for "+exp.toShortString()));
                    }
                }
            }
        }
        public Saxon.Eej.ee.stream.Inversion copy() {
            Saxon.Eej.ee.stream.Inversion r2 = new Saxon.Eej.ee.stream.Inversion();
            Saxon.Impl.Helpers.ListUtils.AddAll(r2.route, route);
            return r2;
        }
        public void append(Saxon.Eej.ee.stream.Inversion.PushAction action) {
            route.Add(action);
        }
        public void prepend(Saxon.Eej.ee.stream.Inversion.PushAction action) {
            route.Insert(0, action);
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.stream.Inversion.PushAction> getRoute() {
            return route;
        }
        public Saxon.Eej.ee.stream.watch.Trigger getWatch(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed parentFeed, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Eej.ee.stream.Inversion.PushAction> route = getRoute();
            foreach (Saxon.Eej.ee.stream.Inversion.PushAction action in route) {
                int pushMethod = action.pushMethod;
                Saxon.Eej.ee.stream.feed.FeedMaker feedMaker = action.feedMaker;
                Saxon.Hej.expr.Expression exp = action.getExpression();
                if ((pushMethod == Saxon.Hej.expr.Expression.ITEM_FEED_METHOD)) {
                    if ((feedMaker == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Streaming is not possible for "+exp.getExpressionName())+" expression. (No feedMaker available for item feed)"), Saxon.Hej.trans.SaxonErrorCode.SXST0060);
                        err.setLocator(exp.getLocation());
                        throw err;
                    }
                    parentFeed = feedMaker.makeItemFeed(watchManager, parentFeed, context);
                } else {
                    if ((pushMethod == Saxon.Hej.expr.Expression.WATCH_METHOD)) {
                        Saxon.Eej.ee.stream.watch.WatchMaker watchMaker = action.watchMaker;
                        if ((watchMaker == null)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Streaming is not possible for "+exp.getExpressionName())+"expression. (Internal error: watch action with no watch maker)"), Saxon.Hej.trans.SaxonErrorCode.SXST0060);
                            err.setLocator(exp.getLocation());
                            throw err;
                        }
                        return /*Delegate.makeWatch()*/watchMaker(watchManager, parentFeed, context);
                    } else {
                        throw new Saxon.Ejava.lang.IllegalStateException(("unknown push method "+pushMethod));
                    }
                }
            }
            return new Saxon.Eej.ee.stream.watch.Trigger(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance()), parentFeed, context);
        }
        public class PushAction {
            public Saxon.Hej.expr.Expression expression;
            public int streamingChildSequence;
            public Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adjunct;
            public int pushMethod;
            public Saxon.Eej.ee.stream.feed.FeedMaker feedMaker;
            public Saxon.Eej.ee.stream.watch.WatchMaker watchMaker;
            public Saxon.Hej.expr.Expression getExpression() {
                return expression;
            }
        
        }
        public void explain(Saxon.Hej.Configuration config) {
            Saxon.Hej.trace.ExpressionPresenter @out = new Saxon.Hej.trace.ExpressionPresenter(config);
            explain(@out);
            @out.close();
        }
        public void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("streamingRoute");
            for (int i = (route.Count-1); (i>=0); i--) {
                Saxon.Eej.ee.stream.Inversion.PushAction action = route[i];
                @out.startSubsidiaryElement("expression");
                @out.emitAttribute("name", ((action.getExpression() == null) ? "*unknown*" : action.getExpression().getExpressionName()));
                @out.emitAttribute("method", getMethodName(action.pushMethod));
                @out.endSubsidiaryElement();
            }
            @out.endElement();
        }
        private string getMethodName(int pushMethod) {
            switch (pushMethod) {
              case Saxon.Hej.expr.Expression.WATCH_METHOD:
                return "watch";
              case Saxon.Hej.expr.Expression.ITEM_FEED_METHOD:
                return "itemFeed";
              default:
                return (""+pushMethod);
            }
        }
    
    }
}