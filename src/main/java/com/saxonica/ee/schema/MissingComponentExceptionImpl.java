package com.saxonica.ee.schema;

import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.MissingComponentException;

/**
 * This exception occurs when an attempt is made to dereference a reference from one
 * schema component to another, if the target of the reference cannot be found. Note that
 * an unresolved reference is not necessarily an error: a schema containing unresolved
 * references may be used for validation, provided the components containing the
 * unresolved references are not actually used.
 */

public class MissingComponentExceptionImpl extends MissingComponentException {

    // The ComponentReference that could not be resolved
    private final ComponentReference reference;

    public MissingComponentExceptionImpl(ComponentReference ref) {
        super("Unresolved reference to schema component " +
                      (ref.getTargetComponentName() == null ? "" : ref.getTargetComponentName().getDisplayName()));
        this.reference = ref;
    }

    public ComponentReference getComponentReference() {
        return reference;
    }

    @Override
    @CSharpModifiers(code={"public", "override"})
    public String getMessage() {
        try {
            String space = SymbolSpace.getSymbolSpaceName(reference.getSymbolSpace());
            String component = reference.getTargetComponentName().getEQName();
            String module = reference.getSystemId();
            if (module.endsWith("override.xsl")) {
                module = "schema_document_resulting_from_override_processing";
            }
            return "The schema contains an unresolved reference to " + space + ' ' + component +
                    " at line " + reference.getLineNumber() + " in " + reference.getSystemId();
        } catch (Exception e) {
            return super.getMessage();
        }
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
