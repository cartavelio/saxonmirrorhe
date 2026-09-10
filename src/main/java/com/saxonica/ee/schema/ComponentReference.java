package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaComponent;

/**
 * A reference to a schema component. The reference may have been resolved to
 * a specific component, or may be as yet unresolved. Any attempt to use a reference
 * that has not yet been resolved causes an unchecked exception.
 */

public interface ComponentReference extends Location {

    /**
     * Determine the configuration containing this reference (and its target)
     *
     * @return the Saxon configuration
     */

    EnterpriseConfiguration getConfiguration();

    /**
     * Try to resolve the reference if possible, relative to components present in a given schema.
     * No error occurs if the reference cannot yet be resolved, but the reference is marked as a dangling reference.
     *
     * @param compiler The compiler being used to compile the referring component.
     *                 This is assumed to know about the schema containing the candidate target components for the reference. If
     *                 null, the schema containing the reference is used.
     */

    void tryToResolve(SchemaCompiler compiler);

    /**
     * Ask whether this reference has been resolved
     *
     * @return true if the reference has been resolved to a schema component
     */

    boolean isResolved();

    /**
     * Ask whether this is known to be a dangling reference
     * @return true if the target of this component reference is known to be absent, and if a warning has
     * been issued to this effect
     */

    boolean isDangling();

    /**
     * Get the schema component identified by this reference.
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException if the reference has not been resolved.
     */

    SchemaComponent getTarget() throws MissingComponentException;

    /**
     * Get the name of the target of this reference
     *
     * @return the component name of the target component
     */

    StructuredQName getTargetComponentName();

    /**
     * Get the symbol space of the target of this reference
     *
     * @return an integer code identifying the symbol space
     */

    int getSymbolSpace();
}

// Copyright (c) 2004-2015 Saxonica Limited.


