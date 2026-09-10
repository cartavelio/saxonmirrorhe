package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;

import java.util.Stack;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;

/**
 * An XML Schema Attribute Group Reference, corresponding to an
 * xs:attributeGroup element with a 'ref' attribute.
 */

public final class AttributeGroupReference extends SchemaStructure
        implements ComponentReference, SchemaComponent {

    /**
     * The name of the attribute group referenced, as a StructuredQName
     */

    private StructuredQName qName;

    /**
     * The declaration of the attribute group that this reference refers to. This
     * is null until the reference has been resolved.
     */

    private AttributeGroupDecl target;

    /**
     * Flag indicating that the target component is known to be absent
     */

    private boolean dangling = false;

    /**
     * Creates a new AttributeGroup reference
     *
     * @param config      the Configuration that this AttributeGroup belongs to.
     * @param target      The name of the referenced
     *                    attribute group (that is, the name contained in the 'ref' attribute)
     */
    public AttributeGroupReference(EnterpriseConfiguration config, StructuredQName target) {
        setConfiguration(config);
        setTargetComponentName(target);
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
     * Get the symbol space of the target of this reference
     */

    @Override
    public int getSymbolSpace() {
        return SymbolSpace.ATTRIBUTE_GROUP;
    }

    /**
     * Set the fingerprint of the target of this reference
     *
     * @param target the the name of the referenced attribute group
     */

    public void setTargetComponentName(StructuredQName target) {
        this.qName = target;
    }

    /**
     * Get the name of the target of this reference
     *
     * @return the component name
     */
    @Override
    public StructuredQName getTargetComponentName() {
        return qName;
    }

    /**
     * Returns the attribute wildcard in the referenced attribute group, if any.
     *
     * @param compiler May be null if used at validation time.
     * @return the the attribute wildcard (xs:anyAttribute element) in the referenced
     *         attribute group, if there is one; otherwise null.
     * @throws MissingComponentException
     *          if the reference to the attribute group
     *          declaration cannot be resolved
     */
    
    AttributeWildcard getAttributeWildcard(SchemaCompiler compiler, Stack<AttributeGroupDecl> stack) {
        //tryToResolve(compiler);
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        if (stack.contains(target)) {
            // XSD 1.1 allows cycles
            return null;
        }
        return target.getAttributeWildcard(compiler, stack);
    }

    /**
     * Fix up references to other elements in the schema.
     *
     * @param compiler the schema compiler
     * @return true if fixup succeeds, false if it fails.
     * @throws SchemaException if a fatal error occurs
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        AttributeGroupDecl decl = (AttributeGroupDecl) PreparedSchema.validateReference(this, compiler, false);
        if (decl == null) {
            setFixupStatus(INVALID);
            return false;
        }
        lookForCycles(new Stack<SchemaComponent>(), compiler);
        setFixupStatus(VALIDATED);
        setValidationStatus(VALIDATED);
        return true;
    }

    /**
     * Checks the validity of this Schema component. For internal use only.
     *
     * @param compiler the schema compiler
     * @return true when this Schema definition is valid, otherwise false.
     */

    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (getFixupStatus() == UNVALIDATED) {
            boolean b = fixup(compiler);
            if (b) {
                setFixupStatus(VALIDATED);
                setValidationStatus(VALIDATED);
                return true;
            } else {
                setFixupStatus(INVALID);
                setValidationStatus(INVALID);
                return false;
            }
        }
        if (getFixupStatus() == INVALID) {
            setValidationStatus(INVALID);
            return false;
        }
        setValidationStatus(VALIDATED);
        return true;
    }

    /**
     * Check for cycles in the attribute group. For internal use only.
     *
     * @param references A list of attribute groups that reference this one,
     *                   directly or indirectly.
     * @param compiler   the schema compiler
     * @throws SchemaException if a circularity is detected.
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException {
        if (compiler.getLanguageVersion() == Configuration.XSD11) {
            // circular attribute groups are not an error in XSD 1.1
            return;
        }
        try {
            if (references.contains(this)) {
                throw new SchemaException("The definition of attribute group " +
                        Err.wrap(getTargetComponentName().getDisplayName()) +
                        " is circular");
            }
            StructuredQName fp = getTargetComponentName();
            if (fp == null) {
                // error already reported
                return;
            }
            AttributeGroupDecl group = compiler.getPreparedSchema().getAttributeGroup(fp);
            if (group != null) {
                references.push(this);
                group.lookForCycles(references, compiler);
                references.pop();
            }
        } catch (SchemaException err) {
            // if a cycle is found, break the reference so we only report it once
            setTargetComponentName(StandardNames.SQ_XS_INVALID_NAME);
            throw err;
        }
    }

    /**
     * Try to resolve the reference if possible, relative to components present in a given schema.
     * If the reference cannot be resolved, the action depends on whether the compiler service allows
     * dangling references to absent components. If it does, we try to patch the schema up with a dummy
     * component (where possible). If not, the method throws an exception
     *
     * @param compiler The compiler being used to compile the referring component.
     * @param fatal true if a failure to resolve is to be treated as fatal
     * @throws SchemaException if the reference cannot be resolved or repaired.
     */

    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) throws SchemaException {
        if (!dangling) {
            PreparedSchema schema = compiler.getPreparedSchema();
            if (target == null) {
                target = schema.getAttributeGroup(getTargetComponentName());
            }
            if (target == null) {
                target = getConfiguration().getSuperSchema().getAttributeGroup(getTargetComponentName());
            }
            if (target == null) {
                if (compiler.isAllowDanglingReferences()) {
                    compiler.warning("Unknown attribute group " + qName.getEQName()
                                             + " (substituting an empty attribute group)", this);
                    dangling = true;
                    target = new AttributeGroupDecl(compiler.getConfiguration());
                    target.addAttributeGroupReference(this);
                    target.setAttributeGroupName(qName);
                } else {
                    throw new SchemaException("Unknown type " + qName.getEQName(), this);
                }
            }
        }
    }

    /**
     * Resolves the attribute group reference
     * Note this method doesn't bind irrevocably to the target attribute group declaration,
     * because this may change following a redefine.
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException if the reference has not been resolved.
     */

    @Override
    public SchemaComponent getTarget() throws MissingComponentException {
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        return target;
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void elaborate(SchemaCompiler compiler) {
        // no action
    }

    @Override
    public Function getComponentAsFunction() {
        // Not supported, because an attribute group reference is not a "true" schema component and is never exported
        throw new UnsupportedOperationException();
    }
}

// Copyright (c) 2004-2022 Saxonica Limited