package com.saxonica.ee.trans;

import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.Streamability;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.Actor;
import net.sf.saxon.expr.instruct.AttributeSet;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.functions.AccumulatorFn;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ErrorType;
import net.sf.saxon.type.ItemType;

/**
 * Saxon-EE version of ContextItemStaticInfo, with extra information used for streamability analysis
 */
public class ContextItemStaticInfoEE extends ContextItemStaticInfo {

    private Posture explicitPosture = Posture.UNKNOWN;
    private boolean strictStreamabilityRules;
    private Actor container;
    private AccumulatorFn.Phase accumulatorPhase = AccumulatorFn.Phase.UNSPECIFIED;


    /**
     * Create a ContextItemStaticInfo
     *
     * @param itemType       the item type of the context item. If the context item is absent, set this to
     *                       {@link net.sf.saxon.type.ErrorType#getInstance()}.
     * @param maybeUndefined set to true if it is possible (or certain) that the context item will be absent.
     */

    public ContextItemStaticInfoEE(ItemType itemType, boolean maybeUndefined) {
        super(itemType, maybeUndefined);
    }

    /**
     * Create a ContextItemStaticInfo
     *
     * @param itemType           the item type of the context item. If the context item is absent, set this to
     *                           {@link net.sf.saxon.type.ErrorType#getInstance()}.
     * @param maybeUndefined     set to true if it is possible (or certain) that the context item will be absent.
     * @param contextItemPosture the context item posture.
     */

    public ContextItemStaticInfoEE(ItemType itemType, boolean maybeUndefined, Posture contextItemPosture) {
        super(itemType, maybeUndefined);
        this.explicitPosture = contextItemPosture;
    }

    @Override
    public void setContextPostureStriding() {
        this.explicitPosture = Posture.STRIDING;
    }

    @Override
    public void setContextPostureGrounded() {
        this.explicitPosture = Posture.GROUNDED;
    }


    public Actor getContainer() {
        return container;
    }

    public void setContainer(Actor container) {
        this.container = container;
    }

    /**
     * Get the context item posture
     *
     * @return the posture of the expression that sets the context item
     */

    public Posture getContextItemPosture() {
        if (explicitPosture == Posture.UNKNOWN) {
            Expression contextSetter = getContextSettingExpression();
            if (contextSetter != null) {
                return Streamability.getPosture(contextSetter);
            } else {
                Actor actor = getContainer();
                if (actor instanceof AttributeSet) {
                    return ((AttributeSet)actor).isDeclaredStreamable() ? Posture.STRIDING : Posture.ROAMING;
                } else {
                    return Posture.ROAMING;
                    // TODO: see spec section 19.5
                }
            }
        } else {
            return explicitPosture;
        }
    }

    @Override
    public boolean isStrictStreamabilityRules() {
        return strictStreamabilityRules;
    }

    public void setStrictStreamabilityRules(boolean strictStreamabilityRules) {
        this.strictStreamabilityRules = strictStreamabilityRules;
    }

    /**
     * Indicate that the expression is within the select expression or sequence constructor
     * of an accumulator rule, and whether the rule is for the start or end phase
     * @param phase indicates whether this is the start or end phase, or null to indicate
     *              that the expression is not within an accumulator rule
     */
    public void setAccumulatorPhase(AccumulatorFn.Phase phase) {
        this.accumulatorPhase = phase;
    }

    /**
     * Ask whether the expression is within the select expression or sequence constructor
     * of an accumulator rule, and whether the rule is for the start or end phase
     *
     * @return whether this is the start or end phase, or null to indicate
     *              that the expression is not within an accumulator rule
     */

    public AccumulatorFn.Phase getAccumulatorPhase() {
        return accumulatorPhase;
    }


    /**
     * Default information when nothing else is known
     */

    @CSharpModifiers(code={"public", "static", "readonly", "new"})
    public final static ContextItemStaticInfoEE DEFAULT =
            new ContextItemStaticInfoEE(AnyItemType.getInstance(), true, Posture.STRIDING);

    @CSharpModifiers(code = {"public", "static", "readonly", "new"})
    public final static ContextItemStaticInfoEE ABSENT =
            new ContextItemStaticInfoEE(ErrorType.getInstance(), true, Posture.STRIDING);

}

// Copyright (c) 2018-2022 Saxonica Limited