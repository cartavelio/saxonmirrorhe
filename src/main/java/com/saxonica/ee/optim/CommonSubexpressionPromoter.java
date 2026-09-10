package com.saxonica.ee.optim;

import com.saxonica.ee.stream.StreamInstr;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.LocalParam;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * This class is responsible for identifying and extracting common subexpressions within
 * an expression. For example the expression (x/y/z = 3 or x/y/z = 4) is turned into
 * (let $zz = x/y/z return ($zz = 3 or $zz = 4).
 * 
 * For simplicity, in this initial version:
 *
 * (a) we extract at most one subexpression: the one that has the highest
 * value of instances*cost.
 *
 * (b) we restrict the scope of the search for duplicates to that part of the expression tree
 * that shares the same dynamic context (local variables, focus, etc).
 */
public class CommonSubexpressionPromoter {

    private final OptimizerEE optimizer;

    public CommonSubexpressionPromoter(OptimizerEE opt) {
        this.optimizer = opt;
    }

    /**
     * Search for duplicate subexpressions within an expression tree, and if one is found,
     * generate a local variable bound to the common subexpression, replacing the original
     * subexpression with a reference to the new local variable
     * @param in the expression to be searched
     * @return the new expression, with common subexpressions promoted; or the original
     * expression unchanged.
     */

    public Expression promoteCommonSubexpressions(Expression in) {
        HashMap<Expression, List<Operand>> eligibles = new HashMap<Expression, List<Operand>>();
        gatherEligibles(in, eligibles);
        double topCost = 0;
        Expression topCandidate = null;
        for (Map.Entry<Expression, List<Operand>> entry : eligibles.entrySet()) {
            int count = entry.getValue().size();
            if (count > 1) {
                double cost = count * entry.getKey().getCost();
                if (cost > topCost) {
                    topCost = cost;
                    topCandidate = entry.getKey();
                }
            }
        }
        if (topCost > 10) {
            final List<Operand> equivalents = eligibles.get(topCandidate);
            optimizer.trace("Identified commmon subexpression", topCandidate);
            int cardinality = 0;
            UType itemType = UType.VOID;
            for (Operand o : equivalents) {
                Expression e = o.getChildExpression();
                cardinality = Cardinality.union(cardinality, e.getCardinality());
                itemType = itemType.union(e.getItemType().getUType());
            }
            SequenceType type = SequenceType.makeSequenceType(itemType.toItemType(), cardinality);
            LetExpression let = new LetExpression();
            let.setSequence(topCandidate);
            let.setVariableQName(new StructuredQName("vv", NamespaceUri.SAXON_GENERATED_VARIABLE, "cc" + topCandidate.hashCode()));
            let.setRequiredType(type);
            let.setNeedsLazyEvaluation(true);
            //let.setEvaluator(Cardinality.allowsMany(topCandidate.getCardinality()) ? Evaluator.MemoClosureEvaluator.INSTANCE : Evaluator.SingletonClosure.INSTANCE);
            for (Operand o : equivalents) {
                o.setChildExpression(new LocalVariableReference(let));
            }
            let.setAction(in);
            ExpressionTool.resetPropertiesWithinSubtree(in);
            let.rebuildReferenceList(true);
            return let;
        }
        return in;
    }


    private static boolean containsExact(List<Expression> list, Expression exp) {
        for (Expression e : list) {
            if (e == exp) {
                return true;
            }
        }
        return false;
    }

    private void gatherEligibles(Expression in, Map<Expression, List<Operand>> eligibles) {
        if (in.getCost() < 2) {
            // ignore literals and variable references
            return;
        }
        if (in instanceof StreamInstr) {
            // no action: could disturb streamability
        } else if (!in.isLiftable(false)) {
            // no action: can't be evaluated independently
        } else if (!in.allowExtractingCommonSubexpressions() ||
                in instanceof LocalBinding ||
                isBlockWithLocalParams(in)) {
            processChildren(in);
        } else {
            for (Operand o : in.operands()) {
                Expression child = o.getChildExpression();
                if (o.hasSameFocus() && !o.getOperandRole().isConstrainedClass()) {
                    if (eligibles.containsKey(child)) {
                        eligibles.get(child).add(o);
                    } else {
                        List<Operand> equivalents = new ArrayList<Operand>(2);
                        equivalents.add(o);
                        eligibles.put(child, equivalents);
                    }
                    gatherEligibles(child, eligibles);
                } else {
                    // eliminate common subexpressions within the scope of the child expression
                    o.setChildExpression(promoteCommonSubexpressions(child));
                }
            }
        }
    }

    private static boolean isBlockWithLocalParams(Expression in) {
        if (in instanceof Block) {
            Operand[] operands = ((Block)in).getOperanda();
            return operands.length > 0 && operands[0].getChildExpression() instanceof LocalParam;
        } else {
            return false;
        }
    }

    private void processChildren(Expression in) {
        for (Operand o: in.operands()) {
            Expression child = o.getChildExpression();
            o.setChildExpression(promoteCommonSubexpressions(child));
        }
    }


}

// Copyright (c) 2018-2022 Saxonica Limited