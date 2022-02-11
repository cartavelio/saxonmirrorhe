using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class StreamingAdjunct {

        static StreamingAdjunct() {
            map("AdjacentTextNodeMerger", typeof(Saxon.Eej.ee.stream.adjunct.AdjacentTextNodeMergerAdjunct));
            map("ApplyImports", typeof(Saxon.Eej.ee.stream.adjunct.ApplyImportsAdjunct));
            map("ApplyTemplates", typeof(Saxon.Eej.ee.stream.adjunct.ApplyTemplatesAdjunct));
            map("ArrayBlock", typeof(Saxon.Eej.ee.stream.adjunct.ArrayBlockAdjunct));
            map("AtomicSequenceConverter", typeof(Saxon.Eej.ee.stream.adjunct.AtomicSequenceConverterAdjunct));
            map("Atomizer", typeof(Saxon.Eej.ee.stream.adjunct.AtomizerAdjunct));
            map("AxisExpression", typeof(Saxon.Eej.ee.stream.adjunct.AxisExpressionAdjunct));
            map("Block", typeof(Saxon.Eej.ee.stream.adjunct.BlockAdjunct));
            map("CallTemplate", typeof(Saxon.Eej.ee.stream.adjunct.CallTemplateAdjunct));
            map("CardinalityChecker", typeof(Saxon.Eej.ee.stream.adjunct.CardinalityCheckerAdjunct));
            map("Choose", typeof(Saxon.Eej.ee.stream.adjunct.ChooseAdjunct));
            map("ConditionalBlock", typeof(Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct));
            map("ContextItemExpr", typeof(Saxon.Eej.ee.stream.adjunct.ContextItemExprAdjunct));
            map("Copy", typeof(Saxon.Eej.ee.stream.adjunct.CopyAdjunct));
            map("CopyOf", typeof(Saxon.Eej.ee.stream.adjunct.CopyOfAdjunct));
            map("CurrentGroup", typeof(Saxon.Eej.ee.stream.adjunct.CurrentGroupStreamer));
            map("DocumentInstr", typeof(Saxon.Eej.ee.stream.adjunct.DocumentInstrAdjunct));
            map("DocumentSorterAdjunct", typeof(Saxon.Eej.ee.stream.adjunct.DocumentSorterAdjunct));
            map("ElementCreator", typeof(Saxon.Eej.ee.stream.adjunct.ElementCreatorAdjunct));
            map("EmptyTextNodeRemover", typeof(Saxon.Eej.ee.stream.adjunct.EmptyTextNodeRemoverAdjunct));
            map("FilterExpression", typeof(Saxon.Eej.ee.stream.adjunct.FilterExpressionAdjunct));
            map("FirstItemExpression", typeof(Saxon.Eej.ee.stream.adjunct.FirstItemExpressionAdjunct));
            map("ForEach", typeof(Saxon.Eej.ee.stream.adjunct.ForEachAdjunct));
            map("ForEachGroup", typeof(Saxon.Eej.ee.stream.adjunct.ForEachGroupAdjunct));
            map("ForExpression", typeof(Saxon.Eej.ee.stream.adjunct.ForExpressionAdjunct));
            map("Fork", typeof(Saxon.Eej.ee.stream.adjunct.ForkAdjunct));
            map("GeneralComparison", typeof(Saxon.Eej.ee.stream.adjunct.GeneralComparisonAdjunct));
            map("InstanceOf", typeof(Saxon.Eej.ee.stream.adjunct.InstanceOfAdjunct));
            map("IsLastExpr", typeof(Saxon.Eej.ee.stream.adjunct.IsLastExprAdjunct));
            map("ItemChecker", typeof(Saxon.Eej.ee.stream.adjunct.ItemCheckerAdjunct));
            map("Iterate", typeof(Saxon.Eej.ee.stream.adjunct.IterateAdjunct));
            map("Literal", typeof(Saxon.Eej.ee.stream.adjunct.LiteralAdjunct));
            map("MergeInstr", typeof(Saxon.Eej.ee.stream.adjunct.MergeInstrAdjunct));
            map("NextIteration", typeof(Saxon.Eej.ee.stream.adjunct.NextIterationAdjunct));
            map("NextMatch", typeof(Saxon.Eej.ee.stream.adjunct.NextMatchAdjunct));
            map("OnEmpty", typeof(Saxon.Eej.ee.stream.adjunct.OnEmptyAdjunct));
            map("OnNonEmpty", typeof(Saxon.Eej.ee.stream.adjunct.OnNonEmptyAdjunct));
            map("RootExpression", typeof(Saxon.Eej.ee.stream.adjunct.RootExpressionAdjunct));
            map("ResultDocument", typeof(Saxon.Eej.ee.stream.adjunct.ResultDocumentAdjunct));
            map("SequenceInstr", typeof(Saxon.Eej.ee.stream.adjunct.SequenceInstrAdjunct));
            map("SimpleNodeConstructor", typeof(Saxon.Eej.ee.stream.adjunct.SimpleNodeConstructorAdjunct));
            map("SingletonAtomizer", typeof(Saxon.Eej.ee.stream.adjunct.SingletonAtomizerAdjunct));
            map("SlashExpression", typeof(Saxon.Eej.ee.stream.adjunct.ForEachAdjunct));
            map("SortExpression", typeof(Saxon.Eej.ee.stream.adjunct.SortExpressionAdjunct));
            map("SubscriptExpression", typeof(Saxon.Eej.ee.stream.adjunct.SubscriptExpressionAdjunct));
            map("SwitchExpression", typeof(Saxon.Eej.ee.stream.adjunct.SwitchAdjunct));
            map("TailExpression", typeof(Saxon.Eej.ee.stream.adjunct.TailExpressionAdjunct));
            map("TraceExpr", typeof(Saxon.Eej.ee.stream.adjunct.TraceExprAdjunct));
            map("TryCatch", typeof(Saxon.Eej.ee.stream.adjunct.TryCatchAdjunct));
            map("UseAttributeSet", typeof(Saxon.Eej.ee.stream.adjunct.UseAttributeSetAdjunct));
            map("VariableReference", typeof(Saxon.Eej.ee.stream.adjunct.VariableReferenceAdjunct));
            map("VennExpression", typeof(Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct));
            map("WherePopulated", typeof(Saxon.Eej.ee.stream.adjunct.WherePopulatedAdjunct));
        }
        private static readonly System.Collections.Generic.IDictionary<string,System.Type> adjunctMap = emptyAdjunctMap();
        public static System.Collections.Generic.IDictionary<string,System.Type> emptyAdjunctMap() {
            return new System.Collections.Generic.Dictionary<string, System.Type>();
        }
        private static void map(string s, System.Type a) {
            Saxon.Impl.Helpers.MapUtils.Put(adjunctMap, s, a);
        }
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.expr.Expression expression;
        public static Saxon.Eej.ee.stream.adjunct.StreamingAdjunct makeStreamingAdjunct(Saxon.Hej.Configuration config, Saxon.Hej.expr.Expression exp) {
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adj = createStreamingAdjunct(config, exp);
            adj.setConfiguration(config);
            adj.setExpression(exp);
            return adj;
        }
        private static Saxon.Eej.ee.stream.adjunct.StreamingAdjunct createStreamingAdjunct(Saxon.Hej.Configuration config, Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.sort.DocumentSorter)) {
                Saxon.Hej.expr.Expression operand = ((Saxon.Hej.expr.sort.DocumentSorter)(exp)).getBaseExpression();
                if (((operand is Saxon.Hej.expr.SlashExpression) && ((Saxon.Hej.expr.SlashExpression)(operand)).getActionExpression().isCallOn(typeof(Saxon.Hej.functions.Reverse)))) {
                    ((Saxon.Hej.expr.SlashExpression)(operand)).setStep(((Saxon.Hej.expr.SystemFunctionCall)(((Saxon.Hej.expr.SlashExpression)(operand)).getActionExpression())).getArg(0));
                }
                return new Saxon.Eej.ee.stream.adjunct.DocumentSorterAdjunct();
            } else {
                if ((exp is Saxon.Hej.functions.IntegratedFunctionCall)) {
                    System.Object adjunct = ((Saxon.Hej.functions.IntegratedFunctionCall)(exp)).getFunction().getStreamingImplementation();
                    if ((adjunct is Saxon.Eej.ee.stream.adjunct.StreamingAdjunct)) {
                        return (Saxon.Eej.ee.stream.adjunct.StreamingAdjunct)(adjunct);
                    }
                } else {
                    if ((exp is Saxon.Hej.expr.LetExpression)) {
                        Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.LetExpression)(exp)).getAction();
                        if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(action) != null) && (/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(action) == Saxon.Eej.ee.stream.Sweep.CONSUMING))) {
                            return new Saxon.Eej.ee.stream.adjunct.LetExpressionAdjunctB();
                        } else {
                            return new Saxon.Eej.ee.stream.adjunct.LetExpressionAdjunct();
                        }
                    } else {
                        if ((exp is Saxon.Eej.ee.stream.StreamInstr)) {
                            return adjunctForStreamInstr((Saxon.Eej.ee.stream.StreamInstr)(exp));
                        } else {
                            if ((exp is Saxon.Hej.expr.SystemFunctionCall)) {
                                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(exp);
                                string streamerName = call.getTargetFunction().getStreamerName();
                                Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.StreamerMap.getStreamingAdjunct(streamerName);
                                if ((sa == null)) {
                                    if ((call.getTargetFunction() is Saxon.Hej.functions.ContextItemAccessorFunction)) {
                                        Saxon.Hej.expr.Expression e2 = ((Saxon.Hej.functions.ContextItemAccessorFunction)(call.getTargetFunction())).makeContextItemExplicit();
                                        return makeStreamingAdjunct(config, e2);
                                    }
                                    return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
                                } else {
                                    return sa;
                                }
                            } else {
                                if ((exp is Saxon.Hej.expr.UserFunctionCall)) {
                                    Saxon.Hej.expr.UserFunctionCall ufc = (Saxon.Hej.expr.UserFunctionCall)(exp);
                                    Saxon.Hej.trans.FunctionStreamability ability = ufc.getFunction().getDeclaredStreamability();
                                    if ((ability == Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED)) {
                                    } else {
                                        if ((ability == Saxon.Hej.trans.FunctionStreamability.FILTER)) {
                                            return new Saxon.Eej.ee.stream.adjunct.FilterFunctionCallAdjunct();
                                        } else {
                                            if ((ability == Saxon.Hej.trans.FunctionStreamability.ABSORBING)) {
                                                if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null) && (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(ufc.getArg(0)) == Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                                                    return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
                                                } else {
                                                    return new Saxon.Eej.ee.stream.adjunct.AbsorbingFunctionCallAdjunct();
                                                }
                                            } else {
                                                if ((ability == Saxon.Hej.trans.FunctionStreamability.SHALLOW_DESCENT)) {
                                                    if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null) && (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(ufc.getArg(0)) == Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                                                        return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
                                                    } else {
                                                        return new Saxon.Eej.ee.stream.adjunct.ShallowDescentFunctionCallAdjunct();
                                                    }
                                                } else {
                                                    if ((ability == Saxon.Hej.trans.FunctionStreamability.DEEP_DESCENT)) {
                                                        if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null) && (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(ufc.getArg(0)) == Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                                                            return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
                                                        } else {
                                                            return new Saxon.Eej.ee.stream.adjunct.DeepDescentFunctionCallAdjunct();
                                                        }
                                                    } else {
                                                        if ((ability == Saxon.Hej.trans.FunctionStreamability.ASCENT)) {
                                                            if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null) && (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(ufc.getArg(0)) == Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                                                                return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
                                                            } else {
                                                                return new Saxon.Eej.ee.stream.adjunct.AscentFunctionCallAdjunct();
                                                            }
                                                        } else {
                                                            return new Saxon.Eej.ee.stream.adjunct.UserFunctionCallAdjunct();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((exp.getStreamerName() != null)) {
                System.Type aClass = Saxon.Impl.Helpers.MapUtils.Get(adjunctMap, exp.getStreamerName());
                if ((aClass == null)) {
                    throw new Saxon.Ejava.lang.AssertionError(("No streaming support for "+exp.getStreamerName()));
                }
                try {
                    return (Saxon.Eej.ee.stream.adjunct.StreamingAdjunct)System.Activator.CreateInstance(aClass);
                } catch (System.Exception e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            }
            return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
        }
        private static Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adjunctForStreamInstr(Saxon.Eej.ee.stream.StreamInstr stream) {
            return new Inner_StreamingAdjunct_d301e1400(stream);
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public virtual void setExpression(Saxon.Hej.expr.Expression expression) {
            this.expression = expression;
        }
        public Saxon.Hej.expr.Expression getExpression() {
            return expression;
        }
        public virtual Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.Expression exp = getExpression();
            System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> ops = exp.operands();
            return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, ops, contextInfo, reasons);
        }
        public static int findConsumingArg(Saxon.Hej.expr.Expression expression, int knownConsumingArg) {
            if ((knownConsumingArg == -1)) {
                int i = 0;
                int arg = 0;
                foreach (Saxon.Hej.expr.Operand o in expression.operands()) {
                    if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                        arg = i;
                    }
                    i++;
                }
                if ((arg == -1)) {
                    throw new Saxon.Hej.trans.XPathException("No consuming subexpression found");
                }
                return arg;
            } else {
                return knownConsumingArg;
            }
        }
        public virtual Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return null;
        }
        public virtual Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Eej.ee.stream.Posture posture = /*134*/Saxon.Eej.ee.stream.Streamability.getPosture(getExpression());
            if (((posture == Saxon.Eej.ee.stream.Posture.STRIDING) || (posture == Saxon.Eej.ee.stream.Posture.CRAWLING))) {
                throw new Saxon.Hej.trans.XPathException(("No watch implemented for "+getExpression().getExpressionName()), Saxon.Hej.trans.SaxonErrorCode.SXST0067, getExpression().getLocation());
            }
            return null;
        }
        public virtual Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            if ((this is Saxon.Eej.ee.stream.feed.FeedMaker)) {
                return (Saxon.Eej.ee.stream.feed.FeedMaker)(this);
            }
            return getDefaultFeedMaker(arg);
        }
        protected internal Saxon.Eej.ee.stream.feed.FeedMaker getDefaultFeedMaker(int arg) {
            bool allowsMany = false;
            int n = 0;
            Saxon.Hej.expr.Operand streamingOp = null;
            foreach (Saxon.Hej.expr.Operand o in getExpression().operands()) {
                if ((n++ == arg)) {
                    allowsMany = Saxon.Hej.value.Cardinality.allowsMany(o.getRequiredType().getCardinality());
                    streamingOp = o;
                    break ;
                }
            }
            if (((getExpression() is Saxon.Hej.expr.StaticFunctionCall) && !(allowsMany))) {
                Saxon.Hej.om.Function fn = ((Saxon.Hej.expr.StaticFunctionCall)(getExpression())).getTargetFunction();
                Saxon.Hej.expr.Expression[] args = ((Saxon.Hej.expr.StaticFunctionCall)(getExpression())).getArguments();
                return new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => new Saxon.Eej.ee.stream.feed.CallableFeed(fn, @out, context, args));
            }
            System.Diagnostics.Debug.Assert((streamingOp != null));
            Saxon.Hej.expr.Expression proxy = streamingOp.getChildExpression();
            Saxon.Hej.expr.ConsumingOperand co = new Saxon.Hej.expr.ConsumingOperand(proxy);
            streamingOp.setChildExpression(co);
            co.getOperand().setOperandRole(new Saxon.Hej.expr.OperandRole(0, streamingOp.getUsage()));
            /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(co, /*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(proxy));
            return new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => new Saxon.Eej.ee.stream.feed.BufferingFeed(getExpression(), @out, context));
        }
    
        private class Inner_StreamingAdjunct_d301e1400 : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
            public Inner_StreamingAdjunct_d301e1400(Saxon.Eej.ee.stream.StreamInstr stream) {
                this.stream = stream;
            }
            private Saxon.Eej.ee.stream.StreamInstr stream;
            public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
                /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(/*extraParam*/stream.getHref(), /*localparam*/contextInfo, /*localparam*/reasons);
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, /*134*/Saxon.Eej.ee.stream.Streamability.getSweep(/*extraParam*/stream.getHref()));
            }
        }

    }
}