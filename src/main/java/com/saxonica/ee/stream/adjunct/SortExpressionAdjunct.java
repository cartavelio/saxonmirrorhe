package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.sort.SortExpression;
import net.sf.saxon.expr.sort.SortKeyDefinition;
import net.sf.saxon.expr.sort.SortKeyDefinitionList;

import java.util.ArrayList;
import java.util.List;


/**
 * Streaming code for sort expression. Sort expressions are streamable only in the case
 * where the input sequence is grounded.
 */
public class SortExpressionAdjunct extends StreamingAdjunct {

    /**
     * Get the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     * @param contextInfo      Information about the context item for the expression
     * @param reasons          the caller may supply a list, in which case the implementation may add to this
     */
    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {

        SortExpression exp = (SortExpression)getExpression();
        Expression sortInput = exp.getBaseExpression();

        // Short-cut
        PostureAndSweep basePS = Streamability.getStreamability(sortInput, contextInfo, new ArrayList<String>());
        if (basePS.getPosture() != Posture.GROUNDED) {
            reasons.add("A sort expression is not streamable unless the input sequence is grounded");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        List<Operand> expandedOperands = new ArrayList<Operand>();
        expandedOperands.add(exp.getBaseOperand());
        SortKeyDefinitionList skdl = exp.getSortKeyDefinitionList();
        for (SortKeyDefinition skd : skdl) {
             for (Operand o : skd.operands()) {
                 expandedOperands.add(o);
             }
        }

        PostureAndSweep ps = Streamability.generalStreamabilityRules(
                getExpression(), expandedOperands, contextInfo, reasons);
        if (ps.getSweep() == Sweep.MOTIONLESS || ps.getSweep() == Sweep.CONSUMING) {
            Streamability.setPostureAndSweep(skdl, PostureAndSweep.GROUNDED_AND_MOTIONLESS);
            for (SortKeyDefinition skd : skdl) {
                Streamability.setPostureAndSweep(skd, PostureAndSweep.GROUNDED_AND_MOTIONLESS);
            }
        }
        return ps;
    }

}

// Copyright (c) 2014-2023 Saxonica Limited