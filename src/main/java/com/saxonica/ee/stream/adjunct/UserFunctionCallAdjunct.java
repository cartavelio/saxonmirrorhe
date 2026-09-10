package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandUsage;
import net.sf.saxon.expr.UserFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.trans.FunctionStreamability;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;


/**
 * Handle streaming for calls to user-defined functions
 * Used only for XSLT streaming capability.
 */

public class UserFunctionCallAdjunct extends StreamingAdjunct implements FeedMaker {

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        if (reasons == null) {
            reasons = new ArrayList<>();
        }

        UserFunctionCall exp = (UserFunctionCall)getExpression();
        UserFunction target = exp.getFunction();

        FunctionStreamability category = target.getDeclaredStreamability();

        Operand[] operanda = exp.getOperanda().copy();  // TODO: perhaps the operanda are already OK?
        for (int i = 1; i < operanda.length; i++) {
            operanda[i].setUsage(Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
        }

        switch (category) {
            case UNCLASSIFIED:
                throw new AssertionError(); // not handled by this class

            case ABSORBING: {
                throw new AssertionError(); // not handled by this class
            }
            case ASCENT: { // TODO: not used? Handled by AscentFunctionCallAdjunct
                operanda[0].setUsage(OperandUsage.INSPECTION);
                PostureAndSweep ps0 = Streamability.generalStreamabilityRules(
                        exp, Arrays.asList(operanda), contextInfo, reasons);
                if (ps0.getPosture() == Posture.ROAMING || ps0.getSweep() != Sweep.MOTIONLESS) {
                    return PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
                if (ps0.getPosture() == Posture.GROUNDED) {
                    return PostureAndSweep.GROUNDED_AND_MOTIONLESS;
                }
                return new PostureAndSweep(Posture.CLIMBING, Sweep.MOTIONLESS);
            }
            case FILTER: {
                operanda[0].setUsage(OperandUsage.TRANSMISSION);
                return Streamability.generalStreamabilityRules(
                        exp, Arrays.asList(operanda), contextInfo, reasons);

            }
            case INSPECTION: {
                operanda[0].setUsage(OperandUsage.INSPECTION);
                return Streamability.generalStreamabilityRules(exp, Arrays.asList(operanda), contextInfo, reasons);
            }

            case SHALLOW_DESCENT:
                throw new AssertionError(); // not handled by this class

//            case DEEP_DESCENT:
//                return PostureAndSweep.ROAMING_AND_FREE_RANGING; // TODO
//

            default:
                reasons.add("Saxon 9.8 does not implement streamability = " + category);
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;

        }



    }


    @Override
    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        return null;
    }

    @Override
    public ItemFeed makeItemFeed(WatchManager watchManager, ItemFeed out, XPathContext context) throws XPathException {
        UserFunctionCall call = (UserFunctionCall)getExpression();
        UserFunction function = call.getTargetFunction(context);
        FunctionStreamability ability = function.getDeclaredStreamability();
        if (ability == FunctionStreamability.UNCLASSIFIED) {
            throw new AssertionError(); // We should have used a general-purpose StreamingAdjunct
        } else {
            throw new XPathException("Streamed calls on functions with streamability = " + ability + " are currently not supported", "SXST0062");
        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited