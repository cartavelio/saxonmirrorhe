package com.saxonica.ee.stream;

import com.saxonica.ee.bytecode.CompiledExpression;
import com.saxonica.ee.stream.adjunct.StreamingAdjunct;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.SourceDocument;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.expr.sort.SortKeyDefinition;
import net.sf.saxon.functions.AccumulatorFn;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;

import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * Supporting class for assessing the streamability of expressions. Among other things, it contains
 * an implementation of the General Streamability Rules.
 */

public class Streamability {

    /**
     * Get the consuming (or potentially consuming) operand of an expression, assuming there is exactly one
     * @param exp the expression
     * @return if there are one or more consuming subexpressions, then the first of these;
     * otherwise, null. Also returns null if the posture and sweep have not yet been assessed.
     */

    public static Operand getConsumingOperand(Expression exp) {
        for (Operand o : exp.operands()) {
            Expression e = o.getChildExpression();
            PostureAndSweep ps = getPostureAndSweepIfKnown(e);
            if (ps == null) {
                return null;
            }
            if (ps.getSweep() == Sweep.CONSUMING) {
                return o;
            }
            if (o.getUsage() == OperandUsage.TRANSMISSION && ps.getPosture() != Posture.GROUNDED) {
                return o;
            }
            if (o.getUsage() == OperandUsage.ABSORPTION && isIncrementalPosture(ps.getPosture())) {
                return o;
            }
        }
        return null;
    }

    public static Expression rewriteQuantifiedExpressionAsFilterExpression(QuantifiedExpression quant) throws XPathException {
        Expression select = quant.getSequence();
        Expression test = quant.getAction().copy(new RebindingMap());
        RetainedStaticContext rsc = quant.getRetainedStaticContext();
        test = replaceVariableReferenceByDot(test, quant);
        if (test != null) {
            if (quant.getOperator() == Token.SOME) {
                Expression fn = SystemFunction.makeCall("boolean", rsc, test);
                FilterExpression fe = new FilterExpression(select, fn);
                return SystemFunction.makeCall("exists", rsc, fe);
            } else {
                Expression fn = SystemFunction.makeCall("not", rsc, test);
                FilterExpression fe = new FilterExpression(select, fn);
                return SystemFunction.makeCall("empty", rsc, fe);
            }
        }
        return null;
    }

    public static Expression rewriteForExpressionAsMappingExpression(ForExpression forEx) {
        Expression select = forEx.getSequence();
        Expression step = forEx.getAction().copy(new RebindingMap());
        step = replaceVariableReferenceByDot(step, forEx);
        if (step != null) {
            return new ForEach(select, step);
        }
        return null;
    }

    private static Expression replaceVariableReferenceByDot(Expression test, Binding binding) {
        for (Operand info : test.operands()) {
            if (!info.hasSameFocus() && ExpressionTool.dependsOnVariable(info.getChildExpression(), new Binding[]{binding})) {
                return null;  //failed
            }
            if (info.getChildExpression() instanceof LocalVariableReference &&
                    ((LocalVariableReference) info.getChildExpression()).getBinding() == binding) {
                info.setChildExpression(new ContextItemExpression());
            } else {
                Expression e2 = replaceVariableReferenceByDot(info.getChildExpression(), binding);
                if (e2 == null) {
                    return null; // replacement was not possible in some subexpression
                }
            }
        }
        return test;
    }

    /**
     * Implement the general streamability rules; the rules used to determine the posture and sweep of any
     * expression except when more specific rules are defined for that class of expression
     * @param target the expression whose streamability is being analyzed (used only for diagnostics)
     * @param operands the operands of the expression being evaluated
     * @param contextInfo information about the static context item type and posture
     * @param reasons (optionally null) an array to which explanations of non-streamability should
     * be appended
     * @return the posture and sweep of the expression
     */

    public static PostureAndSweep generalStreamabilityRules(
            Expression target,
            Iterable<Operand> operands, ContextItemStaticInfoEE contextInfo, List<String> reasons) {

        if (target.getRetainedStaticContext().isBackwardsCompatibility()) {
            reasons.add("Instructions with effective version='1.0' are not streamable");
            return PostureAndSweep.ROAMING_AND_FREE_RANGING;
        }

        Operand potentiallyConsumingOperand = null;
        PostureAndSweep postureAndSweepOfPotentiallyConsumingOperand = null;
        Sweep widestInChoiceGroup = Sweep.MOTIONLESS;
        Set<Posture> choiceGroupPostures = new HashSet<>();

        ContextItemStaticInfoEE newFocus = null;
        for (Operand o : operands) {
            if (o.setsNewFocus()) {
                getStreamability(o.getChildExpression(), contextInfo, reasons);
                newFocus = (ContextItemStaticInfoEE)target.getConfiguration().makeContextItemStaticInfo(getItemType(o.getChildExpression(), contextInfo), false);
                newFocus.setContextSettingExpression(o.getChildExpression());
            }
        }

        boolean foundOperand = false;
        for (Operand o : operands) {
            Expression child = o.getChildExpression();
            foundOperand = true;
            OperandUsage usage = o.getUsage();
            //assert o.hasSameFocus() || newFocus != null;
            ContextItemStaticInfoEE info2;
            if (o.hasSameFocus()) {
                info2 = contextInfo;
            } else if (o.hasSpecialFocusRules()) {
                info2 = getCustomStaticInfo(o);
            } else {
                assert newFocus != null;
                info2 = newFocus;
            }
            PostureAndSweep ps = getStreamability(child, info2, reasons);
            Posture p = ps.getPosture();
            Sweep s = ps.getSweep();
            if (s == Sweep.FREE_RANGING || p == Posture.ROAMING) {
                s = Sweep.FREE_RANGING;
            } else if (p == Posture.GROUNDED) {
                // s' = s
            } else {
                ItemType t = getItemType(child, info2);
                if (usage == OperandUsage.ABSORPTION && !t.getUType().overlaps(UType.DOCUMENT.union(UType.ELEMENT))) {
                    usage = OperandUsage.INSPECTION;
                }
                if (usage == OperandUsage.NAVIGATION) {
                    if (reasons != null) {
                        reasons.add(describeOperand(target, o) +
                            " selects streamed nodes in a context that allows arbitrary navigation (line " + o.getChildExpression().getLocation().getLineNumber() + ")");
                    }
                    s = Sweep.FREE_RANGING;
                } else if (usage == OperandUsage.ABSORPTION) {
                    if (p == Posture.CLIMBING) {
                        if (reasons != null) {
                            reasons.add(describeOperand(target, o) +
                                    " reads the string value or typed value of a node in climbing posture (line " +
                                o.getChildExpression().getLocation().getLineNumber() + ")");
                        }
                        s = Sweep.FREE_RANGING;
                    } else if (p == Posture.CRAWLING) {
                        // Spec change: bug 25185
                        s = Sweep.CONSUMING;
                    } else if (p == Posture.STRIDING) {
                        s = Sweep.CONSUMING;
                    }
                }
            }
            if (s == Sweep.FREE_RANGING) {
                // Rule 2(b)
                return new PostureAndSweep(Posture.ROAMING, Sweep.FREE_RANGING);
            }
            if (s == Sweep.CONSUMING || (usage == OperandUsage.TRANSMISSION && p != Posture.GROUNDED)) {
                if (potentiallyConsumingOperand != null) {
                    // Rule 2(c)(ii)
                    if (reasons != null) {
                        Expression e0 = potentiallyConsumingOperand.getChildExpression();
                        Expression e1 = o.getChildExpression();
                        int line0 = e0.getLocation().getLineNumber();
                        int line1 = e1.getLocation().getLineNumber();
                        String suffix;
                        if (line0 == line1) {
                            suffix = "{" + e0.toShortString() + "} and {" +
                                    e1.toShortString() + "}, both on line " + line0;
                        } else {
                            suffix = "{" + e0.toShortString() + "} on line " + line0 + ", and {" +
                                    e1.toShortString() + "} on line " + line1;
                        }
                        if (s == Sweep.CONSUMING && getSweep(potentiallyConsumingOperand.getChildExpression()) == Sweep.CONSUMING) {
                            reasons.add("There is more than one consuming operand: " + suffix);
                        } else {
                            reasons.add("There is more than one potentially consuming operand: " + suffix);
                        }
                    }
                    return new PostureAndSweep(Posture.ROAMING, Sweep.FREE_RANGING);
                }
                if (o.isInChoiceGroup()) {
                    widestInChoiceGroup = wider(widestInChoiceGroup, s);
                    choiceGroupPostures.add(p);
                } else if (!choiceGroupPostures.isEmpty()) {
                    if (reasons != null) {
                        reasons.add("There is a consuming operand in a test condition (line " +
                            o.getChildExpression().getLocation().getLineNumber() + "), and another in a previous conditional branch");
                    }
                    return new PostureAndSweep(Posture.ROAMING, Sweep.FREE_RANGING);

                } else {
                    potentiallyConsumingOperand = o;
                    postureAndSweepOfPotentiallyConsumingOperand = ps;
                }
            }
        }

        // Rule 2(a)
        if (!foundOperand) {
            return new PostureAndSweep(Posture.GROUNDED, Sweep.MOTIONLESS);
        }
        // Rule 2(b) already done
        // Rule 2(c)
        if (!choiceGroupPostures.isEmpty()) {
            return new PostureAndSweep(combinedPosture(choiceGroupPostures), widestInChoiceGroup);
        }
        // Rule 2(d)
        if (potentiallyConsumingOperand != null) {
            if (potentiallyConsumingOperand.isEvaluatedRepeatedly()) {
                // Rule 2(d)(i)
                if (reasons != null) {
                    reasons.add("A consuming operand is evaluated repeatedly");
                }
                return PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            OperandUsage u = potentiallyConsumingOperand.getUsage();
            if (u == OperandUsage.ABSORPTION || u == OperandUsage.INSPECTION) {
                // Rule 2(d)(ii)
                return new PostureAndSweep(Posture.GROUNDED, Sweep.CONSUMING);
            }
            // Rule 2(s)(iii)
            return postureAndSweepOfPotentiallyConsumingOperand;
        }
        // Rule 2(e)
        return new PostureAndSweep(Posture.GROUNDED, Sweep.MOTIONLESS);

    }

    private static ItemType getItemType(Expression exp, ContextItemStaticInfo info) {
        if (info.isStrictStreamabilityRules()) {
            return exp.getStaticUType(info.getItemType().getUType()).toItemType();
        } else {
            return exp.getItemType();
        }
    }

    private static ContextItemStaticInfoEE getCustomStaticInfo(Operand o) {
        Expression exp = o.getParentExpression();
        if (exp instanceof SortKeyDefinition) {
            ContextItemStaticInfoEE ciso = new ContextItemStaticInfoEE(exp.getItemType(), false );
            ciso.setContextPostureGrounded();
            return ciso;
        } else if (exp instanceof SourceDocument) {
            ContextItemStaticInfoEE ciso = new ContextItemStaticInfoEE(NodeKindTest.DOCUMENT, false);
            ciso.setContextPostureGrounded();
            return ciso;
        }
        assert false;
        return null;
    }

    private static String describeOperand(Expression target, Operand o) {
        if (o.getChildExpression() instanceof ContextItemExpression && target instanceof LetExpression &&
                ((LetExpression)target).getVariableQName().getLocalPart().startsWith("current")) {
            return "The current() function";
        } else {
            return "Operand {" + o.getChildExpression().toShortString() + "} of {" + target.toShortString() + "}";
        }
    }

    /**
     * Ask whether an ItemType only allows nodes that cannot have children
     * @param type the item type
     * @return true if the item type only allows nodes that cannot have children
     */

    public static boolean isChildlessNodeKind(ItemType type) {
        return (type instanceof NodeTest) &&
                !type.getUType().overlaps(UType.PARENT_NODE_KINDS);
    }

    /**
     * Convert an expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     * @param expr the expression to be converted
     * @param config the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */

    public static Pattern toStreamingPattern(Expression expr, Configuration config) {
        if (expr instanceof Instruction) {
            return null;
        }
        StreamingAdjunct adj = StreamingAdjunct.makeStreamingAdjunct(config, expr);
        Pattern pat = adj.toStreamingPattern(config);
        if (pat != null) {
            pat.setRecoverable(false);
        }
        return pat;
    }


    /**
     * Get the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This provides an assessment of stylesheet code against the W3C criteria for guaranteed
     * streamability, and is implemented to allow these criteria to be tested. It is not the
     * case that all expression that emerge as streamable from this analysis are currently
     * capable of being streamed by Saxon
     *
     * @param contextInfo Information about the context item type and posture
     * @param reasons     the caller may supply a list, in which case the implementation may add to this
     *                    list a message explaining why the construct is not streamable, suitable for inclusion in an
     * @return the posture and sweep of the expression
     */

    public static PostureAndSweep getStreamability(Expression exp, ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        PostureAndSweep postureAndSweep = (PostureAndSweep) exp.getExtraProperty("P+S");
        if (postureAndSweep == null) {
            if (exp instanceof Pattern) {
                if (contextInfo.getContextItemPosture() == Posture.GROUNDED || ((Pattern) exp).isMotionless()) {
                    postureAndSweep = PostureAndSweep.GROUNDED_AND_MOTIONLESS;
                } else {
                    if (reasons != null) {
                        reasons.add("The pattern " + exp.toShortString() + " is not motionless");
                    }
                    postureAndSweep = PostureAndSweep.ROAMING_AND_FREE_RANGING;
                }
            } else if (exp.isCallOn(AccumulatorFn.AccumulatorAfter.class)) {
                // treated specially because the streamability depends on what has come earlier in the sequence constructor
                Sweep sweep = computeAccumulatorAfterSweep((SystemFunctionCall) exp, contextInfo, reasons);
                postureAndSweep = new PostureAndSweep(Posture.GROUNDED, sweep);
            } else {
                StreamingAdjunct adjunct = StreamingAdjunct.makeStreamingAdjunct(exp.getConfiguration(), exp);
                postureAndSweep = adjunct.computeStreamability(contextInfo, reasons);
            }
            exp.setExtraProperty("P+S", postureAndSweep);
        }
        return postureAndSweep;
    }

    public static Posture getPosture(Expression exp) {
        PostureAndSweep postureAndSweep = getPostureAndSweepIfKnown(exp);
        if (postureAndSweep != null) {
            return postureAndSweep.getPosture();
        } else if (isCompiledExpression(exp)) {
            return getPosture(exp.getInterpretedExpression());
        } else {
            throw new IllegalStateException("getPosture() called when streamability has not yet been computed");
        }
    }

    private static boolean isCompiledExpression(Expression exp) {
        boolean result = false;
        //#if BYTECODE==true
        result = exp instanceof CompiledExpression;
        //#endif
        return result;
    }

    public static Sweep getSweep(Expression exp) {
        PostureAndSweep postureAndSweep = getPostureAndSweepIfKnown(exp);
        if (postureAndSweep != null) {
            return postureAndSweep.getSweep();
        } else if (isCompiledExpression(exp)) {
            return getSweep(exp.getInterpretedExpression());
        } else {
            throw new IllegalStateException(exp.getClass().getName() + ".getSweep() called when streamability has not yet been computed");
        }
    }

    /**
     * Get the posture and sweep of the expression if these have already been computed
     *
     * @return the posture and sweep if known, or null if the streamability has not yet been assessed.
     */

    public static PostureAndSweep getPostureAndSweepIfKnown(Expression exp) {
        return (PostureAndSweep) exp.getExtraProperty("P+S");
    }

    public static void setPostureAndSweep(Expression exp, PostureAndSweep ps) {
        exp.setExtraProperty("P+S", ps);
    }

    /**
     * Compute the sweep of a call to accumulator-after (which depends on what else is found in the
     * same sequence constructor
     * @param call the call on accumulator-after
     * @return the computed sweep, which is consuming if all previous instructions are motionless,
     * or motionless if a previous instruction is consuming.
     */

    public static Sweep computeAccumulatorAfterSweep(SystemFunctionCall call, ContextItemStaticInfoEE contextInfo, List<String> reasons) {

        boolean strict = contextInfo.isStrictStreamabilityRules();

        Sweep firstArgSweep = getStreamability(call.getArg(0), contextInfo, reasons).getSweep();
        // Section 19.8.9.1
        // Rule 1
        if (firstArgSweep != Sweep.MOTIONLESS) {
            reasons.add("First argument of accumulator-after() is " + firstArgSweep);
            return Sweep.FREE_RANGING;
        }
        // Rule 2
        if (contextInfo.getContextItemPosture() == Posture.GROUNDED) {
            return Sweep.MOTIONLESS;
        }
        // Rule 3
        if (contextInfo.getItemType().getUType().intersection(UType.PARENT_NODE_KINDS).equals(UType.VOID)) {
            return Sweep.MOTIONLESS;
        }
        // Rule 4 and 5
        AccumulatorFn.Phase phase = contextInfo.getAccumulatorPhase();
        if (phase == AccumulatorFn.Phase.BEFORE) {
            return Sweep.FREE_RANGING;
        }
        if (phase == AccumulatorFn.Phase.AFTER) {
            return Sweep.MOTIONLESS;
        }
        // Rule 6 - TBA
        // Rule 7
        Expression focusSettingContainer = ExpressionTool.getFocusSettingContainer(call);
        Expression parent;
        if (focusSettingContainer != null) {
            parent = call.getParentExpression();
            while (parent != null && !(parent instanceof Instruction)) {
                parent = parent.getParentExpression();
            }
            if (parent != null && focusSettingContainer != ExpressionTool.getFocusSettingContainer(parent)) {
                reasons.add("Call on accumulator-after() has different focus from containing instruction");
                return Sweep.FREE_RANGING;
            }
        }
        // Rule 8
        parent = call;
        boolean foundPrecedingConsumingExpression = false;
        while (parent != null) {
            Expression grandma = parent.getParentExpression();
            if (parent.isInstruction() || !strict) {
                if (grandma != null) {
                    for (Operand sib : grandma.operands()) {
                        if (sib.getChildExpression() == parent) {
                            break;
                        }
                        PostureAndSweep pas = getStreamability(sib.getChildExpression(), contextInfo, reasons);
                        if (pas.getSweep() == Sweep.CONSUMING) {
                            foundPrecedingConsumingExpression = true;
                            break;
                        }
                    }
                }
                if (foundPrecedingConsumingExpression) {
                    break;
                }
            }
            parent = grandma;
        }
        if (!foundPrecedingConsumingExpression) {
            reasons.add("A call to accumulator-after() is consuming when there are no preceding consuming instructions");
            return Sweep.CONSUMING;
        }
        // Rule 9
        return Sweep.MOTIONLESS;

    }


    /**
     * Return the wider of two sweeps. The order of increasing width is MOTIONLESS, CONSUMING, FREE_RANGING.
     * @param a the first sweep
     * @param b the second sweep
     * @return the wider of the two
     */
    public static Sweep wider(Sweep a, Sweep b) {
        switch (a) {
            case FREE_RANGING:
                return a;
            case CONSUMING:
                return b == Sweep.FREE_RANGING ? b : a;
            case MOTIONLESS:
                return b;
            default:
                throw new IllegalArgumentException();
        }
    }

    /**
     * Test whether a supplied posture is either striding or crawling
     * @param posture the posture to be tested
     * @return true if this posture is either striding or crawling
     */

    public static boolean isIncrementalPosture(Posture posture) {
        return posture == Posture.STRIDING || posture == Posture.CRAWLING;
    }

    /**
     * Get the combined posture of a set of postures, as defined in the XSLT 3.0 specification
     * @param postures the set of postures
     * @return the combined posture
     */

    public static Posture combinedPosture(Set<Posture> postures) {
        if (postures.contains(Posture.ROAMING)) {
            return Posture.ROAMING;
        }

        if (postures.isEmpty()) {
            return Posture.GROUNDED;
        }

        if (postures.contains(Posture.GROUNDED) && postures.size() == 1) {
            return Posture.GROUNDED;
        }

        if (postures.contains(Posture.CLIMBING)) {
            if (postures.size() == 1) {
                return Posture.CLIMBING;
            }
            if (postures.size() == 2 && postures.contains(Posture.GROUNDED)) {
                return Posture.CLIMBING;
            }
        }

        if (postures.contains(Posture.STRIDING)) {
            if (postures.size() == 1) {
                return Posture.STRIDING;
            }
            if (postures.size() == 2 && postures.contains(Posture.GROUNDED)) {
                return Posture.STRIDING;
            }
        }

        if (postures.contains(Posture.CRAWLING) && !postures.contains(Posture.CLIMBING)) {
            return Posture.CRAWLING;
        }

        return Posture.ROAMING;
    }
}

// Copyright (c) 2012-2022 Saxonica Limited
