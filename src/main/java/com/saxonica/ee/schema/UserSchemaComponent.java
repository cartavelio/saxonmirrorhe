package com.saxonica.ee.schema;

import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.MissingComponentException;

import java.util.Stack;

/**
 * A SchemaComponent is an object in a schema.
 * While the schema is being built, a list of such objects is maintained,
 * and on completion, the fixup method is called to resolve forwards references.
 * Subsequently, the validate method is called to invoke validation
 */
public interface UserSchemaComponent extends SchemaComponent {

    /**
     * Check references from this component to other components
     *
     * @param compiler the schema compiler
     * @return true if all is well, false if errors found
     */

    boolean fixup(SchemaCompiler compiler) throws SchemaException;

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     *
     * @param references A stack of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler the schema compiler
     * @throws SchemaException if cycles are found
     */

    void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler)
            throws SchemaException, MissingComponentException;

    /**
     * Checks the validity of this Schema component.
     *
     * @param compiler the schema compiler
     * @return true when this Schema definition is valid, otherwise false.
     * @throws SchemaException if the ErrorListener decides that an error is fatal.
     */

    boolean validate(SchemaCompiler compiler) throws SchemaException;

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     */

    void elaborate(SchemaCompiler compiler) throws SchemaException;


}

// Copyright (c) 2004-2023 Saxonica Limited No warranty.
