package com.saxonica.ee.trans;

import com.saxonica.ee.stream.ComponentInversion;
import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import net.sf.saxon.expr.instruct.TemplateRule;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.UType;

import java.util.List;

/**
 * Saxon-EE version of the TemplateRule class, with added code for JIT compilation and streamability analysis
 */
public class TemplateRuleEE extends TemplateRule {

    private boolean declaredStreamable;
    private TemplateRuleInitializer initializer = null;

    /**
     * Create a template rule
     */

    public TemplateRuleEE() {
    }

    /**
     * Say whether or not this template is declared as streamable
     *
     * @param streamable true if the template belongs to a streamable mode; set to false if it does not belong
     *                   to a streamable mode, or if it is found that the template is not actually streamable, and fallback
     *                   to non-streaming has been requested.
     */

    @Override
    public void setDeclaredStreamable(boolean streamable) {
        this.declaredStreamable = streamable;
    }

    /**
     * Ask whether or not this template is declared as streamable
     *
     * @return true if the template belongs to a streamable mode; false if it does not belong
     * to a streamable mode, or if it is found that the template is not actually streamable, and fallback
     * to non-streaming has been requested.
     */

    @Override
    public boolean isDeclaredStreamable() {
        return declaredStreamable;
    }

    public void setInitializer(TemplateRuleInitializer initializer) {
        this.initializer = initializer;
    }

    public TemplateRuleInitializer getInitializer() {
        return initializer;
    }

    /**
     * Prepare for JIT compilation.
     *
     * @param compilation the XSLT compilation
     * @param decl        the component declaration of this template rule
     */

    @Override
    public void prepareInitializer(Compilation compilation, ComponentDeclaration decl) {
        if (getInitializer() == null) {
            setInitializer(new TemplateRuleInitializer(compilation, decl, this));
        }
    }

    /**
     * Ensure that any first-time initialization has been done. Used in Saxon-EE
     * to do JIT compilation
     *
     * @throws XPathException if any error occurs
     */
    @Override
    public synchronized void initialize() throws XPathException {
        assert !(body == null && initializer == null);
        if (initializer != null) {
            initializer.init();
            initializer = null;
        }
    }

    /**
     * Ask whether this template satisfies the rules for a guaranteed streamable template
     *
     * @param reasons the caller may supply a list which on return will contain explanations
     *                for why the template is not streamable, suitable for inclusion in error messages
     * @return true if the template is within the (Saxon or W3C) definition of guaranteed streamable
     */

    public boolean isActuallyStreamable(List<String> reasons) {
        if (matchPattern == null) {
            if (reasons != null) {
                reasons.add("There is no match pattern");
            }
            return false;
        }
        if (!matchPattern.isMotionless()) {
            if (reasons != null) {
                reasons.add("The match pattern is not motionless");
            }
            return false;
        }
        if (body == null) {
            if (reasons != null) {
                reasons.add("The template has no body");
            }
            return false;
        }
        ContextItemStaticInfoEE info =
                (ContextItemStaticInfoEE)body.getConfiguration().makeContextItemStaticInfo(matchPattern.getItemType(), false);

        if (matchPattern.getItemType().getUType().overlaps(UType.ANY_NODE)) {
            info.setContextPostureStriding();
        } else {
            // This rule isn't in the spec, but it makes sense: if the item matched by the template rule can't be a node,
            // then the result of the template rule must be motionless and grounded
            info.setContextPostureGrounded();
        }
        // The spec says we want the type-adjusted posture and sweep of the template body. But the body
        // expression is already in effect type-adjusted, as a consequence of the type-checking process
        // performed earlier.
        PostureAndSweep s = Streamability.getStreamability(body, info, reasons);
        if (s.getPosture() == Posture.GROUNDED) {
            return true;
        } else {
            reasons.add("The result of the template rule can contain streamed nodes");
            return false;
        }
    }


    /**
     * Generate the inversion of the expression comprising the body of a template rules.
     * Supported in Saxon-EE only
     */


    public void makeInversion() throws XPathException {
        // Create the Jackson inversion of the template
        if (inversion == null) {
//            if (getBody() instanceof TraceExpression) {
//                throw new XPathException(
//                        "Saxon restriction: code that uses streaming cannot be compiled with tracing enabled", SaxonErrorCode.SXST0065, this);
//            }
            ComponentInversion inv = new ComponentInversion(getBody());
            inv.setStackFrame(getStackFrameMap());
            inv.invert(isActuallyStreamable(null));
            inversion = inv;
        }
    }

    private ComponentInversion inversion;

    public ComponentInversion getInversion() throws XPathException {
        if (inversion == null) {
            makeInversion();
        }
        return inversion;
    }



}

// Copyright (c) 2018-2023 Saxonica Limited