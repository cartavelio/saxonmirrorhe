package com.saxonica.ee.stream.adjunct;


import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.flwor.LocalVariableBinding;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.pattern.PatternThatSetsCurrent;
import net.sf.saxon.pattern.StreamingFunctionArgumentPattern;
import net.sf.saxon.trans.FunctionStreamability;
import net.sf.saxon.value.Cardinality;

import java.util.List;

public class VariableReferenceAdjunct extends CopyOfAdjunct {

    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     *
     * @param config           the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */
    @Override
    public Pattern toStreamingPattern(Configuration config) {
        return StreamingFunctionArgumentPattern.getInstance();
    }

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        // handle the case where the variable is bound to ".", typically as part of the expansion of a call to current()
        VariableReference var = (VariableReference)getExpression();
        if (var instanceof LocalVariableReference) {
            LocalBinding binding = (LocalBinding)var.getBinding();
            if (binding instanceof LetExpression && ((LetExpression)binding).getSequence() instanceof ContextItemExpression) {
                // See the rules for streamability of the current() function
                ContextItemExpression cie = (ContextItemExpression) ((LetExpression) binding).getSequence();
                // See also test stream-210, where the local variable is declared outside a containing xsl:stream instruction,
                // so its posture is unknown and irrelevant.
                if (Streamability.getPostureAndSweepIfKnown(cie) == null || Streamability.getPosture(cie) == Posture.GROUNDED) {
                    return new PostureAndSweep(Posture.GROUNDED, Sweep.MOTIONLESS);
                }
                // "If the path in the expression tree connecting current() to E contains a higher-order operand..."
                Expression e = var;
                while (e != binding) {
                    Expression parent = e.getParentExpression();
                    for (Operand o : parent.operands()) {
                        if (o.getChildExpression() == e && o.isEvaluatedRepeatedly()) {
                            return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);
                        }
                    }
                    e = parent;
                }
                return Streamability.getPostureAndSweepIfKnown(cie);
            }
            // current() used within a pattern
            if (binding instanceof LocalVariableBinding) {
                Expression e = var;
                Expression parent = e.getParentExpression();
                boolean inLoop = false;
                while (parent != null) {
                    if (!inLoop) {
                        for (Operand o : parent.operands()) {
                            if (o.getChildExpression() == e && o.isEvaluatedRepeatedly()) {
                                inLoop = true;
                            }
                        }
                    }
                    if (inLoop && parent instanceof PatternThatSetsCurrent && ((PatternThatSetsCurrent) parent).getCurrentBinding() == binding) {
                        return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);
                    }
                    e = parent;
                    parent = e.getParentExpression();
                }
            }
            if (binding instanceof UserFunctionParameter) {
                FunctionStreamability ability = ((UserFunctionParameter)binding).getFunctionStreamability();
                switch (ability) {
                    case ABSORBING:
                        if (Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            Expression e = var;
                            Expression parent = e.getParentExpression();
                            while (parent != null) {
                                for (Operand o : parent.operands()) {
                                    if (o.getChildExpression() == e && o.isHigherOrder()) {
                                        reasons.add("A reference to the streaming parameter $" + var.getDisplayName() +
                                                            " appears in a higher-order context (that is, it needs to be evaluated repeatedly)");
                                        return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                                    }
                                }
                                e = parent;
                                parent = e.getParentExpression();
                            }
                            return new PostureAndSweep(Posture.STRIDING, Sweep.CONSUMING);
                        } else {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);
                        }
                    case ASCENT:
                        if (Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);
                        }
                    case FILTER:
                        if (Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);
                        }
                    case INSPECTION:
                        if (Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);
                        }
                    case SHALLOW_DESCENT:
                        if (Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);
                        }
                    case DEEP_DESCENT:
                        if (Cardinality.allowsMany(binding.getRequiredType().getCardinality())) {
                            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                        } else {
                            return new PostureAndSweep(Posture.STRIDING, Sweep.MOTIONLESS);
                        }
                    default:
                        return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
                }

            }
        }
        return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
    }




}

// Copyright (c) 2018-2023 Saxonica Limited