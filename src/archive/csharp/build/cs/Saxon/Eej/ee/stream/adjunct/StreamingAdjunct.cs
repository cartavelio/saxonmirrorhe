using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class StreamingAdjunct {

        static StreamingAdjunct() {
            map("AdjacentTextNodeMerger", () => new Saxon.Eej.ee.stream.adjunct.AdjacentTextNodeMergerAdjunct());
            map("ApplyImports", () => new Saxon.Eej.ee.stream.adjunct.ApplyImportsAdjunct());
            map("ApplyTemplates", () => new Saxon.Eej.ee.stream.adjunct.ApplyTemplatesAdjunct());
            map("ArrayBlock", () => new Saxon.Eej.ee.stream.adjunct.ArrayBlockAdjunct());
            map("AtomicSequenceConverter", () => new Saxon.Eej.ee.stream.adjunct.AtomicSequenceConverterAdjunct());
            map("Atomizer", () => new Saxon.Eej.ee.stream.adjunct.AtomizerAdjunct());
            map("AxisExpression", () => new Saxon.Eej.ee.stream.adjunct.AxisExpressionAdjunct());
            map("Block", () => new Saxon.Eej.ee.stream.adjunct.BlockAdjunct());
            map("CallTemplate", () => new Saxon.Eej.ee.stream.adjunct.CallTemplateAdjunct());
            map("CardinalityChecker", () => new Saxon.Eej.ee.stream.adjunct.CardinalityCheckerAdjunct());
            map("Choose", () => new Saxon.Eej.ee.stream.adjunct.ChooseAdjunct());
            map("ConditionalBlock", () => new Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct());
            map("ContextItemExpr", () => new Saxon.Eej.ee.stream.adjunct.ContextItemExprAdjunct());
            map("Copy", () => new Saxon.Eej.ee.stream.adjunct.CopyAdjunct());
            map("CopyOf", () => new Saxon.Eej.ee.stream.adjunct.CopyOfAdjunct());
            map("CurrentGroup", () => new Saxon.Eej.ee.stream.adjunct.CurrentGroupStreamer());
            map("DocumentInstr", () => new Saxon.Eej.ee.stream.adjunct.DocumentInstrAdjunct());
            map("DocumentSorterAdjunct", () => new Saxon.Eej.ee.stream.adjunct.DocumentSorterAdjunct());
            map("ElementCreator", () => new Saxon.Eej.ee.stream.adjunct.ElementCreatorAdjunct());
            map("EmptyTextNodeRemover", () => new Saxon.Eej.ee.stream.adjunct.EmptyTextNodeRemoverAdjunct());
            map("FilterExpression", () => new Saxon.Eej.ee.stream.adjunct.FilterExpressionAdjunct());
            map("FirstItemExpression", () => new Saxon.Eej.ee.stream.adjunct.FirstItemExpressionAdjunct());
            map("ForEach", () => new Saxon.Eej.ee.stream.adjunct.ForEachAdjunct());
            map("ForEachGroup", () => new Saxon.Eej.ee.stream.adjunct.ForEachGroupAdjunct());
            map("ForExpression", () => new Saxon.Eej.ee.stream.adjunct.ForExpressionAdjunct());
            map("Fork", () => new Saxon.Eej.ee.stream.adjunct.ForkAdjunct());
            map("GeneralComparison", () => new Saxon.Eej.ee.stream.adjunct.GeneralComparisonAdjunct());
            map("InstanceOf", () => new Saxon.Eej.ee.stream.adjunct.InstanceOfAdjunct());
            map("IsLastExpr", () => new Saxon.Eej.ee.stream.adjunct.IsLastExprAdjunct());
            map("ItemChecker", () => new Saxon.Eej.ee.stream.adjunct.ItemCheckerAdjunct());
            map("Iterate", () => new Saxon.Eej.ee.stream.adjunct.IterateAdjunct());
            map("Literal", () => new Saxon.Eej.ee.stream.adjunct.LiteralAdjunct());
            map("MergeInstr", () => new Saxon.Eej.ee.stream.adjunct.MergeInstrAdjunct());
            map("NextIteration", () => new Saxon.Eej.ee.stream.adjunct.NextIterationAdjunct());
            map("NextMatch", () => new Saxon.Eej.ee.stream.adjunct.NextMatchAdjunct());
            map("OnEmpty", () => new Saxon.Eej.ee.stream.adjunct.OnEmptyAdjunct());
            map("OnNonEmpty", () => new Saxon.Eej.ee.stream.adjunct.OnNonEmptyAdjunct());
            map("RootExpression", () => new Saxon.Eej.ee.stream.adjunct.RootExpressionAdjunct());
            map("ResultDocument", () => new Saxon.Eej.ee.stream.adjunct.ResultDocumentAdjunct());
            map("SequenceInstr", () => new Saxon.Eej.ee.stream.adjunct.SequenceInstrAdjunct());
            map("SimpleNodeConstructor", () => new Saxon.Eej.ee.stream.adjunct.SimpleNodeConstructorAdjunct());
            map("SingletonAtomizer", () => new Saxon.Eej.ee.stream.adjunct.SingletonAtomizerAdjunct());
            map("SlashExpression", () => new Saxon.Eej.ee.stream.adjunct.ForEachAdjunct());
            map("SortExpression", () => new Saxon.Eej.ee.stream.adjunct.SortExpressionAdjunct());
            map("SubscriptExpression", () => new Saxon.Eej.ee.stream.adjunct.SubscriptExpressionAdjunct());
            map("SwitchExpression", () => new Saxon.Eej.ee.stream.adjunct.SwitchAdjunct());
            map("TailExpression", () => new Saxon.Eej.ee.stream.adjunct.TailExpressionAdjunct());
            map("TraceExpr", () => new Saxon.Eej.ee.stream.adjunct.TraceExprAdjunct());
            map("TryCatch", () => new Saxon.Eej.ee.stream.adjunct.TryCatchAdjunct());
            map("UseAttributeSet", () => new Saxon.Eej.ee.stream.adjunct.UseAttributeSetAdjunct());
            map("VariableReference", () => new Saxon.Eej.ee.stream.adjunct.VariableReferenceAdjunct());
            map("VennExpression", () => new Saxon.Eej.ee.stream.adjunct.VennExpressionAdjunct());
            map("WherePopulated", () => new Saxon.Eej.ee.stream.adjunct.WherePopulatedAdjunct());
        }
        private static readonly System.Collections.Generic.IDictionary<string,Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct>> adjunctMap = emptyAdjunctMap();
        public static System.Collections.Generic.IDictionary<string,Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct>> emptyAdjunctMap() {
            return new System.Collections.Generic.Dictionary<string, Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct>>();
        }
        private static void map(string s, Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct> a) {
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
                Saxon.Ejava.util.function.Supplier<Saxon.Eej.ee.stream.adjunct.StreamingAdjunct> supplier = Saxon.Impl.Helpers.MapUtils.Get(adjunctMap, exp.getStreamerName());
                if ((supplier == null)) {
                    throw new Saxon.Ejava.lang.AssertionError(("No streaming support for "+exp.getStreamerName()));
                }
                try {
                    return supplier();
                } catch (System.Exception e) {
                    throw new Saxon.Ejava.lang.AssertionError(e);
                }
            }
            return new Saxon.Eej.ee.stream.adjunct.StreamingAdjunct();
        }
        private static Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adjunctForStreamInstr(Saxon.Eej.ee.stream.StreamInstr stream) {
            return new Inner_StreamingAdjunct_167(stream);
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
                Saxon.Hej.om.FunctionItem fn = ((Saxon.Hej.expr.StaticFunctionCall)(getExpression())).getTargetFunction();
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
        public static string genId(Saxon.Hej.om.NodeInfo node) {
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(16);
            node.generateId(buffer);
            return buffer.ToString();
        }
    
        private class Inner_StreamingAdjunct_167 : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
            public Inner_StreamingAdjunct_167(Saxon.Eej.ee.stream.StreamInstr stream) {
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