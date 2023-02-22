using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class Streamability {
        public static Saxon.Hej.expr.Operand getConsumingOperand(Saxon.Hej.expr.Expression exp) {
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                Saxon.Hej.expr.Expression e = o.getChildExpression();
                Saxon.Eej.ee.stream.PostureAndSweep ps = getPostureAndSweepIfKnown(e);
                if ((ps == null)) {
                    return null;
                }
                if ((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                    return o;
                }
                if (((o.getUsage() == Saxon.Hej.expr.OperandUsage.TRANSMISSION) && (ps.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                    return o;
                }
                if (((o.getUsage() == Saxon.Hej.expr.OperandUsage.ABSORPTION) && isIncrementalPosture(ps.getPosture()))) {
                    return o;
                }
            }
            return null;
        }
        public static Saxon.Hej.expr.Expression rewriteQuantifiedExpressionAsFilterExpression(Saxon.Hej.expr.QuantifiedExpression quant) {
            Saxon.Hej.expr.Expression select = quant.getSequence();
            Saxon.Hej.expr.Expression test = quant.getAction().copy(new Saxon.Hej.expr.parser.RebindingMap());
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = quant.getRetainedStaticContext();
            test = replaceVariableReferenceByDot(test, quant);
            if ((test != null)) {
                if ((quant.getOperator() == Saxon.Hej.expr.parser.Token.SOME)) {
                    Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("boolean", rsc, test);
                    Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(select, fn);
                    return Saxon.Hej.functions.SystemFunction.makeCall("exists", rsc, fe);
                } else {
                    Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("not", rsc, test);
                    Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(select, fn);
                    return Saxon.Hej.functions.SystemFunction.makeCall("empty", rsc, fe);
                }
            }
            return null;
        }
        public static Saxon.Hej.expr.Expression rewriteForExpressionAsMappingExpression(Saxon.Hej.expr.ForExpression forEx) {
            Saxon.Hej.expr.Expression select = forEx.getSequence();
            Saxon.Hej.expr.Expression step = forEx.getAction().copy(new Saxon.Hej.expr.parser.RebindingMap());
            step = replaceVariableReferenceByDot(step, forEx);
            if ((step != null)) {
                return new Saxon.Hej.expr.instruct.ForEach(select, step);
            }
            return null;
        }
        private static Saxon.Hej.expr.Expression replaceVariableReferenceByDot(Saxon.Hej.expr.Expression test, Saxon.Hej.expr.Binding binding) {
            foreach (Saxon.Hej.expr.Operand info in test.operands()) {
                if ((!(info.hasSameFocus()) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(info.getChildExpression(), new Saxon.Hej.expr.Binding[]{binding}))) {
                    return null;
                }
                if (((info.getChildExpression() is Saxon.Hej.expr.LocalVariableReference) && (((Saxon.Hej.expr.LocalVariableReference)(info.getChildExpression())).getBinding() == binding))) {
                    info.setChildExpression(new Saxon.Hej.expr.ContextItemExpression());
                } else {
                    Saxon.Hej.expr.Expression e2 = replaceVariableReferenceByDot(info.getChildExpression(), binding);
                    if ((e2 == null)) {
                        return null;
                    }
                }
            }
            return test;
        }
        public static Saxon.Eej.ee.stream.PostureAndSweep generalStreamabilityRules(Saxon.Hej.expr.Expression target, System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands, Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if (target.getRetainedStaticContext().isBackwardsCompatibility()) {
                reasons.Add("Instructions with effective version='1.0' are not streamable");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Hej.expr.Operand potentiallyConsumingOperand = null;
            Saxon.Eej.ee.stream.PostureAndSweep postureAndSweepOfPotentiallyConsumingOperand = null;
            Saxon.Eej.ee.stream.Sweep widestInChoiceGroup = Saxon.Eej.ee.stream.Sweep.MOTIONLESS;
            System.Collections.Generic.ISet<Saxon.Eej.ee.stream.Posture> choiceGroupPostures = new System.Collections.Generic.HashSet<Saxon.Eej.ee.stream.Posture>();
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE newFocus = null;
            foreach (Saxon.Hej.expr.Operand o in operands) {
                if (o.setsNewFocus()) {
                    getStreamability(o.getChildExpression(), contextInfo, reasons);
                    newFocus = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(target.getConfiguration().makeContextItemStaticInfo(getItemType(o.getChildExpression(), contextInfo), false));
                    newFocus.setContextSettingExpression(o.getChildExpression());
                }
            }
            bool foundOperand = false;
            foreach (Saxon.Hej.expr.Operand o in operands) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                foundOperand = true;
                Saxon.Hej.expr.OperandUsage usage = o.getUsage();
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE info2;
                if (o.hasSameFocus()) {
                    info2 = contextInfo;
                } else {
                    if (o.hasSpecialFocusRules()) {
                        info2 = getCustomStaticInfo(o);
                    } else {
                        System.Diagnostics.Debug.Assert((newFocus != null));
                        info2 = newFocus;
                    }
                }
                Saxon.Eej.ee.stream.PostureAndSweep ps = getStreamability(child, info2, reasons);
                Saxon.Eej.ee.stream.Posture p = ps.getPosture();
                Saxon.Eej.ee.stream.Sweep s = ps.getSweep();
                if (((s == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) || (p == Saxon.Eej.ee.stream.Posture.ROAMING))) {
                    s = Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
                } else {
                    if ((p == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                    } else {
                        Saxon.Hej.type.ItemType t = getItemType(child, info2);
                        if (((usage == Saxon.Hej.expr.OperandUsage.ABSORPTION) && !(t.getUType().overlaps(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.ELEMENT))))) {
                            usage = Saxon.Hej.expr.OperandUsage.INSPECTION;
                        }
                        if ((usage == Saxon.Hej.expr.OperandUsage.NAVIGATION)) {
                            if ((reasons != null)) {
                                reasons.Add((((describeOperand(target, o)+" selects streamed nodes in a context that allows arbitrary navigation (line ")+o.getChildExpression().getLocation().getLineNumber())+")"));
                            }
                            s = Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
                        } else {
                            if ((usage == Saxon.Hej.expr.OperandUsage.ABSORPTION)) {
                                if ((p == Saxon.Eej.ee.stream.Posture.CLIMBING)) {
                                    if ((reasons != null)) {
                                        reasons.Add((((describeOperand(target, o)+" reads the string value or typed value of a node in climbing posture (line ")+o.getChildExpression().getLocation().getLineNumber())+")"));
                                    }
                                    s = Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
                                } else {
                                    if ((p == Saxon.Eej.ee.stream.Posture.CRAWLING)) {
                                        s = Saxon.Eej.ee.stream.Sweep.CONSUMING;
                                    } else {
                                        if ((p == Saxon.Eej.ee.stream.Posture.STRIDING)) {
                                            s = Saxon.Eej.ee.stream.Sweep.CONSUMING;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if ((s == Saxon.Eej.ee.stream.Sweep.FREE_RANGING)) {
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.ROAMING, Saxon.Eej.ee.stream.Sweep.FREE_RANGING);
                }
                if (((s == Saxon.Eej.ee.stream.Sweep.CONSUMING) || (((usage == Saxon.Hej.expr.OperandUsage.TRANSMISSION) && (p != Saxon.Eej.ee.stream.Posture.GROUNDED))))) {
                    if ((potentiallyConsumingOperand != null)) {
                        if ((reasons != null)) {
                            Saxon.Hej.expr.Expression e0 = potentiallyConsumingOperand.getChildExpression();
                            Saxon.Hej.expr.Expression e1 = o.getChildExpression();
                            int line0 = e0.getLocation().getLineNumber();
                            int line1 = e1.getLocation().getLineNumber();
                            string suffix;
                            if ((line0 == line1)) {
                                suffix = ((((("{"+e0.toShortString())+"} and {")+e1.toShortString())+"}, both on line ")+line0);
                            } else {
                                suffix = ((((((("{"+e0.toShortString())+"} on line ")+line0)+", and {")+e1.toShortString())+"} on line ")+line1);
                            }
                            if (((s == Saxon.Eej.ee.stream.Sweep.CONSUMING) && (getSweep(potentiallyConsumingOperand.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING))) {
                                reasons.Add(("There is more than one consuming operand: "+suffix));
                            } else {
                                reasons.Add(("There is more than one potentially consuming operand: "+suffix));
                            }
                        }
                        return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.ROAMING, Saxon.Eej.ee.stream.Sweep.FREE_RANGING);
                    }
                    if (o.isInChoiceGroup()) {
                        widestInChoiceGroup = wider(widestInChoiceGroup, s);
                        choiceGroupPostures.Add(p);
                    } else {
                        if (!(choiceGroupPostures.Count==0)) {
                            if ((reasons != null)) {
                                reasons.Add((("There is a consuming operand in a test condition (line "+o.getChildExpression().getLocation().getLineNumber())+"), and another in a previous conditional branch"));
                            }
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.ROAMING, Saxon.Eej.ee.stream.Sweep.FREE_RANGING);
                        } else {
                            potentiallyConsumingOperand = o;
                            postureAndSweepOfPotentiallyConsumingOperand = ps;
                        }
                    }
                }
            }
            if (!(foundOperand)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
            }
            if (!(choiceGroupPostures.Count==0)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(combinedPosture(choiceGroupPostures), widestInChoiceGroup);
            }
            if ((potentiallyConsumingOperand != null)) {
                if (potentiallyConsumingOperand.isEvaluatedRepeatedly()) {
                    if ((reasons != null)) {
                        reasons.Add("A consuming operand is evaluated repeatedly");
                    }
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
                Saxon.Hej.expr.OperandUsage u = potentiallyConsumingOperand.getUsage();
                if (((u == Saxon.Hej.expr.OperandUsage.ABSORPTION) || (u == Saxon.Hej.expr.OperandUsage.INSPECTION))) {
                    return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                }
                return postureAndSweepOfPotentiallyConsumingOperand;
            }
            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
        }
        private static Saxon.Hej.type.ItemType getItemType(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.ContextItemStaticInfo info) {
            if (info.isStrictStreamabilityRules()) {
                return exp.getStaticUType(info.getItemType().getUType()).toItemType();
            } else {
                return exp.getItemType();
            }
        }
        private static Saxon.Eej.ee.trans.ContextItemStaticInfoEE getCustomStaticInfo(Saxon.Hej.expr.Operand o) {
            Saxon.Hej.expr.Expression exp = o.getParentExpression();
            if ((exp is Saxon.Hej.expr.sort.SortKeyDefinition)) {
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE ciso = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(exp.getItemType(), false);
                ciso.setContextPostureGrounded();
                return ciso;
            } else {
                if ((exp is Saxon.Hej.expr.instruct.SourceDocument)) {
                    Saxon.Eej.ee.trans.ContextItemStaticInfoEE ciso = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false);
                    ciso.setContextPostureGrounded();
                    return ciso;
                }
            }
            System.Diagnostics.Debug.Assert(false);
            return null;
        }
        private static string describeOperand(Saxon.Hej.expr.Expression target, Saxon.Hej.expr.Operand o) {
            if ((((o.getChildExpression() is Saxon.Hej.expr.ContextItemExpression) && (target is Saxon.Hej.expr.LetExpression)) && ((Saxon.Hej.expr.LetExpression)(target)).getVariableQName().getLocalPart().StartsWith("current"))) {
                return "The current() function";
            } else {
                return (((("Operand {"+o.getChildExpression().toShortString())+"} of {")+target.toShortString())+"}");
            }
        }
        public static bool isChildlessNodeKind(Saxon.Hej.type.ItemType type) {
            return (((type is Saxon.Hej.pattern.NodeTest)) && !(type.getUType().overlaps(Saxon.Hej.type.UType.PARENT_NODE_KINDS)));
        }
        public static Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.expr.Expression expr, Saxon.Hej.Configuration config) {
            if ((expr is Saxon.Hej.expr.instruct.Instruction)) {
                return null;
            }
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adj = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(config, expr);
            Saxon.Hej.pattern.Pattern pat = adj.toStreamingPattern(config);
            if ((pat != null)) {
                pat.setRecoverable(false);
            }
            return pat;
        }
        public static Saxon.Eej.ee.stream.PostureAndSweep getStreamability(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Eej.ee.stream.PostureAndSweep postureAndSweep = (Saxon.Eej.ee.stream.PostureAndSweep)(exp.getExtraProperty("P+S"));
            if ((postureAndSweep == null)) {
                if ((exp is Saxon.Hej.pattern.Pattern)) {
                    if (((contextInfo.getContextItemPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED) || ((Saxon.Hej.pattern.Pattern)(exp)).isMotionless())) {
                        postureAndSweep = Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
                    } else {
                        if ((reasons != null)) {
                            reasons.Add((("The pattern "+exp.toShortString())+" is not motionless"));
                        }
                        postureAndSweep = Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                    }
                } else {
                    if (exp.isCallOn(typeof(Saxon.Hej.functions.AccumulatorFn.AccumulatorAfter))) {
                        Saxon.Eej.ee.stream.Sweep sweep = computeAccumulatorAfterSweep((Saxon.Hej.expr.SystemFunctionCall)(exp), contextInfo, reasons);
                        postureAndSweep = new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, sweep);
                    } else {
                        Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adjunct = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(exp.getConfiguration(), exp);
                        postureAndSweep = adjunct.computeStreamability(contextInfo, reasons);
                    }
                }
                exp.setExtraProperty("P+S", postureAndSweep);
            }
            return postureAndSweep;
        }
        public static Saxon.Eej.ee.stream.Posture getPosture(Saxon.Hej.expr.Expression exp) {
            Saxon.Eej.ee.stream.PostureAndSweep postureAndSweep = getPostureAndSweepIfKnown(exp);
            if ((postureAndSweep != null)) {
                return postureAndSweep.getPosture();
            } else {
                throw new Saxon.Ejava.lang.IllegalStateException("getPosture() called when streamability has not yet been computed");
            }
        }
        public static Saxon.Eej.ee.stream.Sweep getSweep(Saxon.Hej.expr.Expression exp) {
            Saxon.Eej.ee.stream.PostureAndSweep postureAndSweep = getPostureAndSweepIfKnown(exp);
            if ((postureAndSweep != null)) {
                return postureAndSweep.getSweep();
            } else {
                throw new Saxon.Ejava.lang.IllegalStateException((exp.GetType().Name+".getSweep() called when streamability has not yet been computed"));
            }
        }
        public static Saxon.Eej.ee.stream.PostureAndSweep getPostureAndSweepIfKnown(Saxon.Hej.expr.Expression exp) {
            return (Saxon.Eej.ee.stream.PostureAndSweep)(exp.getExtraProperty("P+S"));
        }
        public static void setPostureAndSweep(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.PostureAndSweep ps) {
            exp.setExtraProperty("P+S", ps);
        }
        public static Saxon.Eej.ee.stream.Sweep computeAccumulatorAfterSweep(Saxon.Hej.expr.SystemFunctionCall call, Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            bool strict = contextInfo.isStrictStreamabilityRules();
            Saxon.Eej.ee.stream.Sweep firstArgSweep = getStreamability(call.getArg(0), contextInfo, reasons).getSweep();
            if ((firstArgSweep != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                reasons.Add(("First argument of accumulator-after() is "+firstArgSweep));
                return Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
            }
            if ((contextInfo.getContextItemPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return Saxon.Eej.ee.stream.Sweep.MOTIONLESS;
            }
            if (contextInfo.getItemType().getUType().intersection(Saxon.Hej.type.UType.PARENT_NODE_KINDS).Equals(Saxon.Hej.type.UType.VOID)) {
                return Saxon.Eej.ee.stream.Sweep.MOTIONLESS;
            }
            Saxon.Hej.functions.AccumulatorFn.Phase phase = contextInfo.getAccumulatorPhase();
            if ((phase == Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE)) {
                return Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
            }
            if ((phase == Saxon.Hej.functions.AccumulatorFn.Phase.AFTER)) {
                return Saxon.Eej.ee.stream.Sweep.MOTIONLESS;
            }
            Saxon.Hej.expr.Expression focusSettingContainer = Saxon.Hej.expr.parser.ExpressionTool.getFocusSettingContainer(call);
            Saxon.Hej.expr.Expression parent;
            if ((focusSettingContainer != null)) {
                parent = call.getParentExpression();
                while (((parent != null) && !(((parent is Saxon.Hej.expr.instruct.Instruction))))) {
                    parent = parent.getParentExpression();
                }
                if (((parent != null) && (focusSettingContainer != Saxon.Hej.expr.parser.ExpressionTool.getFocusSettingContainer(parent)))) {
                    reasons.Add("Call on accumulator-after() has different focus from containing instruction");
                    return Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
                }
            }
            parent = call;
            bool foundPrecedingConsumingExpression = false;
            while ((parent != null)) {
                Saxon.Hej.expr.Expression grandma = parent.getParentExpression();
                if ((parent.isInstruction() || !(strict))) {
                    if ((grandma != null)) {
                        foreach (Saxon.Hej.expr.Operand sib in grandma.operands()) {
                            if ((sib.getChildExpression() == parent)) {
                                break ;
                            }
                            Saxon.Eej.ee.stream.PostureAndSweep pas = getStreamability(sib.getChildExpression(), contextInfo, reasons);
                            if ((pas.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                                foundPrecedingConsumingExpression = true;
                                break ;
                            }
                        }
                    }
                    if (foundPrecedingConsumingExpression) {
                        break ;
                    }
                }
                parent = grandma;
            }
            if (!(foundPrecedingConsumingExpression)) {
                reasons.Add("A call to accumulator-after() is consuming when there are no preceding consuming instructions");
                return Saxon.Eej.ee.stream.Sweep.CONSUMING;
            }
            return Saxon.Eej.ee.stream.Sweep.MOTIONLESS;
        }
        public static Saxon.Eej.ee.stream.Sweep wider(Saxon.Eej.ee.stream.Sweep a, Saxon.Eej.ee.stream.Sweep b) {
            switch (a) {
              case Saxon.Eej.ee.stream.Sweep.FREE_RANGING:
                return a;
              case Saxon.Eej.ee.stream.Sweep.CONSUMING:
                return ((b == Saxon.Eej.ee.stream.Sweep.FREE_RANGING) ? b : a);
              case Saxon.Eej.ee.stream.Sweep.MOTIONLESS:
                return b;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public static bool isIncrementalPosture(Saxon.Eej.ee.stream.Posture posture) {
            return ((posture == Saxon.Eej.ee.stream.Posture.STRIDING) || (posture == Saxon.Eej.ee.stream.Posture.CRAWLING));
        }
        public static Saxon.Eej.ee.stream.Posture combinedPosture(System.Collections.Generic.ISet<Saxon.Eej.ee.stream.Posture> postures) {
            if (postures.Contains(Saxon.Eej.ee.stream.Posture.ROAMING)) {
                return Saxon.Eej.ee.stream.Posture.ROAMING;
            }
            if (postures.Count==0) {
                return Saxon.Eej.ee.stream.Posture.GROUNDED;
            }
            if ((postures.Contains(Saxon.Eej.ee.stream.Posture.GROUNDED) && (postures.Count == 1))) {
                return Saxon.Eej.ee.stream.Posture.GROUNDED;
            }
            if (postures.Contains(Saxon.Eej.ee.stream.Posture.CLIMBING)) {
                if ((postures.Count == 1)) {
                    return Saxon.Eej.ee.stream.Posture.CLIMBING;
                }
                if (((postures.Count == 2) && postures.Contains(Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                    return Saxon.Eej.ee.stream.Posture.CLIMBING;
                }
            }
            if (postures.Contains(Saxon.Eej.ee.stream.Posture.STRIDING)) {
                if ((postures.Count == 1)) {
                    return Saxon.Eej.ee.stream.Posture.STRIDING;
                }
                if (((postures.Count == 2) && postures.Contains(Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                    return Saxon.Eej.ee.stream.Posture.STRIDING;
                }
            }
            if ((postures.Contains(Saxon.Eej.ee.stream.Posture.CRAWLING) && !(postures.Contains(Saxon.Eej.ee.stream.Posture.CLIMBING)))) {
                return Saxon.Eej.ee.stream.Posture.CRAWLING;
            }
            return Saxon.Eej.ee.stream.Posture.ROAMING;
        }
    
    }
}