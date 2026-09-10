package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.MissingComponentException;

import javax.xml.transform.SourceLocator;

/**
 * A reference to an identity constraint
 */

public class IdentityConstraintReference extends SchemaStructure implements ComponentReference {

    /**
     * The name of the constraint referenced
     */
    private StructuredQName targetQName;
    private boolean dangling;

    /**
     * The corresponding IdentityConstraint. This is null until the reference has been resolved.
     */

    private IdentityConstraint target;

    /**
     * The kind of constraint expected as the target. One of
     * {@link StandardNames#XS_UNIQUE}, {@link StandardNames#XS_KEY}, {@link StandardNames#XS_KEYREF}
     */
    private int expectedKind;

    /**
     * Create a new uninitialized IdentityConstraint reference
     * (used during schema model loading)
     */

    public IdentityConstraintReference() {
        // no action
    }

    /**
     * Creates a new IdentityConstraint reference
     *
     * @param targetQName the name of the constraint being referenced
     * @param kind        the name pool fingerprint of the kind of constraint being referenced, e.g. xs:unique
     * @param config      identifies the XML Schema in which this type reference appears. May be null if the reference
     *                    is in a built-in type
     * @param locator     the SourceLocator identifying the location of the reference. May be null if the reference
     *                    is in a built-in type
     */
    public IdentityConstraintReference(StructuredQName targetQName, int kind, EnterpriseConfiguration config, /*@Nullable*/ SourceLocator locator) {
        setConfiguration(config);
        this.targetQName = targetQName;
        this.expectedKind = kind;
        if (locator != null) {
            setLocator(locator);
        }

    }

    /**
     * Get the expected kind of constraint
     *
     * @return one of
     *         {@link StandardNames#XS_UNIQUE}, {@link StandardNames#XS_KEY}, {@link StandardNames#XS_KEYREF}
     */

    public int getExpectedKindOfConstraint() {
        return expectedKind;
    }

    /**
     * Determine whether this reference has been resolved
     */

    @Override
    public boolean isResolved() {
        return target != null;
    }

    /**
     * Ask whether this is known to be a dangling reference
     *
     * @return true if the target of this component reference is known to be absent, and if a warning has
     * been issued to this effect
     */
    @Override
    public boolean isDangling() {
        return dangling;
    }

    /**
     * Set the target of this reference. Caller is responsible for ensuring that the target constraint
     * is of the expected kind
     *
     * @param target the target constraint (to which this is a reference)
     */

    public void setTarget(IdentityConstraint target) {
        this.target = target;
        // following code is necessary when fixing up from SCM
        this.targetQName = target.getConstraintName();
        if (target instanceof Key) {
            expectedKind = StandardNames.XS_KEY;
        } else if (target instanceof Unique) {
            expectedKind = StandardNames.XS_UNIQUE;
        } else if (target instanceof KeyRef) {
            expectedKind = StandardNames.XS_KEYREF;
        }
    }


    /**
     * Get the name of the target of this reference
     *
     * @return the component name of the target component
     */
    @Override
    public StructuredQName getTargetComponentName() {
        return targetQName;
    }

    /**
     * Get the symbol space of the target of this reference
     */

    @Override
    public int getSymbolSpace() {
        return SymbolSpace.IDENTITY_CONSTRAINT;
    }


    /**
     * Returns the identity constraint that this type reference is a reference to
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException
     *          if the reference has not been resolved.
     */

    @Override
    public IdentityConstraint getTarget() throws MissingComponentException {
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        return target;
    }

    /**
     * Attempt to resolve this reference using the constraints defined in a given schema.
     * No error results if the reference cannot be resolved.
     */

    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) {
        if (!dangling) {
            if (target == null) {
                target = compiler.getPreparedSchema().getIdentityConstraint(getTargetComponentName());
            }
            if (target == null) {
                target = getConfiguration().getSuperSchema().getIdentityConstraint(getTargetComponentName());
            }
            if (target == null) {
                dangling = true;
            }
        }
    }

}

// Copyright (c) 2010-2022 Saxonica Limited