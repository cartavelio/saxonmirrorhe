package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;

/**
 * A reference to a named schema group within the content model of a complex type
 */

public class SimpleComponentReference extends SchemaStructure implements ComponentReference {

    /**
     * The name of the SchemaComponent referenced
     */
    private StructuredQName targetComponentName = null;

    /**
     * The fingerprint of the SchemaComponent referenced
     */

    private int targetFingerprint = -1;

    /**
     * Flag set if the target component is known to be absent
     */

    private boolean dangling = false;


    /**
     * The corresponding SchemaComponent. This is null until the reference has been resolved.
     */

    protected SchemaComponent target;

    /**
     * The symbol space representing the kind of schema component that is referenced
     */

    protected int symbolSpace;

    /**
     * Default constructor for use by subclasses
     */

    protected SimpleComponentReference() {
    }

    /**
     * Creates a new SimpleComponentReference definition
     *
     * @param symbolSpace the symbol space for the component reference, for example elements, attributes,
     *                    or types
     * @param name the component name
     * @param config      the configuration to which this component belongs
     */
    public SimpleComponentReference(int symbolSpace, StructuredQName name, EnterpriseConfiguration config) {
        setConfiguration(config);
        this.symbolSpace = symbolSpace;
        setTargetComponentName(name);

    }

    /**
     * Determine whether this reference has been resolved
     */

    @Override
    public boolean isResolved() {
        return target != null;
    }

    public void setTargetComponentName(StructuredQName name) {
        this.targetComponentName = name;
    }

    @Override
    public StructuredQName getTargetComponentName() {
        return targetComponentName;
    }

    public int getTargetFingerprint() {
        if (targetFingerprint == -1) {
            targetFingerprint = getConfiguration().getNamePool().allocateFingerprint(
                    targetComponentName.getNamespaceUri(), targetComponentName.getLocalPart());
        }
        return targetFingerprint;
    }


    /**
     * Get the symbol space of the target of this reference
     */

    @Override
    public int getSymbolSpace() {
        return symbolSpace;
    }

    /**
     * Returns the schema component that this is a reference to
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException if the reference has not been resolved. Note that
     *                                      this is an unchecked exception.
     */

    @Override
    public SchemaComponent getTarget() throws MissingComponentException {
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        return target;
    }

    /**
     * Set the target component
     *
     * @param target the component that this reference points to
     */
    public void setTarget(SchemaComponent target) {
        this.target = target;
    }

    /**
     * Attempt to resolve this reference using the components defined in a given schema.
     * No error results if the reference cannot be resolved.
     */

    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) throws SchemaException {
        if (!dangling) {
            if (target == null) {
                tryToResolve(compiler.getPreparedSchema());
            }
            if (target == null) {
                dangling = true;
            }
        }
    }

    private void tryToResolve(PreparedSchema schema) {
        switch (symbolSpace) {
            case SymbolSpace.ELEMENT:
                //noinspection RedundantCast
                target = (ElementDecl)schema.getElementDecl(getTargetComponentName());
                return;
            case SymbolSpace.ATTRIBUTE:
                //noinspection RedundantCast
                target = (AttributeDecl)schema.getAttributeDecl(getTargetComponentName());
                return;
            default:
                throw new IllegalStateException();
        }
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
}

// Copyright (c) 2011-2023 Saxonica Limited