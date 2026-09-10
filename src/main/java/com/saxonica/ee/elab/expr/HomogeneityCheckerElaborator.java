package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.HomogeneityChecker;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.tree.iter.HomogeneityCheckerIterator;

/**
 * Elaborator for a homogeneity checker, which checks that the items in a sequence are
 * either all nodes, or all non-nodes.
 */

public class HomogeneityCheckerElaborator extends PullElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        HomogeneityChecker exp = (HomogeneityChecker) getExpression();
        Location location = exp.getLocation();
        Expression arg = exp.getBaseExpression();
        PullEvaluator argEval = Elaborator.makeElaborator(arg).elaborateForPull();

        return context -> new HomogeneityCheckerIterator(argEval.eval(context), location);
    }

}

