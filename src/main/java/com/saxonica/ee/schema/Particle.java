package com.saxonica.ee.schema;

import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntHashSet;

/**
 * Represents a Particle as defined by XML Schema. A particle is a component of
 * a content model, consisting of a term and a permitted cardinality range. The
 * term may be an element declaration, a wildcard, or a model group. To allow
 * for content models that have not been fully resolved, we also allow a model
 * group reference as a particle (that is, a reference to a named model group
 * definition).
 */

public abstract class Particle extends SimpleComponentReference implements UserSchemaComponent {

    /**
     * The maximum number of occurrences
     */
    private int maxOccurs = 1;

    /**
     * The minimum number of occurrences
     */
    private int minOccurs = 1;

    /**
     * A particle is vulnerable if (a) it is contained in a particle that allows repetition, and
     * (b) everything both before and after it in its enclosing particle
     * is optional, that is, the input it accepts may be all that its enclosing particle accepts
     */

    private boolean vulnerable = false;

    /**
     * A constant to represent an UNBOUNDED number of occurrences
     */
    public final static int UNBOUNDED = -1;

    /**
     * Returns the maximum number of occurrences that this Particle
     * may appear in the containing content model
     *
     * @return the maximum number of occurrences that this Particle
     *         may appear.
     *         A negative (n &lt; 0) value indicates that the
     *         value is unspecified (i.e. unbounded).
     */

    public final int getMaxOccurs() {
        return maxOccurs;
    }

    /**
     * Returns the minimum number of occurrences that this Particle
     * must appear in the containing content model
     *
     * @return the minimum number of occurrences that this Particle
     *         must appear
     *         A negative (n &lt; 0) value indicates that the value is unspecified.
     */
    public final int getMinOccurs() {
        return minOccurs;
    }

    /**
     * Sets the maximum number of occurrences for this Particle within
     * the containing content model
     *
     * @param maxOccurs the maximum number of occurrences for this Particle, or -1 to indicate
     *                  that the maximum is unbounded
     */

    public final void setMaxOccurs(int maxOccurs) {
        this.maxOccurs = maxOccurs;
    }

    /**
     * Sets the minimum number of occurrences for this Particle within
     * the containing content model
     *
     * @param minOccurs the minimum number of occurrences for this Particle
     */

    public final void setMinOccurs(int minOccurs) {
        this.minOccurs = minOccurs;
    }

    /**
     * Get the term corresponding to this particle
     *
     * @return the corresponding term
     */

    public abstract Term getTerm() throws MissingComponentException;

    /**
     * Mark the particle as being (or not being) vulnerable.
     *
     * @param vulnerable true if the particle is vulnerable.
     *                   A particle is vulnerable if (a) it is contained in a particle that allows repetition, and
     *                   (b) everything both before and after it in its enclosing particle
     *                   is optional, that is, the input it accepts may be all that its enclosing particle accepts.
     */

    public final void setVulnerable(boolean vulnerable) {
        this.vulnerable = vulnerable;
    }

    /**
     * Mark the vulnerable subParticles of this particle
     */

    public void markVulnerableSubParticles() throws MissingComponentException {
        // default is no action
    }

    /**
     * Ask whether the particle is vulnerable.
     *
     * @return true if the particule is marked as vulnerable.
     *         A particle is vulnerable if (a) it is contained in a particle that allows repetition, and
     *         (b) everything both before and after it in its enclosing particle
     *         is optional, that is, the input it accepts may be all that its enclosing particle accepts.
     *         The significance of this concept arises when compiling finite state machines with counters;
     *         counters are simply not used in the case of a content model like (a(2,4))(3,5); marking the
     *         ElementParticle a as vulnerable suppresses the use of counters. Example: MS tests, particlesT014
     */

    public final boolean isVulnerable() {
        return vulnerable;
    }

    /**
     * Compute an XPath cardinality (e.g. ONE_OR_MORE) from an XML Schema cardinality
     *
     * @param minOccurs Minimum occurrences
     * @param maxOccurs Maximum occurrences (-1 = unbounded)
     * @return The XPath cardinality
     */

    public static int computeParticleCardinality(int minOccurs, int maxOccurs) {
        if (maxOccurs == -1 || maxOccurs > 1) {
            if (minOccurs == 1) {
                return StaticProperty.ALLOWS_ONE_OR_MORE;
            } else {
                return StaticProperty.ALLOWS_ZERO_OR_MORE;
            }
        } else if (maxOccurs == 1) {
            if (minOccurs == 0) {
                return StaticProperty.ALLOWS_ZERO_OR_ONE;
            } else {
                return StaticProperty.EXACTLY_ONE;
            }
        } else {
            return 0;
        }
    }

    /**
     * Ask whether the particle contains an element wildcard
     *
     * @return true if the particle is an element wildcard, or contains one
     */

    public abstract boolean containsElementWildcard() throws MissingComponentException;

    /**
     * Find the set of all element particles allowed within the content model of this particle.
     * Ignore wildcards.
     *
     * @param result          an initially-empty set in which the results are accumulated
     * @param ignoreWildcards if true, wildcards are ignored. If false, the result will contain the name
     *                        XS_INVALID, meaning that it is not possible to enumerate
     *                        the elements that can appear in the content.
     * @throws SchemaException if an error is found in the schema
     */

    public abstract void gatherAllPermittedElements(IntHashSet result, boolean ignoreWildcards) throws SchemaException;

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by elementName), and return the cardinality associated with that element particle,
     * that is, the number of times the element can occur within this complex type. The value is one of
     * {@link net.sf.saxon.expr.StaticProperty#EXACTLY_ONE}, {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_ONE},
     * {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_MORE}, {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ONE_OR_MORE},
     * If there is no such particle, return {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO}.
     *
     * @param elementName Identifies the name of the child element within this content model
     * @return the computed cardinality of this child element within the complex type
     */

    public abstract int getElementParticleCardinality(int elementName) throws MissingComponentException;

    /**
     * Determine if empty content is allowed
     *
     * @return true if the particle matches empty content
     * @throws MissingComponentException if the schema contains an unresolved reference to a required component
     */

    public abstract boolean isEmptiable() throws MissingComponentException;

    /**
     * Find an element particle within this part of a content model having a given element name
     * (identified by fingerprint), and return the schema type associated with that element particle.
     * If there is no such particle, return null. If the fingerprint matches an element wildcard,
     * return the type of the global element declaration with the given name if one exists, or AnyType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param fingerprint Identifies the name of the child element within this content model
     * @return the type of the element identified by the given name fingerprint. The consistency
     *         rule "Element Declarations Consistent" ensures that if there are multiple matching declarations,
     *         they have consistent types
     */

    /*@Nullable*/
    public abstract SchemaType getElementParticleType(int fingerprint) throws MissingComponentException;

    /**
     * Check references from this component to other components
     *
     * @param compiler is used for reporting errors
     * @return true if all is well, false if errors found
     */

    @Override
    public abstract boolean fixup(SchemaCompiler compiler) throws SchemaException;

    /**
     * Check the validity (consistency) of this element definition.
     *
     * @param compiler the schema compiler
     * @return true if validation succeeded, false if errors were found.
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is found and the
     *          error listener decides that the error is fatal
     */

    @Override
    public abstract boolean validate(SchemaCompiler compiler) throws SchemaException;

    /**
     * Compile a particle
     *
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if a fatal error occurs
     */

    public abstract void compile(SchemaCompiler compiler) throws SchemaException;

    /**
     * Test whether the particle is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     *
     * @param container the model group containing this particle; null if there is no containing model group
     * @return true if the particle is pointless (for example, a sequence of one item)
     */

    public abstract boolean isPointless(ModelGroup container) throws MissingComponentException;

    /**
     * Serialize this particle to an XML Schema Component Model file
     *
     * @param serializer the object responsible for serialization
     * @throws net.sf.saxon.trans.XPathException if writing the serialized form to the destination fails
     * @throws MissingComponentException if the schema cannot be serialized because some components are missing.
     */

    public abstract void serializeParticle(SchemaModelSerializer serializer) throws XPathException, MissingComponentException;

    @Override
    public FunctionItem getComponentAsFunction() {
        Particle thisParticle = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
                String key = arguments[0].head().getStringValue();
                switch (key) {
                    case "class":
                        return StringValue.bmp("Particle");
                    case "implementation":
                        return new ObjectValue<>(thisParticle);
                    case "min occurs":
                        return new Int64Value(getMinOccurs(), BuiltInAtomicType.NON_NEGATIVE_INTEGER, false);
                    case "max occurs":
                        if (getMaxOccurs() < 0) {
                            return StringValue.bmp("unbounded");
                        } else {
                            return new Int64Value(getMaxOccurs(), BuiltInAtomicType.NON_NEGATIVE_INTEGER, false);
                        }
                    case "term":
                        return ((SchemaComponent) getTerm()).getComponentAsFunction();
                    default:
                        return EmptySequence.getInstance();
                }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
