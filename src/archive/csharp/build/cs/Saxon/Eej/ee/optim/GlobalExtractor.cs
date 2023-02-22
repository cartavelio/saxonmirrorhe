using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class GlobalExtractor {
        private readonly Saxon.Hej.expr.Expression root;
        private readonly Saxon.Hej.trans.GlobalVariableManager globalVariableManager;
        private readonly Saxon.Hej.expr.parser.ExpressionVisitor visitor;
        private readonly Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.expr.Expression,string> ineligibles = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.expr.Expression,string>();
        private readonly bool tracing;
        public const int COST_THRESHOLD = 3;
        public GlobalExtractor(Saxon.Hej.expr.Expression root, Saxon.Hej.trans.GlobalVariableManager globalVariableManager, Saxon.Hej.expr.parser.ExpressionVisitor visitor)  {
            this.root = root;
            this.globalVariableManager = globalVariableManager;
            this.visitor = visitor;
            this.tracing = visitor.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS);
        }
        public Saxon.Hej.expr.Expression extractGlobals() {
            markIneligibleExpressions(root, visitor.isOptimizeForStreaming());
            return processTree(root);
        }
        private double markIneligibleExpressions(Saxon.Hej.expr.Expression expr, bool forStreaming) {
            if (((expr is Saxon.Hej.expr.LetExpression) && ((Saxon.Hej.expr.LetExpression)(expr)).isNeedsLazyEvaluation())) {
                markSubtreeIneligible(expr);
                return 100;
            } else {
                if ((expr is Saxon.Eej.xsltextn.instruct.DoInstr)) {
                    markSubtreeIneligible(expr);
                } else {
                    if ((!(expr.isLiftable(forStreaming)) || (expr is Saxon.Eej.ee.stream.StreamInstr))) {
                        markAncestorsIneligible(expr);
                    } else {
                        if ((((expr.getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT))) != 0)) {
                            Saxon.Impl.Helpers.MapUtils.Put(ineligibles, expr, "this");
                        } else {
                            if ((expr is Saxon.Hej.expr.TryCatch)) {
                                markSubtreeIneligible(expr);
                                return 100;
                            }
                        }
                    }
                }
            }
            double cost = expr.getCost();
            foreach (Saxon.Hej.expr.Operand o in expr.operands()) {
                double subcost = markIneligibleExpressions(o.getChildExpression(), forStreaming);
                if ((subcost>cost)) {
                    cost = subcost;
                }
            }
            if ((cost<COST_THRESHOLD)) {
                markSubtreeIneligible(expr);
            }
            return cost;
        }
        private void markAncestorsIneligible(Saxon.Hej.expr.Expression expr) {
            Saxon.Hej.expr.Expression parent = expr;
            while ((parent != null)) {
                if ((Saxon.Impl.Helpers.MapUtils.Get(ineligibles, parent) == null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(ineligibles, parent, "this");
                }
                parent = parent.getParentExpression();
            }
        }
        private void markSubtreeIneligible(Saxon.Hej.expr.Expression expr) {
            Saxon.Impl.Helpers.MapUtils.Put(ineligibles, expr, "all");
        }
        private Saxon.Hej.expr.Expression processTree(Saxon.Hej.expr.Expression expr) {
            string label = Saxon.Impl.Helpers.MapUtils.Get(ineligibles, expr);
            if ((label == null)) {
                return promoteToGlobal(expr);
            } else {
                if (label.Equals("all")) {
                    return expr;
                } else {
                    if (label.Equals("this")) {
                        foreach (Saxon.Hej.expr.Operand o in expr.operands()) {
                            o.setChildExpression(processTree(o.getChildExpression()));
                        }
                        return expr;
                    } else {
                        throw new Saxon.Ejava.lang.IllegalStateException();
                    }
                }
            }
        }
        private Saxon.Hej.expr.Expression promoteToGlobal(Saxon.Hej.expr.Expression expr) {
            Saxon.Hej.expr.instruct.GlobalVariable v = globalVariableManager.getEquivalentVariable(expr);
            if ((v != null)) {
                Saxon.Hej.expr.GlobalVariableReference varRef = new Saxon.Hej.expr.GlobalVariableReference(v);
                Saxon.Hej.value.SequenceType type = Saxon.Hej.value.SequenceType.makeSequenceType(expr.getItemType(), expr.getCardinality());
                varRef.setStaticType(type, null, (expr.getSpecialProperties()|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
                varRef.setRetainedStaticContext(visitor.getStaticContext().makeRetainedStaticContext());
                return varRef;
            }
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = expr.getRetainedStaticContext();
            Saxon.Hej.expr.instruct.GlobalVariable gvd = new Saxon.Hej.expr.instruct.GlobalVariable();
            gvd.setPackageData(rsc.getPackageData());
            gvd.setSystemId(expr.getLocation().getSystemId());
            gvd.setLineNumber(expr.getLocation().getLineNumber());
            gvd.setColumnNumber(expr.getLocation().getColumnNumber());
            gvd.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(expr.getItemType(), expr.getCardinality()));
            gvd.setBody(expr);
            expr.setRetainedStaticContext(rsc);
            Saxon.Hej.om.StructuredQName varName = new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE, ("gg"+gvd.GetHashCode()));
            gvd.setVariableQName(varName);
            globalVariableManager.addGlobalVariable(gvd);
            Saxon.Hej.expr.GlobalVariableReference @ref = new Saxon.Hej.expr.GlobalVariableReference(gvd);
            /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(@ref, Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS);
            @ref.setRetainedStaticContext(visitor.getStaticContext().makeRetainedStaticContext());
            gvd.registerReference(@ref);
            gvd.typeCheck(visitor);
            Saxon.Hej.expr.instruct.SlotManager localSlotManager = visitor.getConfiguration().makeSlotManager();
            int slots = Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(expr, 0, localSlotManager);
            if ((slots>0)) {
                gvd.setContainsLocals(localSlotManager);
            }
            gvd.init(expr, varName);
            if (tracing) {
                Saxon.Hej.lib.Logger err = visitor.getConfiguration().getLogger();
                err.info(((("OPT : At line "+expr.getLocation().getLineNumber())+" of ")+expr.getLocation().getSystemId()));
                err.info(((("OPT : Extracted global variable "+varName.getDisplayName())+" := ")+expr.ToString()));
            }
            return @ref;
        }
    
    }
}