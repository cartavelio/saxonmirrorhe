using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class Optimizer {
        protected internal Saxon.Hej.Configuration config;
        private Saxon.Hej.expr.parser.OptimizerOptions optimizerOptions = Saxon.Hej.expr.parser.OptimizerOptions.FULL_EE_OPTIMIZATION;
        protected internal bool tracing;
        public Optimizer(Saxon.Hej.Configuration config)  {
            this.config = config;
            this.tracing = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS);
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setOptimizerOptions(Saxon.Hej.expr.parser.OptimizerOptions options) {
            optimizerOptions = options;
        }
        public Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions() {
            return optimizerOptions;
        }
        public bool isOptionSet(int option) {
            return optimizerOptions.isSet(option);
        }
        public virtual Saxon.Hej.expr.Expression optimizeValueComparison(Saxon.Hej.expr.ValueComparison vc, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression lhs = vc.getLhsExpression();
            Saxon.Hej.expr.Expression rhs = vc.getRhsExpression();
            Saxon.Hej.expr.Expression e2 = optimizePositionVsLast(lhs, rhs, vc.getOperator());
            if ((e2 != null)) {
                trace("Rewrote position() ~= last()", e2);
                return e2;
            }
            e2 = optimizePositionVsLast(rhs, lhs, Saxon.Hej.expr.parser.Token.inverse(vc.getOperator()));
            if ((e2 != null)) {
                trace("Rewrote last() ~= position()", e2);
                return e2;
            }
            return vc;
        }
        private Saxon.Hej.expr.Expression optimizePositionVsLast(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs, int @operator) {
            if ((lhs.isCallOn(typeof(Saxon.Hej.functions.PositionAndLast.Position)) && rhs.isCallOn(typeof(Saxon.Hej.functions.PositionAndLast.Last)))) {
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                  case Saxon.Hej.expr.parser.Token.FGE:
                    Saxon.Hej.expr.IsLastExpression iletrue = new Saxon.Hej.expr.IsLastExpression(true);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(lhs, iletrue);
                    return iletrue;
                  case Saxon.Hej.expr.parser.Token.FNE:
                  case Saxon.Hej.expr.parser.Token.FLT:
                    Saxon.Hej.expr.IsLastExpression ilefalse = new Saxon.Hej.expr.IsLastExpression(false);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(lhs, ilefalse);
                    return ilefalse;
                  case Saxon.Hej.expr.parser.Token.FGT:
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, lhs);
                  case Saxon.Hej.expr.parser.Token.FLE:
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, lhs);
                }
            }
            return null;
        }
        public virtual Saxon.Hej.expr.Expression optimizeGeneralComparison(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.GeneralComparison gc, bool backwardsCompatible, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return gc;
        }
        public virtual Saxon.Hej.expr.Expression optimizeSaxonStreamFunction(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cisi, Saxon.Hej.expr.Expression select) {
            if (select.getItemType().isPlainType()) {
                return select;
            }
            return null;
        }
        public virtual Saxon.Hej.expr.Expression convertPathExpressionToKey(Saxon.Hej.expr.SlashExpression pathExp, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            return null;
        }
        public virtual Saxon.Hej.expr.Expression tryIndexedFilter(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, bool indexFirstOperand, bool contextIsDoc) {
            return f;
        }
        public virtual Saxon.Hej.expr.FilterExpression reorderPredicates(Saxon.Hej.expr.FilterExpression f, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cisi) {
            return f;
        }
        public virtual Saxon.Hej.expr.FilterExpression convertToFilterExpression(Saxon.Hej.expr.SlashExpression pathExp, Saxon.Hej.type.TypeHierarchy th) {
            return null;
        }
        public virtual int isIndexableFilter(Saxon.Hej.expr.Expression filter) {
            return 0;
        }
        public virtual Saxon.Hej.om.GroundedValue makeIndexedValue(Saxon.Hej.om.SequenceIterator iter) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Indexing requires Saxon-EE");
        }
        public virtual void optimizeNodeSetPattern(Saxon.Hej.pattern.NodeSetPattern pattern) {
        }
        public virtual void prepareForStreaming(Saxon.Hej.expr.Expression exp) {
        }
        public virtual Saxon.Hej.om.Sequence evaluateStreamingArgument(Saxon.Hej.expr.Expression expr, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(expr, context);
        }
        public bool isVariableReplaceableByDot(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Binding[] binding) {
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                if (o.hasSameFocus()) {
                    if (!(isVariableReplaceableByDot(o.getChildExpression(), binding))) {
                        return false;
                    }
                } else {
                    if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(o.getChildExpression(), binding)) {
                        return false;
                    }
                }
            }
            return true;
        }
        public virtual Saxon.Hej.expr.Expression makeConditionalDocumentSorter(Saxon.Hej.expr.sort.DocumentSorter sorter, Saxon.Hej.expr.SlashExpression path) {
            return sorter;
        }
        public virtual Saxon.Hej.expr.Expression tryInlineFunctionCall(Saxon.Hej.expr.UserFunctionCall functionCall, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return functionCall;
        }
        public virtual Saxon.Hej.expr.Expression promoteExpressionsToGlobal(Saxon.Hej.expr.Expression body, Saxon.Hej.trans.GlobalVariableManager gvManager, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            return null;
        }
        public virtual Saxon.Hej.expr.Expression eliminateCommonSubexpressions(Saxon.Hej.expr.Expression @in) {
            return @in;
        }
        public virtual Saxon.Hej.expr.Expression trySwitch(Saxon.Hej.expr.instruct.Choose choose, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            return choose;
        }
        public virtual Saxon.Hej.expr.Expression tryGeneralComparison(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.OrExpression orExpr) {
            return orExpr;
        }
        public Saxon.Hej.trans.rules.RuleTarget makeInversion(Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.expr.instruct.NamedTemplate template) {
            return null;
        }
        public virtual void makeCopyOperationsExplicit(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Operand child) {
        }
        public virtual void checkStreamability(Saxon.Hej.style.XSLTemplate sourceTemplate, Saxon.Hej.expr.instruct.TemplateRule compiledTemplate) {
        }
        public virtual Saxon.Hej.expr.Expression optimizeForExpressionForStreaming(Saxon.Hej.expr.ForExpression expr) {
            return expr;
        }
        public virtual Saxon.Hej.expr.Expression optimizeQuantifiedExpressionForStreaming(Saxon.Hej.expr.QuantifiedExpression expr) {
            return expr;
        }
        public virtual Saxon.Hej.expr.Expression generateMultithreadedInstruction(Saxon.Hej.expr.Expression instruction) {
            return instruction;
        }
        public Saxon.Hej.expr.Expression compileToByteCode(Saxon.Hej.expr.parser.ICompilerService compilerService, Saxon.Hej.expr.Expression expr, string objectName, int evaluationMethods) {
            return null;
        }
        public Saxon.Hej.expr.Expression makeByteCodeCandidate(Saxon.Hej.expr.ExpressionOwner owner, Saxon.Hej.expr.Expression expr, string objectName, int requiredEvaluationModes) {
            return expr;
        }
        public void injectByteCodeCandidates(Saxon.Hej.expr.Expression exp) {
        }
        public virtual Saxon.Hej.expr.Expression optimizeNumberInstruction(Saxon.Hej.expr.instruct.NumberInstruction ni, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return null;
        }
        public virtual void assessFunctionStreamability(Saxon.Hej.style.XSLFunction reporter, Saxon.Hej.expr.instruct.UserFunction compiledFunction) {
            throw new Saxon.Hej.trans.XPathException("Streamable stylesheet functions are not supported in Saxon-HE", "XTSE3430");
        }
        public void trace(string message, Saxon.Hej.expr.Expression exp) {
            if (tracing) {
                Saxon.Hej.lib.Logger err = getConfiguration().getLogger();
                err.info(((("OPT : At line "+exp.getLocation().getLineNumber())+" of ")+exp.getLocation().getSystemId()));
                err.info(("OPT : "+message));
                err.info(("OPT : Expression after rewrite: "+exp));
                exp.verifyParentPointers();
            }
        }
        public static void trace(Saxon.Hej.Configuration config, string message, Saxon.Hej.expr.Expression exp) {
            if (config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS)) {
                Saxon.Hej.lib.Logger err = config.getLogger();
                err.info(((("OPT : At line "+exp.getLocation().getLineNumber())+" of ")+exp.getLocation().getSystemId()));
                err.info(("OPT : "+message));
                err.info(("OPT : Expression after rewrite: "+exp));
                exp.verifyParentPointers();
            }
        }
    
    }
}