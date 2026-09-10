package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.MissingComponentException;

import javax.xml.transform.SourceLocator;

/**
 * A reference to a simple or complex type
 */

public class TypeReference extends SchemaStructure implements ComponentReference {

    /**
     * The name of the type object referenced
     */
    private StructuredQName qName;
    private int fingerprint;
    private boolean dangling;

    /**
     * The corresponding SchemaType. This is null until the reference has been resolved.
     */

    private SchemaType target;

    /**
     * Creates a new Type reference
     *
     * @param fingerprint the Name Pool fingerprint of the name of the type being referenced
     * @param config      identifies the XML Schema in which this type reference appears.
     * @param locator     the SourceLocator identifying the location of the reference. May be null if the reference
     *                    is in a built-in type
     */
    public TypeReference(int fingerprint, EnterpriseConfiguration config, SourceLocator locator) {
        this.fingerprint = fingerprint;
        this.qName = config.getNamePool().getStructuredQName(fingerprint);
        if (locator != null) {
            setLocator(locator);
        }
        if (fingerprint < 1024) {
            setTarget(BuiltInType.getSchemaType(fingerprint));
        }
        setConfiguration(config);
    }

    /**
     * Determine whether this reference has been resolved
     */

    public boolean isResolved() {
        return target != null;
    }

    public void setTarget(SchemaType target) {
        this.target = target;
    }

    public int getTargetFingerprint() {
        return fingerprint;
    }

    public StructuredQName getTargetComponentName() {
        return qName;
    }

    /**
     * Ask whether this is known to be a dangling reference
     *
     * @return true if the target of this component reference is known to be absent, and if a warning has
     * been issued to this effect
     */
    public boolean isDangling() {
        return dangling;
    }

    /**
     * Get the symbol space of the target of this reference
     */

    public int getSymbolSpace() {
        return SymbolSpace.TYPE;
    }

    /**
     * Test whether this is a reference to an anonymous type
     *
     * @return true if this SchemaType is an anonymous type
     */

    public boolean isAnonymousType() {
        return getTargetFingerprint() > 1023 && getTargetComponentName().hasURI(NamespaceConstant.ANONYMOUS);
    }


    /**
     * Returns the type that this type reference is a reference to
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException if the reference has not been resolved. Note that
     *                                      this is an unchecked exception.
     */

    /*@NotNull*/
    public SchemaComponent getTarget() throws MissingComponentException {
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        if (target instanceof SimpleTypeDefinition) {
            UserSimpleType ust = ((SimpleTypeDefinition) target).getSimpleType();
            if (ust != null) {
                target = ust;
            }
        }
        return target;
    }

    /**
     * Attempt to resolve this reference using the types defined in a given schema.
     * No error results if the reference cannot be resolved.
     */

    public void tryToResolve(SchemaCompiler compiler) {
        if (!dangling) {
            if (target == null) {
                target = compiler.getPreparedSchema().getType(qName);
            }
            if (target == null) {
                target = getConfiguration().getSuperSchema().getType(qName);
            }
            if (target == null) {
                dangling = true;
            }
        }
    }

}

// Copyright (c) 2004-2015 Saxonica Limited.