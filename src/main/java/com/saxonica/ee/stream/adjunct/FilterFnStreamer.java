package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.functions.hof.FunctionSequenceCoercer;
import net.sf.saxon.functions.hof.UserFunctionReference;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.FunctionStreamability;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;

import java.util.List;


/**
 * Generate streaming code for the fn:filter function. Only streamable if the operand is grounded
 */
public class FilterFnStreamer extends FilteringAdjunct {

    /**
     * Compute the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This must be implemented in all subclasses that don't use the general streamability rules. The implementation
     * should have the side-effect of computing the streamability of all subexpressions, recursively.
     *
     * @param contextInfo Information about the context item type and posture
     * @param reasons     the caller may supply a list, in which case the implementation may add to this; the
     *                    contents are human-readable messages explaining why the expression is not streamable.
     * @return the posture and sweep of the expression
     */

    @Override
    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        Expression exp = getExpression();
        Iterable<Operand> ops = exp.operands();
        PostureAndSweep ps = Streamability.generalStreamabilityRules(exp, ops, contextInfo, reasons);
        if (ps.getSweep() == Sweep.CONSUMING && ps.getPosture() != Posture.GROUNDED) {
            // check that the supplied function has streamability = "inspection"
            Expression fn = ((SystemFunctionCall) exp).getArg(1);
            if (checkPredicateFunction(fn, reasons)) {
                return ps;
            } else {
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
        } else {
            return ps;
        }
    }

    public static boolean checkPredicateFunction(Expression fn, List<String> reasons) {

        if (fn instanceof FunctionSequenceCoercer) {
            fn = ((FunctionSequenceCoercer) fn).getBaseExpression();
        }
        UserFunction uf = null;
        if (fn instanceof UserFunctionReference) {
            uf = (UserFunction) ((UserFunctionReference) fn).getFixedTarget().getActor();
        }
        if (uf == null) {
            reasons.add("Call to " + fn.getParentExpression().toShortString() + " is not streamable, because the predicate function is not statically known");
            return false;
        } else if (uf.getFunctionName().hasURI(NamespaceUri.ANONYMOUS)) {
            // check that the function satisfies the rules for streamability = inspection
            UserFunctionParameter first = uf.getParameterDefinitions()[0];
            first.setFunctionStreamability(FunctionStreamability.INSPECTION);
            first.setRequiredType(new SequenceType(first.getRequiredType().getPrimaryType(), StaticProperty.EXACTLY_ONE));
            Expression body = uf.getBody();
            PostureAndSweep bodyPS = Streamability.getStreamability(body, ContextItemStaticInfoEE.ABSENT, reasons);
            if (bodyPS.getPosture() != Posture.GROUNDED || bodyPS.getSweep() != Sweep.MOTIONLESS) {
                reasons.add("The body of the predicate function must be grounded and motionless");
                return false;
            }
        } else if (uf.getDeclaredStreamability() != FunctionStreamability.INSPECTION) {
            reasons.add("Call to " + fn.getParentExpression().toShortString() + " is not streamable, because the predicate function is not declared with streamability='inspection'");
            return false;
        }

        return true;
    }


    /**
     * Make a filter for the event stream
     *
     * @param context the XPath dynamic context
     * @param out destination for the filtered output
     * @return a filter that can be added to the event stream
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic evaluation error occurs
     */
    @Override
    public FilteringFeed.Filter makeFilter(final XPathContext context, ItemFeed out) throws XPathException {
        final FunctionItem function = (FunctionItem) ((SystemFunctionCall)getExpression()).getArg(1).evaluateItem(context);
        final Sequence[] args = SequenceTool.makeSequenceArray(1);
        XPathContext c2 = function.makeNewContext(context, null);
        return new FilteringFeed.FilterLambda((item, position) -> {
            args[0] = item;
            return ((BooleanValue) function.call(c2, args).head()).getBooleanValue()
                    ? FilteringFeed.FilterResult.MATCHES
                    : FilteringFeed.FilterResult.SKIP;
        });
    }


}

// Copyright (c) 2014-2023 Saxonica Limited