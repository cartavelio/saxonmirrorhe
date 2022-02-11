using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ForEachAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private bool isBuffering;
        public override void setExpression(Saxon.Hej.expr.Expression expression) {
            base.setExpression(expression);
            if ((expression is Saxon.Hej.expr.instruct.ForEach)) {
                isBuffering = (((((Saxon.Hej.expr.instruct.ForEach)(getExpression())).getActionExpression().getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)) != 0);
            }
        }
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            if ((getExpression() is Saxon.Hej.expr.SlashExpression)) {
                Saxon.Hej.expr.SlashExpression exp = (Saxon.Hej.expr.SlashExpression)(getExpression());
                Saxon.Hej.expr.Expression head = exp.getLeadingSteps();
                if ((head is Saxon.Hej.expr.RootExpression)) {
                    return null;
                }
                if ((head is Saxon.Hej.functions.CurrentGroupCall)) {
                    return null;
                }
                if ((head is Saxon.Hej.expr.LocalVariableReference)) {
                    bool fail = true;
                    Saxon.Hej.expr.Binding binding = ((Saxon.Hej.expr.LocalVariableReference)(head)).getBinding();
                    if ((binding is Saxon.Hej.expr.instruct.UserFunctionParameter)) {
                        Saxon.Hej.trans.FunctionStreamability fs = ((Saxon.Hej.expr.instruct.UserFunctionParameter)(binding)).getFunctionStreamability();
                        if (((fs == Saxon.Hej.trans.FunctionStreamability.ABSORBING) || (fs == Saxon.Hej.trans.FunctionStreamability.SHALLOW_DESCENT))) {
                            fail = false;
                        }
                    }
                    if (fail) {
                        return null;
                    }
                } else {
                    if ((head is Saxon.Hej.expr.VariableReference)) {
                        return null;
                    }
                }
                Saxon.Hej.expr.Expression headPredicate = null;
                if ((head is Saxon.Hej.expr.ItemChecker)) {
                    Saxon.Hej.type.ItemType check = ((Saxon.Hej.expr.ItemChecker)(head)).getRequiredType();
                    head = ((Saxon.Hej.expr.ItemChecker)(head)).getBaseExpression();
                    if (!(((check is Saxon.Hej.pattern.AnyNodeTest)))) {
                        if ((check is Saxon.Hej.pattern.DocumentNodeTest)) {
                            return null;
                        }
                        headPredicate = new Saxon.Hej.expr.InstanceOfExpression(new Saxon.Hej.expr.ContextItemExpression(), Saxon.Hej.value.SequenceType.makeSequenceType(check, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE));
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, headPredicate);
                    }
                }
                Saxon.Hej.expr.Expression tail = exp.getLastStep();
                Saxon.Hej.pattern.Pattern tailPattern = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(tail, config);
                if (((tailPattern is Saxon.Hej.pattern.AncestorQualifiedPattern) && (((Saxon.Hej.pattern.AncestorQualifiedPattern)(tailPattern)).getUpperPattern() is Saxon.Hej.pattern.AnchorPattern))) {
                    tailPattern = ((Saxon.Hej.pattern.AncestorQualifiedPattern)(tailPattern)).getBasePattern();
                }
                int axis;
                try {
                    if ((tailPattern is Saxon.Hej.pattern.SimplePositionalPattern)) {
                        axis = Saxon.Hej.om.AxisInfo.PARENT;
                    } else {
                        axis = Saxon.Hej.pattern.PatternMaker.getAxisForPathStep(tail);
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    return null;
                }
                Saxon.Hej.pattern.Pattern headPattern = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(head, config);
                if (((headPattern == null) || (tailPattern == null))) {
                    return null;
                }
                if ((headPredicate != null)) {
                    headPattern = new Saxon.Hej.pattern.BasePatternWithPredicate(headPattern, headPredicate);
                }
                return new Saxon.Hej.pattern.AncestorQualifiedPattern(tailPattern, headPattern, axis);
            } else {
                return null;
            }
        }
        internal Saxon.Eej.ee.stream.Inversion actionInversion = null;
        protected internal Saxon.Eej.ee.stream.Inversion getActionInversion(bool forGrouping) {
            if ((actionInversion == null)) {
                Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.ContextSwitchingExpression)(getExpression())).getActionExpression();
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                    actionInversion = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(action, forGrouping);
                }
            }
            return actionInversion;
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.Configuration config = getConfiguration();
            System.Collections.Generic.IList<string> failures = new System.Collections.Generic.List<string>();
            if (!(forGrouping)) {
                Saxon.Hej.pattern.Pattern pattern = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(getExpression(), config, failures);
                if ((pattern != null)) {
                    return (watchManager, @out, context) => new Saxon.Eej.ee.stream.watch.Trigger(pattern, @out, context);
                }
            }
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.ContextSwitchingExpression)(getExpression())).getSelectExpression();
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select) == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return null;
            }
            Saxon.Eej.ee.stream.Inversion actionInversion = getActionInversion(false);
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, config, null);
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(config, select);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.watch.ForEachAction actor = new Saxon.Eej.ee.stream.watch.ForEachAction(watchManager, getExpression(), @out, context);
                    if ((actionInversion != null)) {
                        actor.setInversion(actionInversion);
                    }
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, actor, context);
                } ;
            } else {
                if ((sa is Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)) {
                    return (watchManager, @out, context) => {
                        Saxon.Eej.ee.stream.watch.ForEachAction actor = new Saxon.Eej.ee.stream.watch.ForEachAction(watchManager, getExpression(), @out, context);
                        if ((actionInversion != null)) {
                            actor.setInversion(actionInversion);
                        }
                        return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(sa)).makeTransmissionFlow(watchManager, select, actor, context);
                    } ;
                } else {
                    return sa.getWatchMaker(forGrouping);
                }
            }
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            if (isBuffering) {
                return new Saxon.Eej.ee.stream.feed.BufferingForEachExpressionFeed(((Saxon.Hej.expr.instruct.ForEach)(getExpression())), @out, context);
            }
            Saxon.Eej.ee.stream.Inversion actionInversion = getActionInversion(false);
            Saxon.Eej.ee.stream.watch.ForEachAction actor = new Saxon.Eej.ee.stream.watch.ForEachAction(watchManager, getExpression(), @out, context);
            if ((getActionInversion(false) != null)) {
                actor.setInversion(actionInversion);
            }
            return actor;
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            return /*134*/Saxon.Eej.ee.stream.adjunct.ForEachAdjunct.getStreamability((Saxon.Hej.expr.ContextSwitchingExpression)(getExpression()), contextInfo, reasons);
        }
        public static Saxon.Eej.ee.stream.PostureAndSweep getStreamability(Saxon.Hej.expr.ContextSwitchingExpression expression, Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.Expression select = expression.getSelectExpression();
            Saxon.Hej.expr.Expression action = expression.getActionExpression();
            Saxon.Eej.ee.stream.PostureAndSweep selectPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(select, contextInfo, reasons);
            if ((selectPS.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules((Saxon.Hej.expr.Expression)(expression), ((Saxon.Hej.expr.Expression)(expression)).operands(), contextInfo, reasons);
            }
            if ((select is Saxon.Hej.expr.sort.SortExpression)) {
                if ((reasons != null)) {
                    reasons.Add("Sorting is not possible during streamed evaluation");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if (((expression is Saxon.Hej.expr.instruct.ForEach) && (((Saxon.Hej.expr.instruct.ForEach)(expression)).getSeparatorExpression() != null))) {
                if ((reasons != null)) {
                    reasons.Add("Streaming is not supported for xsl:for-each/@separator");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE info = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(select.getItemType(), false, selectPS.getPosture());
            info.setContextSettingExpression(select);
            info.setStrictStreamabilityRules(contextInfo.isStrictStreamabilityRules());
            Saxon.Eej.ee.stream.PostureAndSweep actionPS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(action, info, reasons);
            if (((!(((expression is Saxon.Hej.expr.SlashExpression))) && (selectPS.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING)) && (actionPS.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING))) {
                if ((reasons != null)) {
                    reasons.Add((("Expression "+((Saxon.Hej.expr.Expression)(expression)).toShortString())+" is not streamable because the select expression is crawling and the body is consuming"));
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Eej.ee.stream.Sweep s = /*134*/Saxon.Eej.ee.stream.Streamability.wider(selectPS.getSweep(), actionPS.getSweep());
            Saxon.Eej.ee.stream.PostureAndSweep provisional = new Saxon.Eej.ee.stream.PostureAndSweep(((s == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) ? Saxon.Eej.ee.stream.Posture.ROAMING : actionPS.getPosture()), s);
            if (((provisional.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING) && (expression is Saxon.Hej.expr.SlashExpression))) {
                Saxon.Hej.expr.SlashExpression exp = (Saxon.Hej.expr.SlashExpression)(expression);
                Saxon.Hej.pattern.Pattern p = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(exp, exp.getConfiguration());
                if ((p != null)) {
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CRAWLING, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                }
            }
            return provisional;
        }
    
    }
}