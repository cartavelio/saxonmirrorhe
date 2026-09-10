package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandUsage;
import net.sf.saxon.expr.UserFunctionCall;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.oper.OperandArray;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.TypeHierarchy;

import java.util.ArrayList;
import java.util.List;


/**
 * Handle streaming for calls to user-defined functions with streamability="ascent"
 */

public class AscentFunctionCallAdjunct extends StreamingAdjunct {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<>();
        }

        UserFunctionCall exp = (UserFunctionCall)getExpression();
        UserFunction target = exp.getFunction();


        OperandArray operanda = exp.getOperanda();
        operanda.getOperand(0).setUsage(OperandUsage.TRANSMISSION);
        for (int i = 1; i < operanda.getNumberOfOperands(); i++) {
            operanda.getOperand(i).setUsage(Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
        }

        // The rules here are those in the spec, modified by Saxon bug 3916 and spec bug 30289

        // Rule 1
        PostureAndSweep ps0 = Streamability.generalStreamabilityRules(exp, operanda, contextInfo, reasons);

        // Rule 2 is redundant

        // Rule 3
        if (ps0.getSweep() != Sweep.MOTIONLESS) {
            reasons.add("The sweep of the function is not motionless");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 4
        if (ps0.getPosture() == Posture.ROAMING) {
            reasons.add("The posture of the function is roaming");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        // Rule 5
        if (ps0.getPosture() == Posture.GROUNDED) {
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }

        // Rule 6 (amended)
        TypeHierarchy th = getConfiguration().getTypeHierarchy();
        if (th.relationship(target.getResultType().getPrimaryType(), AnyNodeTest.getInstance()) != Affinity.DISJOINT) {
            return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);
        } else {
            return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }


    }


}

// Copyright (c) 2018-2022 Saxonica Limited