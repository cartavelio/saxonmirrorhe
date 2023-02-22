using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class VariableReferenceAdjunct : Saxon.Eej.ee.stream.adjunct.CopyOfAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return Saxon.Hej.pattern.StreamingFunctionArgumentPattern.getInstance();
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.VariableReference var = (Saxon.Hej.expr.VariableReference)(getExpression());
            if ((var is Saxon.Hej.expr.LocalVariableReference)) {
                Saxon.Hej.expr.LocalBinding binding = (Saxon.Hej.expr.LocalBinding)(var.getBinding());
                if (((binding is Saxon.Hej.expr.LetExpression) && (((Saxon.Hej.expr.LetExpression)(binding)).getSequence() is Saxon.Hej.expr.ContextItemExpression))) {
                    Saxon.Hej.expr.ContextItemExpression cie = (Saxon.Hej.expr.ContextItemExpression)(((Saxon.Hej.expr.LetExpression)(binding)).getSequence());
                    if (((/*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(cie) == null) || (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(cie) == Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                        return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                    }
                    Saxon.Hej.expr.Expression e = var;
                    while ((e != binding)) {
                        Saxon.Hej.expr.Expression parent = e.getParentExpression();
                        foreach (Saxon.Hej.expr.Operand o in parent.operands()) {
                            if (((o.getChildExpression() == e) && o.isEvaluatedRepeatedly())) {
                                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                            }
                        }
                        e = parent;
                    }
                    return /*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(cie);
                }
                if ((binding is Saxon.Hej.expr.flwor.LocalVariableBinding)) {
                    Saxon.Hej.expr.Expression e = var;
                    Saxon.Hej.expr.Expression parent = e.getParentExpression();
                    bool inLoop = false;
                    while ((parent != null)) {
                        if (!(inLoop)) {
                            foreach (Saxon.Hej.expr.Operand o in parent.operands()) {
                                if (((o.getChildExpression() == e) && o.isEvaluatedRepeatedly())) {
                                    inLoop = true;
                                }
                            }
                        }
                        if (((inLoop && (parent is Saxon.Hej.pattern.PatternThatSetsCurrent)) && (((Saxon.Hej.pattern.PatternThatSetsCurrent)(parent)).getCurrentBinding() == binding))) {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                        e = parent;
                        parent = e.getParentExpression();
                    }
                }
                if ((binding is Saxon.Hej.expr.instruct.UserFunctionParameter)) {
                    Saxon.Hej.trans.FunctionStreamability ability = ((Saxon.Hej.expr.instruct.UserFunctionParameter)(binding)).getFunctionStreamability();
                    switch (ability.innerEnumValue) {
                      case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ABSORBING:
                        if (Saxon.Hej.value.Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            Saxon.Hej.expr.Expression e = var;
                            Saxon.Hej.expr.Expression parent = e.getParentExpression();
                            while ((parent != null)) {
                                foreach (Saxon.Hej.expr.Operand o in parent.operands()) {
                                    if (((o.getChildExpression() == e) && o.isHigherOrder())) {
                                        reasons.Add((("A reference to the streaming parameter $"+var.getDisplayName())+" appears in a higher-order context (that is, it needs to be evaluated repeatedly)"));
                                        return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                                    }
                                }
                                e = parent;
                                parent = e.getParentExpression();
                            }
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.CONSUMING);
                        } else {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                      case Saxon.Hej.trans.FunctionStreamability.InnerEnum.ASCENT:
                        if (Saxon.Hej.value.Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                      case Saxon.Hej.trans.FunctionStreamability.InnerEnum.FILTER:
                        if (Saxon.Hej.value.Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                      case Saxon.Hej.trans.FunctionStreamability.InnerEnum.INSPECTION:
                        if (Saxon.Hej.value.Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                      case Saxon.Hej.trans.FunctionStreamability.InnerEnum.SHALLOW_DESCENT:
                        if (Saxon.Hej.value.Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                      case Saxon.Hej.trans.FunctionStreamability.InnerEnum.DEEP_DESCENT:
                        if (Saxon.Hej.value.Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
                        }
                      default:
                        return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
                    }
                }
            }
            return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
    
    }
}