using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class LoopLifter {
        public static Saxon.Hej.expr.Expression process(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if (((exp is Saxon.Hej.expr.Literal) || (exp is Saxon.Hej.expr.VariableReference))) {
                return exp;
            } else {
                Saxon.Hej.expr.parser.LoopLifter lifter = new Saxon.Hej.expr.parser.LoopLifter(exp, visitor.getConfiguration(), visitor.isOptimizeForStreaming());
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = exp.getRetainedStaticContext();
                lifter.gatherInfo(exp);
                lifter.loopLift(exp);
                lifter.root.setRetainedStaticContext(rsc);
                lifter.root.setParentExpression(null);
                if (lifter.changed) {
                    Saxon.Hej.expr.parser.ExpressionTool.resetPropertiesWithinSubtree(lifter.root);
                    Saxon.Hej.expr.Expression e2 = lifter.root.optimize(visitor, contextInfo);
                    e2.setParentExpression(null);
                    return e2;
                } else {
                    return lifter.root;
                }
            }
        }
        private Saxon.Hej.expr.Expression root;
        private readonly Saxon.Hej.Configuration config;
        private int sequence = 0;
        private bool changed = false;
        private bool tracing = false;
        private bool streaming = false;
        private const string MARKER = "marker";
        private class ExpInfo {
            internal Saxon.Hej.expr.Expression expression;
            internal int loopLevel;
            internal bool multiThreaded;
            internal System.Collections.Generic.IDictionary<Saxon.Hej.expr.Expression,string> dependees = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.expr.Expression, string>();
        
        }
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.Expression,Saxon.Hej.expr.parser.LoopLifter.ExpInfo> expInfoMap = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.expr.Expression, Saxon.Hej.expr.parser.LoopLifter.ExpInfo>();
        public LoopLifter(Saxon.Hej.expr.Expression root, Saxon.Hej.Configuration config, bool streaming)  {
            this.root = root;
            this.config = config;
            this.tracing = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS);
            this.streaming = streaming;
        }
        public Saxon.Hej.expr.Expression getRoot() {
            return root;
        }
        public void gatherInfo(Saxon.Hej.expr.Expression exp) {
            gatherInfo(exp, 0, 0, false);
        }
        private void gatherInfo(Saxon.Hej.expr.Expression exp, int level, int loopLevel, bool multiThreaded) {
            Saxon.Hej.expr.parser.LoopLifter.ExpInfo info = new Saxon.Hej.expr.parser.LoopLifter.ExpInfo();
            info.expression = exp;
            info.loopLevel = loopLevel;
            info.multiThreaded = multiThreaded;
            Saxon.Impl.Helpers.MapUtils.Put(expInfoMap, exp, info);
            Saxon.Hej.expr.Expression scope = exp.getScopingExpression();
            if ((scope != null)) {
                markDependencies(exp, scope);
            }
            bool threaded = (multiThreaded || exp.isMultiThreaded(config));
            Saxon.Hej.expr.Expression choose = getContainingConditional(exp);
            if ((choose != null)) {
                markDependencies(exp, choose);
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                gatherInfo(o.getChildExpression(), (level+1), (o.isEvaluatedRepeatedly() ? (loopLevel+1) : loopLevel), threaded);
            }
        }
        private Saxon.Hej.expr.Expression getContainingConditional(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.Expression parent = exp.getParentExpression();
            while ((parent != null)) {
                if ((parent is Saxon.Hej.expr.instruct.ConditionalInstruction)) {
                    Saxon.Hej.expr.Operand o = Saxon.Hej.expr.parser.ExpressionTool.findOperand(parent, exp);
                    if ((o == null)) {
                        throw new Saxon.Ejava.lang.AssertionError();
                    }
                    if (o.getOperandRole().isInChoiceGroup()) {
                        return parent;
                    }
                } else {
                    if ((parent is Saxon.Hej.expr.TryCatch)) {
                        return parent;
                    }
                }
                exp = parent;
                parent = parent.getParentExpression();
            }
            return null;
        }
        private bool mayReturnStreamedNodes(Saxon.Hej.expr.Expression exp) {
            return (streaming && !(exp.getItemType().getUType().intersection(Saxon.Hej.type.UType.ANY_NODE).Equals(Saxon.Hej.type.UType.VOID)));
        }
        private void markDependencies(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.Expression variableSetter) {
            Saxon.Hej.expr.Expression parent;
            if ((variableSetter != null)) {
                parent = exp;
                while (((parent != null) && (parent != variableSetter))) {
                    try {
                        Saxon.Impl.Helpers.MapUtils.Put(Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, parent).dependees, variableSetter, MARKER);
                    } catch (Saxon.Ejava.lang.NullPointerException e) {
                        Saxon.Hej.expr.parser.ExpressionTool.validateTree(parent);
                        System.Console.Error.WriteLine(e);
                        throw;
                    }
                    parent = parent.getParentExpression();
                }
            }
        }
        private void loopLift(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.parser.LoopLifter.ExpInfo info = Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, exp);
            if (!(info.multiThreaded)) {
                if (((info.loopLevel>0) && (exp.getNetCost()>0))) {
                    if (((info.dependees.Count==0 && exp.isLiftable(streaming)) && !(mayReturnStreamedNodes(exp)))) {
                        root = lift(exp, root);
                    } else {
                        Saxon.Hej.expr.Expression child = exp;
                        Saxon.Hej.expr.parser.LoopLifter.ExpInfo expInfo = Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, exp);
                        Saxon.Hej.expr.Expression parent = exp.getParentExpression();
                        while ((parent != null)) {
                            if (expInfo.dependees.ContainsKey(parent)) {
                                Saxon.Hej.expr.parser.LoopLifter.ExpInfo childInfo = Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, child);
                                if ((expInfo.loopLevel != childInfo.loopLevel)) {
                                    Saxon.Hej.expr.Operand o = Saxon.Hej.expr.parser.ExpressionTool.findOperand(parent, child);
                                    System.Diagnostics.Debug.Assert((o != null));
                                    if (((exp.isLiftable(streaming) && !(((child is Saxon.Hej.expr.PseudoExpression)))) && !(o.getOperandRole().isConstrainedClass()))) {
                                        Saxon.Hej.expr.Expression lifted = lift(exp, child);
                                        o.setChildExpression(lifted);
                                    }
                                }
                                break ;
                            }
                            child = parent;
                            parent = parent.getParentExpression();
                        }
                    }
                }
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    if (!(o.getOperandRole().isConstrainedClass())) {
                        loopLift(o.getChildExpression());
                    }
                }
            }
        }
        private Saxon.Hej.expr.Expression lift(Saxon.Hej.expr.Expression child, Saxon.Hej.expr.Expression newAction) {
            changed = true;
            Saxon.Hej.expr.parser.LoopLifter.ExpInfo childInfo = Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, child);
            Saxon.Hej.expr.parser.LoopLifter.ExpInfo actionInfo = Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, newAction);
            int hoist = (childInfo.loopLevel-actionInfo.loopLevel);
            Saxon.Hej.expr.Expression oldParent = child.getParentExpression();
            Saxon.Hej.expr.Operand oldOperand = Saxon.Hej.expr.parser.ExpressionTool.findOperand(oldParent, child);
            System.Diagnostics.Debug.Assert((oldOperand != null));
            Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
            let.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.lib.NamespaceConstant.SAXON_GENERATED_VARIABLE, ("v"+sequence++)));
            Saxon.Hej.value.SequenceType type = Saxon.Hej.value.SequenceType.makeSequenceType(child.getItemType(), child.getCardinality());
            let.setRequiredType(type);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(child, let);
            let.setSequence(child);
            let.setNeedsLazyEvaluation(true);
            let.setEvaluator((Saxon.Hej.value.Cardinality.allowsMany(child.getCardinality()) ? Saxon.Hej.expr.parser.Evaluator.MemoClosureEvaluator.INSTANCE : Saxon.Hej.expr.parser.Evaluator.SingletonClosure.INSTANCE));
            let.setAction(newAction);
            let.adoptChildExpression(newAction);
            Saxon.Hej.expr.parser.LoopLifter.ExpInfo letInfo = new Saxon.Hej.expr.parser.LoopLifter.ExpInfo();
            letInfo.expression = let;
            letInfo.dependees = childInfo.dependees;
            Saxon.Impl.Helpers.MapUtils.PutAll(letInfo.dependees, actionInfo.dependees);
            letInfo.loopLevel = actionInfo.loopLevel;
            Saxon.Impl.Helpers.MapUtils.Put(expInfoMap, let, letInfo);
            try {
                Saxon.Hej.expr.parser.ExpressionTool.processExpressionTree(child, null, (expression, result) => {
                    Saxon.Hej.expr.parser.LoopLifter.ExpInfo info = Saxon.Impl.Helpers.MapUtils.Get(expInfoMap, expression);
                    info.loopLevel -= hoist;
                    return false;
                } );
            } catch (Saxon.Hej.trans.XPathException e) {
                System.Console.Error.WriteLine(e);
            }
            Saxon.Hej.expr.LocalVariableReference var = new Saxon.Hej.expr.LocalVariableReference(let);
            int properties = (child.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC);
            var.setStaticType(type, null, properties);
            var.setInLoop(true);
            let.addReference(var, true);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(child, var);
            oldOperand.setChildExpression(var);
            if (tracing) {
                Saxon.Hej.lib.Logger err = config.getLogger();
                err.info(((("OPT : At line "+child.getLocation().getLineNumber())+" of ")+child.getLocation().getSystemId()));
                err.info(((((("OPT : Lifted ("+child.toShortString())+") above (")+newAction.toShortString())+") on line ")+newAction.getLocation().getLineNumber()));
                err.info(("OPT : Expression after rewrite: "+let));
            }
            return let;
        }
    
    }
}