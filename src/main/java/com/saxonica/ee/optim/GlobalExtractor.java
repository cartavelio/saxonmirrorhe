package com.saxonica.ee.optim;

import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.StreamInstr;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.xsltextn.instruct.DoInstr;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RetainedStaticContext;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.GlobalVariableManager;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;

import java.util.IdentityHashMap;
//import java.util.stream.Stream;

/**
 * This class is responsible for extracting global variables from subexpressions within
 * a function or template
 */
public class GlobalExtractor {

    private final Expression root;
    private final GlobalVariableManager globalVariableManager;
    private final ExpressionVisitor visitor;
    private final IdentityHashMap<Expression, String> ineligibles = new IdentityHashMap<Expression, String>();
    private final boolean tracing;

    public final static int COST_THRESHOLD = 3;

    public GlobalExtractor(Expression root, GlobalVariableManager globalVariableManager, ExpressionVisitor visitor) {
        this.root = root;
        this.globalVariableManager = globalVariableManager;
        this.visitor = visitor;
        this.tracing = visitor.getConfiguration().getBooleanProperty(Feature.TRACE_OPTIMIZER_DECISIONS);
    }

    public Expression extractGlobals() throws XPathException {
        markIneligibleExpressions(root, visitor.isOptimizeForStreaming());
        return processTree(root);
    }

    /**
     * Walk the tree rooted at an expression, assessing which subexpressions are eligible for
     * promotion to global variables. At the same time, evaluate and return a simple estimate of
     * the cost of evaluating the expression. The method has the side-effect of updating the
     * contents of the variable "ineligibles".
     * @param expr the expression to be assessed
     * @param forStreaming
     * @return an estimate of the cost.
     */

    private double markIneligibleExpressions(Expression expr, boolean forStreaming) {
        if (expr instanceof LetExpression && ((LetExpression) expr).isNeedsLazyEvaluation()) {
            markSubtreeIneligible(expr);
            return 100;
        } else if (expr instanceof DoInstr) {
            markSubtreeIneligible(expr);
        } else if (!expr.isLiftable(forStreaming) || expr instanceof StreamInstr) {
            markAncestorsIneligible(expr);
        } else if ((expr.getDependencies() & ~StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT) != 0) {
            ineligibles.put(expr, "this");
        } else if (expr instanceof TryCatch) {
            markSubtreeIneligible(expr);
            return 100;
        }
//        if (expr.getParentExpression() instanceof Choose) {
//            Operand o = ExpressionTool.findOperand(expr.getParentExpression(), expr);
//            assert (o != null);
//            if (o.isInChoiceGroup()) {
//                markSubtreeIneligible(expr);
//                return 100;
//            }
//        }
        double cost = expr.getCost();
        for (Operand o : expr.operands()) {
            double subcost = markIneligibleExpressions(o.getChildExpression(), forStreaming);
            if (subcost > cost) {
                cost = subcost;
            }
        }
        if (cost < COST_THRESHOLD) {
            markSubtreeIneligible(expr);
        }
        return cost;
    }

    private void markAncestorsIneligible(Expression expr) {
        Expression parent = expr;
        while (parent != null) {
            if (ineligibles.get(parent) == null) {
                ineligibles.put(parent, "this");
            }
            parent = parent.getParentExpression();
        }
    }

    private void markSubtreeIneligible(Expression expr) {
        ineligibles.put(expr, "all");
    }

    private Expression processTree(Expression expr) throws XPathException {
        String label = ineligibles.get(expr);
        if (label == null) {
            return promoteToGlobal(expr);
        } else if (label.equals("all")) {
            // do nothing; exit
            return expr;
        } else if (label.equals("this")) {
            for (Operand o : expr.operands()) {
                o.setChildExpression(processTree(o.getChildExpression()));
            }
            return expr;
        } else {
            throw new IllegalStateException();
        }
    }

    private Expression promoteToGlobal(Expression expr) throws XPathException {

        GlobalVariable v = globalVariableManager.getEquivalentVariable(expr);
        if (v != null) {
            GlobalVariableReference varRef = new GlobalVariableReference(v);
            SequenceType type = SequenceType.makeSequenceType(expr.getItemType(), expr.getCardinality());
            varRef.setStaticType(type, null, expr.getSpecialProperties() | StaticProperty.NO_NODES_NEWLY_CREATED);
            varRef.setRetainedStaticContext(visitor.getStaticContext().makeRetainedStaticContext());
            return varRef;
        }
        RetainedStaticContext rsc = expr.getRetainedStaticContext();
        GlobalVariable gvd = new GlobalVariable();
        gvd.setPackageData(rsc.getPackageData());
        gvd.setSystemId(expr.getLocation().getSystemId());
        gvd.setLineNumber(expr.getLocation().getLineNumber());
        gvd.setColumnNumber(expr.getLocation().getColumnNumber());
        gvd.setRequiredType(SequenceType.makeSequenceType(
                expr.getItemType(), expr.getCardinality()));
        gvd.setBody(expr);
        expr.setRetainedStaticContext(rsc);
        StructuredQName varName = new StructuredQName(
                "vv", NamespaceConstant.SAXON_GENERATED_VARIABLE, "gg" + gvd.hashCode());
        gvd.setVariableQName(varName);
        globalVariableManager.addGlobalVariable(gvd);

        GlobalVariableReference ref = new GlobalVariableReference(gvd);
        Streamability.setPostureAndSweep(ref, PostureAndSweep.GROUNDED_AND_MOTIONLESS);
        ref.setRetainedStaticContext(visitor.getStaticContext().makeRetainedStaticContext());
        gvd.registerReference(ref);

        gvd.typeCheck(visitor);

        SlotManager localSlotManager = visitor.getConfiguration().makeSlotManager();
        int slots = ExpressionTool.allocateSlots(expr, 0, localSlotManager);
        if (slots > 0) {
            gvd.setContainsLocals(localSlotManager);
        }

        gvd.init(expr, varName);
        if (tracing) {
            Logger err = visitor.getConfiguration().getLogger();
            err.info("OPT : At line " + expr.getLocation().getLineNumber() + " of " + expr.getLocation().getSystemId());
            err.info("OPT : Extracted global variable " + varName.getDisplayName() + " := " + expr.toString());
        }
        return ref;


    }
}

// Copyright (c) 2018-2020 Saxonica Limited