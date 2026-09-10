package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.om.SequenceIterator;

import java.util.ArrayList;
import java.util.List;

public class VennElaborator extends PullElaborator {

    public PullEvaluator getPullEvaluator() {
        final VennExpression exp = (VennExpression)getExpression();
        if (exp.getOperator() == Token.UNION) {
            final List<PullEvaluator> leafEvaluators = new ArrayList<>();
            gatherUnionLeafEvaluators(exp, leafEvaluators);
            return context -> {
                List<SequenceIterator> iterators = new ArrayList<>(leafEvaluators.size());
                for (PullEvaluator evaluator : leafEvaluators) {
                    iterators.add(evaluator.eval(context));
                }
                return new UnionIterator(iterators, GlobalOrderComparer.getInstance());
            };
        } else {
            PullEvaluator p1 = Elaborator.make(exp.getLhsExpression()).getPullEvaluator();
            PullEvaluator p2 = Elaborator.make(exp.getRhsExpression()).getPullEvaluator();
            if (exp.getOperator() == Token.INTERSECT) {
                return context -> new IntersectionEnumeration(
                        p1.eval(context), p2.eval(context), GlobalOrderComparer.getInstance());
            } else {
                return context -> new DifferenceEnumeration(
                        p1.eval(context), p2.eval(context), GlobalOrderComparer.getInstance());
            }
        }
    }

    private static void gatherUnionLeafEvaluators(VennExpression exp, List<PullEvaluator> leafEvaluators) {
        Expression e1 = exp.getLhsExpression();
        if (e1 instanceof VennExpression && ((VennExpression) e1).getOperator() == Token.UNION) {
            gatherUnionLeafEvaluators(((VennExpression) e1), leafEvaluators);
        } else {
            leafEvaluators.add(Elaborator.make(e1).getPullEvaluator());
        }
        Expression e2 = exp.getRhsExpression();
        if (e2 instanceof VennExpression && ((VennExpression) e2).getOperator() == Token.UNION) {
            gatherUnionLeafEvaluators(((VennExpression) e2), leafEvaluators);
        } else {
            leafEvaluators.add(Elaborator.make(e2).getPullEvaluator());
        }
    }

}

