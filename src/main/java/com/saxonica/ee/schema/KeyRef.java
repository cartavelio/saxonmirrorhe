package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;


/**
 * This class represents an xs:keyRef identity constraint in a schema
 */

public class KeyRef extends IdentityConstraint implements ComponentReference {

    /**
     * The QName of the KeyRef (the name of the key being referred to).
     */

    private StructuredQName targetQName;

    /**
     * The IdentityConstraint referenced (once it has been resolved)
     */

    private IdentityConstraint target;

    /**
     * Flag indicating that the target component is known to be absent
     */

    private boolean dangling = false;


    /**
     * Create a KeyRef object representing an xs:keyref constraints
     */

    public KeyRef() {
    }

    /**
     * Create a new KeyRef Identity-Constraint.
     * @param config the configuration
     * @param refer the name of the referenced key.
     */
    public KeyRef(EnterpriseConfiguration config, StructuredQName refer) {
        setConfiguration(config);
        setTargetComponentName(refer);
    }

    @Override
    public String getConstraintCategory() {
        return "keyref";
    }

    /**
     * Get the name of the target of this reference
     *
     * @return the component name
     */
    @Override
    public StructuredQName getTargetComponentName() {
        return targetQName;
    }

    /**
     * Sets the name of the key being referenced.
     *
     * @param target the fingerprint of the name of the key to reference.
     */

    public void setTargetComponentName(StructuredQName target) {
        this.targetQName = target;
    }

    /**
     * Set the target key or unique constraint
     *
     * @param key the target key definition or unique constraint
     */

    public void setTargetKey(IdentityConstraint key) {
        target = key;
    }

    /**
     * Try to resolve the reference if possible, relative to components present in a given schema.
     * No error occurs if the reference cannot yet be resolved.
     *
     * @param compiler The schema containing the candidate target components for the reference. If
     *                 null, the schema containing the reference is used.
     * @param fatal true if a failure to resolve is to be treated as fatal
     */

    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) {
        if (!dangling) {
            target = compiler.getPreparedSchema().getIdentityConstraint(getTargetComponentName());
            if (target == null) {
                target = compiler.getConfiguration().getUnnamedSchema().getIdentityConstraint(getTargetComponentName());
            }
            if (target == null) {
                dangling = true;
            }
        }
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
     * Get the schema component identified by this reference.
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException if the reference has not been resolved. Note that
     *                                      this is an unchecked exception.
     */

    /*@Nullable*/
    @Override
    public SchemaComponent getTarget() throws MissingComponentException {
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        return target;
    }

    /**
     * Get the symbol space of the target of this reference
     */

    @Override
    public int getSymbolSpace() {
        return SymbolSpace.IDENTITY_CONSTRAINT;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler used for reporting error messages
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
            IdentityConstraint key = (IdentityConstraint) PreparedSchema.validateReference(this, compiler, false);
            if (key == null) {
                setFixupStatus(INVALID);
                return false;
            } else if (key instanceof KeyRef) {
                compiler.error("The key reference " +
                        Err.wrap(getTargetComponentName().getDisplayName()) +
                        " refers to a key reference, not to a key or unique constraint", this);
                target = null;
                setFixupStatus(INVALID);
                return false;
            } else {
                key.fixup(compiler);
            }
        }
        setFixupStatus(VALIDATED);
        return true;
    }

    /**
     * Checks the validity of this Schema defintion.
     *
     * @throws SchemaException when this Schema definition is invalid.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {

        boolean result = super.validate(compiler);
        tryToResolve(compiler, false);
        if (target == null) {
            compiler.error("Identity constraint " +
                    Err.wrap(getTargetComponentName().getDisplayName()) +
                    " is referenced, but has not been declared", this);
            result = false;
        } else {
            if (target instanceof KeyRef) {
                compiler.error("The key reference " +
                        Err.wrap(getTargetComponentName().getDisplayName()) +
                        " refers to a key reference, not to a key or unique constraint", this);
                result = false;
            } else if (getFields().size() != target.getFields().size()) {
                compiler.error("The key reference " +
                        Err.wrap(getTargetComponentName().getDisplayName()) +
                        " has a different number of fields from the identity constraint that it references", this);
                result = false;
            }
        }
        return result;
    }


}

// Copyright (c) 2004-2023 Saxonica Limited